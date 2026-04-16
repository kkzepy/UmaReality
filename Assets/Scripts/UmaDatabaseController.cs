using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using Debug = UnityEngine.Debug;


public class UmaDatabase
{
    public static string persistentPath = "E:\\Uma\\Persistent\\";
    public static string masterDbPath = "E:\\Uma\\Persistent\\master\\master.mdb";
    public static string metaDbPath = "E:\\Uma\\Persistent\\meta";
    public static string DBKey = "6D5B65336336632554712D73505363386D34377B356370233734532973433633";
    public static string DBBaseKey = "F170CEA4DFCEA3E1A5D8C70BD1000000";
    public static string ABKey = "532B4631E4A7B9473E7CFB";

    public static SqliteConnection mdbConn;

    public static List<CharaEntry> CharaData = new List<CharaEntry>();
    public static List<DataRow> MobCharaData;
    public static List<DataRow> FaceTypeData;
    public static List<DataRow> DressData;
    public static List<DataRow> CharaNameData;

    public static Dictionary<string, UmaDatabaseEntry> MetaData;

    public static string BodyPath = "3d/chara/body/";
    public static string MiniBodyPath = "3d/chara/mini/body/";
    public static string HeadPath = "3d/chara/head/";
    public static string TailPath = "3d/chara/tail/";
    public static string MotionPath = "3d/motion/";
    public static string CharaPath = "3d/chara/";
    public static string EffectPath = "3d/effect/";
    public static string CostumePath = "outgame/dress/";

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
            ReadCharaData(mdbConn);//CharaData = ReadMaster(mdbConn, "SELECT * FROM chara_data C,(SELECT D.'index' charaid,D.'text' charaname FROM text_data D WHERE id like 6) T WHERE C.id like T.charaid            
            
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

    public static void ReadCharaData(SqliteConnection conn, string sql = "SELECT * FROM chara_data C,(SELECT D.'index' charaid,D.'text' charaname FROM text_data D WHERE id like 6) T WHERE C.id like T.charaid")
    {
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
                CharaEntry entry = new CharaEntry();
                for (int i = 0; i < sqlite_datareader.FieldCount; i++)
                {
                    //row[i] = sqlite_datareader[i];
                    if (sqlite_datareader.GetName(i) == "id")
                    {
                        entry.Id = Convert.ToInt32(sqlite_datareader[i]);
                    }

                    if (sqlite_datareader.GetName(i) == "birth_year")
                    {
                        entry.BirthYear = Convert.ToInt32(sqlite_datareader[i]);
                    }

                    if (sqlite_datareader.GetName(i) == "birth_month")
                    {
                        entry.BirthMonth = Convert.ToInt32(sqlite_datareader[i]);
                    }

                    if (sqlite_datareader.GetName(i) == "birth_day")
                    {
                        entry.BirthDay = Convert.ToInt32(sqlite_datareader[i]);
                    }

                    if (sqlite_datareader.GetName(i) == "last_year")
                    {
                        entry.LastYear = Convert.ToInt32(sqlite_datareader[i]);
                    }

                    if (sqlite_datareader.GetName(i) == "sex")
                    {
                        entry.Sex = Convert.ToInt32(sqlite_datareader[i]);
                    }

                    if (sqlite_datareader.GetName(i) == "height")
                    {
                        entry.Height = Convert.ToInt32(sqlite_datareader[i]);
                    }

                    if (sqlite_datareader.GetName(i) == "bust")
                    {
                        entry.Bust = Convert.ToInt32(sqlite_datareader[i]);
                    }

                    if (sqlite_datareader.GetName(i) == "scale")
                    {
                        entry.Scale = Convert.ToInt32(sqlite_datareader[i]);
                    }

                    if (sqlite_datareader.GetName(i) == "skin")
                    {
                        entry.Skin = Convert.ToInt32(sqlite_datareader[i]);
                    }

                    if (sqlite_datareader.GetName(i) == "shape")
                    {
                        entry.Shape = Convert.ToInt32(sqlite_datareader[i]);
                    }

                    if (sqlite_datareader.GetName(i) == "socks")
                    {
                        entry.Socks = Convert.ToInt32(sqlite_datareader[i]);
                    }

                    if (sqlite_datareader.GetName(i) == "tail_model_id")
                    {
                        entry.TailModelId = Convert.ToInt32(sqlite_datareader[i]);
                    }
                }
                CharaData.Add(entry);
            }
        }
    }

    static Dictionary<string, UmaDatabaseEntry> ReadMetaFromEncryptedDb(string dbPath, byte[] keyBytes, int cipherIndex = -1)
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
    static byte[] GenFinalKey(byte[] key)
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


    public static CharaEntry GetCharaEntry(int Id)
    {
        return CharaData.Where(x => x.Id == Id)?.FirstOrDefault();
    }

    public static string QueryBodyPath(int characterId, int costumeId)
    {

        string _costumeId = costumeId.ToString();

        if (_costumeId.Length == 1)
        {
            _costumeId = "0" + _costumeId;
        }

        return $"{BodyPath}bdy{characterId}_{_costumeId}/pfb_bdy{characterId}_{_costumeId}";
    }

    public static string QueryHeadPath(int characterId, int headId)
    {
        string _headId = headId.ToString();

        if (_headId.Length == 1)
        {
            _headId = "0" + _headId;
        }

        return $"{HeadPath}chr{characterId}_{_headId}/pfb_chr{characterId}_{_headId}";
    }

    public static string QueryTailPath(int tailId)
    {
        string _tailId = tailId.ToString();

        if (_tailId.Length < 4)
        {
            _tailId = new string('0', 4 - _tailId.Length) + _tailId;
        }

        return $"{TailPath}tail{_tailId}_00/pfb_tail{_tailId}_00";
    }

    public static string ResolvePath(string logicalPath)
    {
        return MetaData[logicalPath]?.QueryPath();
    }

}