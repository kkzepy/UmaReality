using Mono.Data.Sqlite;
using PlasticGui.Configuration.CloudEdition.Welcome;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace Uma
{
    public class UmaDatabase
    {
        public static string persistentPath = "E:\\Uma\\Persistent\\";
        public static string masterDbPath = "E:\\Uma\\Persistent\\master\\master.mdb";
        public static string metaDbPath = "E:\\Uma\\Persistent\\meta";
        public static string DBKey = "6D5B65336336632554712D73505363386D34377B356370233734532973433633";
        public static string DBBaseKey = "F170CEA4DFCEA3E1A5D8C70BD1000000";
        public static string ABKey = "532B4631E4A7B9473E7CFB";

        public static SqliteConnection mdbConn;

        public static List<CharaData> CharaData = new List<CharaData>();
        //public static List<DataRow> MobCharaData;
        public static List<FaceTypeData> FaceTypeData;
        public static List<DressData> DressData;
        //public static List<DataRow> CharaNameData;
        public static List<CharaMotionSet> CharaMotionSet = new();

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

                //MobCharaData = ReadMaster(mdbConn, "SELECT * FROM mob_data M,(SELECT D.'index' charaid,D.'text' charaname FROM text_data D WHERE id like 59) T WHERE M.mob_id like T.charaid");
                FaceTypeData = ReadFaceTypeData(mdbConn);
                //DressData = ReadMaster(mdbConn, "SELECT * FROM dress_data C,(SELECT D.'index' dressid,D.'text' dressname FROM text_data D WHERE id like 14) T WHERE C.id like T.dressid");
                //CharaNameData = ReadMaster(mdbConn, "SELECT * FROM text_data WHERE id = 372");
                ReadCharaMotionSet(mdbConn);//CharaMotionSet = ReadMaster(mdbConn, "SELECT * FROM chara_motion_set WHERE id > 1001000 AND id < 9100125;");
                DressData = ReadDressData(mdbConn);

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
                    CharaData entry = new CharaData();
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
        public static List<DressData> ReadDressData(SqliteConnection connection)
        {
            var dressList = new List<DressData>();
            string query = "SELECT * FROM dress_data WHERE id > 1001";

            using (var command = new SqliteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = new DressData
                        {
                            Id = reader.GetInt32(0),
                            ConditionType = reader.GetInt32(1),
                            HaveMini = reader.GetInt32(2),
                            GeneralPurpose = reader.GetInt32(3),
                            CostumeType = reader.GetInt32(4),
                            CharaId = reader.GetInt32(5),
                            UseGender = reader.GetInt32(6),
                            BodyShape = reader.GetInt32(7),
                            BodyType = reader.GetInt32(8),
                            BodyTypeSub = reader.GetInt32(9),
                            BodySetting = reader.GetInt32(10),
                            UseRace = reader.GetInt32(11),
                            UseLive = reader.GetInt32(12),
                            UseLiveTheater = reader.GetInt32(13),
                            UseHome = reader.GetInt32(14),
                            UseDressChange = reader.GetInt32(15),
                            IsWet = reader.GetInt32(16),
                            IsDirt = reader.GetInt32(17),
                            HeadSubId = reader.GetInt32(18),
                            UseSeason = reader.GetInt32(19),
                            DressColorMain = reader.GetString(20),
                            DressColorSub = reader.GetString(21),
                            ColorNum = reader.GetInt32(22),
                            DispOrder = reader.GetInt32(23),
                            TailModelId = reader.GetInt32(24),
                            TailModelSubId = reader.GetInt32(25),
                            MiniMayuShaderType = reader.GetInt32(26),
                            StartTime = reader.GetInt32(27),
                            EndTime = reader.GetInt32(28)
                        };

                        dressList.Add(data);
                    }
                }
            }

            return dressList;
        }
        public static void ReadCharaMotionSet(SqliteConnection conn, string sql = "SELECT * FROM chara_motion_set WHERE id > 1001000 AND id < 9100125;")
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
                    CharaMotionSet entry = new();
                    
                    for (int i = 0; i < sqlite_datareader.FieldCount; i++)
                    {
                        //row[i] = sqlite_datareader[i];
                        if (sqlite_datareader.GetName(i) == "id")
                        {
                            entry.Id = Convert.ToInt32(sqlite_datareader[i]);
                        }

                        if (sqlite_datareader.GetName(i) == "body_motion")
                        {
                            entry.BodyMotion = sqlite_datareader[i].ToString();
                        }

                        if (sqlite_datareader.GetName(i) == "body_motion_type")
                        {
                            entry.BodyMotionType = Convert.ToInt32(sqlite_datareader[i]);
                        }

                        if (sqlite_datareader.GetName(i) == "body_motion_path_segment")
                        {
                            entry.BodyMotionPathSegment = Convert.ToInt32(sqlite_datareader[i]);
                        }

                        if (sqlite_datareader.GetName(i) == "body_motion_play_type")
                        {
                            entry.BodyMotionPlayType = Convert.ToInt32(sqlite_datareader[i]);
                        }

                        if (sqlite_datareader.GetName(i) == "face_type")
                        {
                            entry.FaceType = sqlite_datareader[i].ToString();
                        }

                        if (sqlite_datareader.GetName(i) == "eye_default")
                        {
                            entry.EyeDefault = Convert.ToInt32(sqlite_datareader[i]);
                        }

                        if (sqlite_datareader.GetName(i) == "ear_motion")
                        {
                            entry.EarMotion = sqlite_datareader[i].ToString();
                        }

                        if (sqlite_datareader.GetName(i) == "tail_motion")
                        {
                            entry.TailMotion = sqlite_datareader[i].ToString();
                        }

                        if (sqlite_datareader.GetName(i) == "tail_motion_type")
                        {
                            entry.TailMotionType = Convert.ToInt32(sqlite_datareader[i]);
                        }
                    }
                    CharaMotionSet.Add(entry);
                }
            }
        }

        public static List<FaceTypeData> ReadFaceTypeData(SqliteConnection conn)
        {
            List<FaceTypeData> data = new List<FaceTypeData>();
            SqliteCommand sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM face_type_data";
            SqliteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                FaceTypeData entry = new FaceTypeData()
                {
                    label = sqlite_datareader.GetString(0),
                    eyebrow_l = sqlite_datareader.GetString(1),
                    eyebrow_r = sqlite_datareader.GetString(2),
                    eye_l = sqlite_datareader.GetString(3),
                    eye_r = sqlite_datareader.GetString(4),
                    mouth = sqlite_datareader.GetString(5),
                    mouth_shape_type = sqlite_datareader.GetInt32(6),
                    inverce_face_type = sqlite_datareader.GetString(7),
                    set_face_group = sqlite_datareader.GetInt32(8),
                };
                data.Add(entry);
            }
            return data;
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

                        if (!Enum.TryParse(m, /*ignoreCase*/ false, out UmaFileType type))
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


        public static CharaData GetCharaEntry(int Id)
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
            //return MetaData[logicalPath]?.QueryPath();

            if (MetaData.TryGetValue(logicalPath, out var path))
            {
                return path.QueryPath();
            }
            else
            {
                return null;
            }
        }

    }
}