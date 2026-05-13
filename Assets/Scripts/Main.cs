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
        

        //File.WriteAllText("log.txt", log);
    }

    public Texture2D texture;
    public UnityEngine.UI.RawImage displayUI;

    private void Start()
    {
        //3d/chara/body/bdy{costumeIdShort}/pfb_bdy{costumeId}_{height}_{shape}_{bust}  
        //(costume id)_(body_type_sub)_(body_setting)_(height)_(shape)_(bust)  

        /*
        var chara = UmaDatabase.CharaData.FirstOrDefault(x => x.Id == 1007);
        
        int costumeId;
        int bodyTypeSub;
        int bodySetting;

        var dressEntry = UmaDatabase.DressData.FirstOrDefault(x => x.Id == 100302);

        costumeId = Convert.ToInt32(dressEntry.Id.ToString()[^2..]);
        bodyTypeSub = dressEntry.BodyTypeSub;
        bodySetting = dressEntry.BodySetting;

        UmaAssembler.CreateGenericBody(skin : chara.Skin, costumeId : costumeId, bodyTypeSub : bodyTypeSub, bodySetting : bodySetting, height : chara.Height, shape : dressEntry.BodyShape, bust : chara.Bust, socks : chara.Socks);
        

        var chara = UmaDatabase.CharaData.FirstOrDefault(x => x.Id == 1007);

        GameObject root = new();

        var controller = root.AddComponent<Uma.UmaCharacter>();
        controller.charaData = chara;
        controller.InstantiateParts();
        controller.Initialize(Resources.Load<AnimatorOverrideController>("Animations/Face Override Controller"), Resources.Load<Animator>("Animations/Face Controller"));
        controller.InitializePhysics();
        controller.InitializeFaceMorph();
        controller.AssembleParts();*/

        var goLogPath = "bg/bg_0069_00111";
        var goPath = UmaDatabase.ResolvePath(goLogPath);
        var fKey = UmaDatabase.MetaData[goLogPath].FKey;

        using var stream = new UmaAssetBundleStream(goPath, fKey);

        AssetBundle bundle;
        bundle = AssetBundle.LoadFromStream(stream);
        var assets = bundle.LoadAllAssets<Texture2D>();
        texture = assets.FirstOrDefault<Texture2D>();

        foreach(var item in assets)
        {
            Debug.Log(item.GetType().ToString());
        }

        /*displayUI = new UnityEngine.UI.RawImage();
        displayUI.texture = assets.FirstOrDefault<Texture2D>();*/

        /*var go = new GameObject();
        Renderer renderer = go.GetComponent<Renderer>();
        renderer.material.mainTexture = assets.FirstOrDefault<Texture2D>();

        Instantiate(go);*/

        //Debug.Log(goPath);
        //Instantiate(go);
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
