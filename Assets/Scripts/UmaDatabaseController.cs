using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using Debug = UnityEngine.Debug;


public class UmaDatabaseController
{
    public static string persistentPath = "E:\\Uma\\Persistent\\";
    public static string masterDbPath = "E:\\Uma\\Persistent\\master\\master.mdb";
    public static string metaDbPath = "E:\\Uma\\Persistent\\meta";
    public static string DBKey = "6D5B65336336632554712D73505363386D34377B356370233734532973433633";
    public static string DBBaseKey = "F170CEA4DFCEA3E1A5D8C70BD1000000";
    public static string ABKey = "532B4631E4A7B9473E7CFB";

    public static SqliteConnection mdbConn;

    public static List<DataRow> CharaData;
    public static List<DataRow> MobCharaData;
    public static List<DataRow> FaceTypeData;
    public static List<DataRow> DressData;
    public static List<DataRow> CharaNameData;

    public static Dictionary<string, UmaDatabaseEntry> MetaData;

    public static void CreateConnection()
    {
        try
        {
            mdbConn = new SqliteConnection($"Data Source={masterDbPath};");
        }
        catch (Exception e)
        {
            Debug.LogError("Error creating database connections: " + e);
            throw;
        }
    }

    public static void Initialize()
    {
        try
        {
            mdbConn.Open();
            CharaData = ReadMaster(mdbConn, "SELECT * FROM chara_data C,(SELECT D.'index' charaid,D.'text' charaname FROM text_data D WHERE id like 6) T WHERE C.id like T.charaid");
            MobCharaData = ReadMaster(mdbConn, "SELECT * FROM mob_data M,(SELECT D.'index' charaid,D.'text' charaname FROM text_data D WHERE id like 59) T WHERE M.mob_id like T.charaid");
            //FaceTypeData = ReadMaster(mdbConn, "SELECT * FROM face_type");
            DressData = ReadMaster(mdbConn, "SELECT * FROM dress_data C,(SELECT D.'index' dressid,D.'text' dressname FROM text_data D WHERE id like 14) T WHERE C.id like T.dressid");
            CharaNameData = ReadMaster(mdbConn, "SELECT * FROM text_data WHERE id = 372");

            MetaData = ReadMetaFromEncryptedDb(metaDbPath, GenFinalKey(Utility.HexStringToBytes(DBKey)), 3);
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading data: " + e);
            throw;
        }
    }

    public static DataRow ReadMobHairColor(string colorid)
    {
        var results = ReadMaster(mdbConn, $"SELECT * FROM mob_hair_color_set WHERE id LIKE {colorid}");
        foreach (var data in results)
        {
            return data;
        }
        return null;
    }

    public static List<DataRow> ReadMaster(SqliteConnection conn, string sql)
    {
        List<DataRow> dr = new List<DataRow>();

        using (var sqlite_cmd = conn.CreateCommand())
        {
            sqlite_cmd.CommandText = sql;
            using var sqlite_datareader = sqlite_cmd.ExecuteReader();
            var result = new DataTable();
            for (int i = 0; i < sqlite_datareader.FieldCount; i++)
            {
                result.Columns.Add(sqlite_datareader.GetName(i), sqlite_datareader.GetFieldType(i));
            }
            while (sqlite_datareader.Read())
            {
                DataRow row = result.NewRow();
                for (int i = 0; i < sqlite_datareader.FieldCount; i++)
                {
                    row[i] = sqlite_datareader[i];
                }
                dr.Add(row);
            }
        }
        return dr;
    }

    public static Dictionary<string, UmaDatabaseEntry> ReadMetaFromEncryptedDb(string dbPath, byte[] keyBytes, int cipherIndex = -1)
    {
        var meta = new Dictionary<string, UmaDatabaseEntry>(StringComparer.Ordinal);
        IntPtr db = IntPtr.Zero;

        try
        {
            db = Sqlite3MC.Open(dbPath);

            if (cipherIndex >= 0)
            {
                try
                {
                    int cfgRc = Sqlite3MC.MC_Config(db, "cipher", cipherIndex);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"MC_Config thrown: {e}");
                }
            }

            int rcKey = Sqlite3MC.Key_SetBytes(db, keyBytes);
            if (rcKey != Sqlite3MC.SQLITE_OK)
            {
                string em = Sqlite3MC.GetErrMsg(db);
                throw new InvalidOperationException($"sqlite3_key returned rc={rcKey}, errmsg={em}");
            }

            if (!Sqlite3MC.ValidateReadable(db, out string validateErr))
            {
                Console.WriteLine($"DB validation after key failed: {validateErr}");
                throw new InvalidOperationException("DB validation after key failed: " + validateErr);
            }

            

            string sql = "SELECT m,n,h,c,d,e FROM a";
            Sqlite3MC.ForEachRow(sql, db, (stmt) =>
            {
                try
                {
                    string m = Sqlite3MC.ColumnText(stmt, 0);
                    string n = Sqlite3MC.ColumnText(stmt, 1);
                    string h = Sqlite3MC.ColumnText(stmt, 2);
                    string c = Sqlite3MC.ColumnText(stmt, 3);
                    string d = Sqlite3MC.ColumnText(stmt, 4);
                    long e = Sqlite3MC.ColumnInt64(stmt, 5);

                    if (string.IsNullOrEmpty(m))
                    {
                        Console.WriteLine("Skipping row: empty type string (m).");
                        return;
                    }

                    if (!Enum.TryParse<UmaFileType>(m, /*ignoreCase*/ false, out UmaFileType type))
                    {
                        Console.WriteLine($"Unrecognized EntryType Enum Value :{m}");
                        return;
                    }

                    if (string.IsNullOrEmpty(n))
                    {
                        Console.WriteLine($"Invalid entry name '{n}' or URL '{h}'. Skipping row.");
                        return;
                    }

                    var entry = new UmaDatabaseEntry
                    {
                        Type = type,
                        Name = n,
                        Url = h,
                        Checksum = c,
                        Prerequisites = d,
                        Key = e
                    };

                    if (!meta.ContainsKey(entry.Name))
                    {
                        meta.Add(entry.Name, entry);
                    }
                }
                catch (Exception exRow)
                {
                    Console.WriteLine("Error caught while reading row: " + exRow);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("ReadMetaFromEncryptedDb failed: " + ex);
            throw;
        }
        finally
        {
            if (db != IntPtr.Zero)
            {
                try { Sqlite3MC.Close(db); }
                catch (Exception e) { Console.WriteLine("Closing DB failed: " + e); }
            }
        }

        return meta;
    }

    

    public static byte[] GenFinalKey(byte[] key)
    {
        byte[] baseKey = Utility.HexStringToBytes(DBBaseKey);
        if (baseKey.Length < 13)
            throw new Exception("Invalid Base Key length");
        for (int i = 0; i < key.Length; i++)
        {
            key[i] = (byte)(key[i] ^ baseKey[i % 13]);
        }
        return key;
    }

}