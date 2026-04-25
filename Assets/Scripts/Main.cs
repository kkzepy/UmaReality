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
    //public Shader shader;
    //public GameObject obj;
    
    public static GameObject uma;
    public static AnimationClip clip;
    public TMP_Text progressBar;
    //public List<UmaDatabaseEntry> Sounds = new List<UmaDatabaseEntry>();

    void Awake()
    {
        //UmaDatabase.persistentPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\";
        //UmaDatabase.masterDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\master\\master.mdb";
        //UmaDatabase.metaDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\meta";

        UmaDatabase.persistentPath = "C:\\Users\\harry\\AppData\\LocalLow\\Cygames\\umamusume\\";
        UmaDatabase.masterDbPath = "C:\\Users\\harry\\AppData\\LocalLow\\Cygames\\umamusume\\master\\master.mdb";
        UmaDatabase.metaDbPath = "C:\\Users\\harry\\AppData\\LocalLow\\Cygames\\umamusume\\meta";
        UmaDatabase.DBKey = "56636B634272377665704162";
        GraphicsSettings.renderPipelineAsset = null;
        
        progressBar.text = "Creating DB Connenctions...";
        UmaDatabase.CreateConnection();
        progressBar.text = "Initializing DB...";
        UmaDatabase.Initialize();
        progressBar.text = "Loading shaders...";
        UmaAssetManager.LoadShaders();
        progressBar.text = "";

        //var aaaa = 
        List<UmaDatabaseEntry> Sounds = UmaDatabase.MetaData.Where(ab => ab.Value.Type == UmaFileType.sound).Select(ab => ab.Value).ToList();


        //Test();

        string log = "";

        foreach (var sound in Sounds)
        {
            log += sound.Name + "\n";
        }
        File.WriteAllText("acbawb.txt", log);
        
    }


    void AStart()
    {
        var chara = UmaDatabase.GetCharaEntry(1100);

        //var head = UmaAssembler.CreateHead(1006, 0, true);

        GameObject root = new GameObject();
        root.name = "Uma";
        var umachar = root.AddComponent<UmaCharacter>();

        int costumeId = 0;
        int headId = 0;
        umachar.charaEntry = chara;
        umachar.costumeId = costumeId;
        umachar.headId = headId;
        umachar.FaceOverrideController = Resources.Load<AnimatorOverrideController>("Animations/Face Override Controller");

        var bodyLogicalPath = UmaDatabase.QueryBodyPath(chara.Id, costumeId);
        var headLogicalPath = UmaDatabase.QueryHeadPath(chara.Id, headId);
        var tailLogicalPath = UmaDatabase.QueryTailPath(chara.TailModelId);

        UmaAssetManager.PreLoadPrerequistes(bodyLogicalPath);
        UmaAssetManager.PreLoadPrerequistes(headLogicalPath);
        UmaAssetManager.PreLoadPrerequistes(tailLogicalPath);

        UmaAssetManager.LoadPrerequistes(bodyLogicalPath);
        UmaAssetManager.LoadPrerequistes(headLogicalPath);
        UmaAssetManager.LoadPrerequistes(tailLogicalPath);

        umachar.bodyInstance = UmaAssembler.CreateBody(chara.Id, 0, false, root);
        umachar.headInstance = UmaAssembler.CreateHead(chara.Id, headId, false, root);
        umachar.tailInstance = UmaAssembler.CreateTail(chara.TailModelId, false, root);
        UmaAssembler.ApplyTailTexture(umachar.tailInstance, chara.Id);

        umachar.Initialize();
        umachar.LoadPhysics();
        umachar.SetupPhysics();
        umachar.InitializeFaceMorph();

        //umachar.FaceDrivenKeyTarget.ChangeMorphWeight(umachar.FaceDrivenKeyTarget.AllMorphs.Where(a => a.name == "Mouth_5_0").FirstOrDefault(), 1);

        /*
        string log = "";

        foreach (FacialMorph morph in umachar.FaceDrivenKeyTarget.AllMorphs)
        {
            log += $"{morph.name} : {morph.tag}\n";
        }

        File.WriteAllText("morphs.txt", log);
        */

        root = UmaAssembler.AssembleToExistingRoot(umachar.bodyInstance, umachar.headInstance, umachar.tailInstance, root);

        uma = root;

        clip = UmaAssetManager.LoadAsset<AnimationClip>("3d/motion/event/body/chara/chr1001_00/anm_eve_chr1001_00_pdk01_s");
        Debug.Log(UmaDatabase.MetaData["3d/motion/event/body/chara/chr1001_00/anm_eve_chr1001_00_pdk01_s"].Prerequisites);
    }

    void Test()
    {
        var entry = UmaDatabase.MetaData;

        
        string log = "";

        foreach (var item in entry)
        {
            /*
            if (item.Key.StartsWith(UmaAssetManager.BodyPath) && item.Key.Contains("pfb")) 
            {
                log += item.Key + " : " + item.Value.Prerequisites + "\n";
            }
            */

            if (item.Key.Contains("sound") || item.Key.Contains("voice") || item.Key.Contains("awb") || item.Key.Contains("acb"))
            {
                log += item.Value.Name + " : " + item.Value.Prerequisites + "\n";
            }

        }
        File.WriteAllText("acbawb.txt", log);
     
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            var controller = uma.GetComponent<UmaCharacter>();
            //if (controller.transform.Find("M_Cheek").gameObject.activeSelf)
            {
                controller.SetSmile(true, 1f, .14f, true, true, false);
                controller.SetRandomBlink(false);
            }
            /*else
            {
                controller.SetSmile(false, 1f, 1f, true, true, true);
                controller.SetRandomBlink(true);
            }*/
            //controller.UpBodyReset();
        }
    }
}
