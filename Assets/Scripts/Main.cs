using Newtonsoft.Json.Bson;
using PlasticGui.WorkspaceWindow.Items;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Media;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using TMPro;
using Uma;
using Uma;
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

        UmaDatabase.PersistentPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\";
        //UmaDatabase.masterDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\master\\master.mdb";
        //UmaDatabase.metaDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\meta";

        //UmaDatabase.PersistentPath = "E:\\Uma\\Persistent\\";

        /*
        string user = "Rhxxza";

        UmaDatabase.persistentPath = $"C:\\Users\\{user}\\AppData\\LocalLow\\Cygames\\umamusume\\";
        UmaDatabase.masterDbPath = $"C:\\Users\\{user}\\AppData\\LocalLow\\Cygames\\umamusume\\master\\master.mdb";
        UmaDatabase.metaDbPath = $"C:\\Users\\{user}\\AppData\\LocalLow\\Cygames\\umamusume\\meta";
        UmaDatabase.DBKey = "56636B634272377665704162"; //Global DB Key
        GraphicsSettings.renderPipelineAsset = null; // Set to null for global*/

        progressBar.text = "Creating DB Connenctions...";
        UmaDatabase.CreateConnection();
        progressBar.text = "Initializing DB...";
        UmaDatabase.Initialize();
        progressBar.text = "Loading shaders...";
        UmaAssetManager.LoadShaders();
        progressBar.text = "";

        string log = "";

        foreach(var item in UmaDatabase.MetaData.Where(x => x.Key.Contains("camera")))
        {
            log += item.Key + "\n";
        }

        File.WriteAllText("log.txt", log);
    }

    public Texture2D texture;
    public UnityEngine.UI.RawImage displayUI;

    private void Start()
    {
        var bg = Assembler.LoadBackground("bg/bg_0029_06111");
        bg.transform.position = new Vector3(0, 0, -5.7f);
    }

    private void OnGUI()
    {
        /*if (texture != null)
        {
            // Menggambar tekstur di pojok kiri atas layar
            GUI.DrawTexture(new Rect(10, 10, 200, 200), texture, ScaleMode.ScaleToFit);
            GUI.Label(new Rect(10, 215, 200, 20), "Debug: " + texture.name);
        }*/
    }

    void Update()
    {

    }
}
