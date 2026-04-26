using System.Collections.Generic;
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

        string user = "Rhxxza";

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
    }

    void Update()
    {

    }
}
