using PlasticGui.WorkspaceWindow.Items;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Media;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
//using Gallop;

public class Main : MonoBehaviour
{
    public static GameObject uma;
    public static AnimationClip clip;
    public TMP_Text progressBar;

    void Awake()
    {
        // Initialization

        //UmaDatabase.persistentPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\";
        //UmaDatabase.masterDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\master\\master.mdb";
        //UmaDatabase.metaDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\meta";

        string user = "harry";

        UmaDatabase.persistentPath = $"C:\\Users\\{user}\\AppData\\LocalLow\\Cygames\\umamusume\\";
        UmaDatabase.masterDbPath = $"C:\\Users\\{user}\\AppData\\LocalLow\\Cygames\\umamusume\\master\\master.mdb";
        UmaDatabase.metaDbPath = $"C:\\Users\\{user}\\AppData\\LocalLow\\Cygames\\umamusume\\meta";
        UmaDatabase.DBKey = "56636B634272377665704162"; //Global DB Key
        GraphicsSettings.renderPipelineAsset = null; // Set to null for global

        progressBar.text = "Creating DB Connenctions...";
        UmaDatabase.CreateConnection();
        progressBar.text = "Initializing DB...";
        UmaDatabase.Initialize();
        progressBar.text = "Loading shaders...";
        UmaAssetManager.LoadShaders();
        progressBar.text = "";

        string motset = "";
        int id = -1;

        foreach (DataRow row in UmaDatabase.CharaMotionSet)
        {
            int currentId = Convert.ToInt32(row[0].ToString().Substring(0, 4));

            if (id == -1)
            {
                id = currentId;
            }
            if (id!=currentId)
            {
                id = currentId;
                motset += $"\n\n{row[0]}, {row[1]}, {row[4]}\n";
                //continue;
            }
            else
            {
                motset += $"{row[0]}, {row[1]}, {row[4]}\n";
            }

        }

        File.WriteAllText("motset.txt", motset);

        string loops = "";

        foreach (string key in UmaDatabase.MetaData.Keys)
        {
            if (key.Contains("_loop"))
            {
                loops += key + "\n";
            }
        }

        File.WriteAllText("loops.txt", loops);
    }

    void Update()
    {

    }
}
