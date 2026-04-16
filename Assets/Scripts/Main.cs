using System.Data;
using System.IO;
using UnityEngine;
//using Gallop;

public class Main : MonoBehaviour
{
    //public Shader shader;
    //public GameObject obj;
    
    public static GameObject uma;
    public static AnimationClip clip;
    
    void Awake()
    {
        //UmaDatabase.persistentPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\";
        //UmaDatabase.masterDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\master\\master.mdb";
        //UmaDatabase.metaDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\meta";

        UmaDatabase.CreateConnection();
        UmaDatabase.Initialize();
        UmaAssetManager.LoadShaders();

        Debug.Log(UmaDatabase.CharaData.Count);
        Debug.Log(UmaAssetManager.QueryAvailableCostumeId(1025));

        //Test();
    }


    void Start()
    {
        int id = 1007;
        int costumeId = 0;
        var chara = UmaDatabase.GetCharaEntry(id);

        var bodyLogicalPath = UmaDatabase.QueryBodyPath(chara.Id, costumeId);
        var headLogicalPath = UmaDatabase.QueryHeadPath(chara.Id, 0);
        var tailLogicalPath = UmaDatabase.QueryTailPath(chara.TailModelId);

        UmaAssetManager.PreLoadPrerequistes(bodyLogicalPath);
        UmaAssetManager.PreLoadPrerequistes(headLogicalPath);
        UmaAssetManager.PreLoadPrerequistes(tailLogicalPath);

        UmaAssetManager.LoadPrerequistes(bodyLogicalPath);
        UmaAssetManager.LoadPrerequistes(headLogicalPath);
        UmaAssetManager.LoadPrerequistes(tailLogicalPath);


        var bodyInstance = UmaAssembler.CreateBody(id, costumeId);
        var headInstance = UmaAssembler.CreateHead(id, 0);
        var tailInstance = UmaAssembler.CreateTail(chara.TailModelId);

        UmaAssembler.ApplyTailTexture(tailInstance, id, chara.TailModelId);

        uma = UmaAssembler.Assemble(bodyInstance, headInstance, tailInstance);
        uma.AddComponent<UmaCharacter>();
        //uma.AddComponent<AnimationLoader>();

        clip = UmaAssetManager.LoadAnim("3d/motion/event/body/chara/chr1001_00/anm_eve_chr1001_00_pdk01_s");
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

            if (item.Key.StartsWith(UmaDatabase.HeadPath)) 
            {
                log += item.Key + "\n";
            }

        }
        File.WriteAllText("head.txt", log);
     
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            var controller = uma.GetComponent<UmaCharacter>();
            controller.ToggleBlush();
            controller.UpBodyReset();
        }
    }
}
