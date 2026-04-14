using System;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
//using Gallop;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject headInstance;
    int i;
    public Shader shader;
    //public GameObject obj;
    void Start()
    {
        UmaDatabaseController.persistentPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\";
        UmaDatabaseController.masterDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\master\\master.mdb";
        UmaDatabaseController.metaDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\meta";

        UmaDatabaseController.CreateConnection();
        UmaDatabaseController.Initialize();

        LoadShader();
        transform.GetComponent<UmaViewerGlobalShader>().setGlobal();
        Debug.Log(Shader.GetGlobalVector("_MainParam"));
        //GameObject go = new GameObject("_GlobalShader");
        //go.AddComponent<UmaViewerGlobalShader>();
        //Test();

        //UmaCharacterAssembler assembler = new UmaCharacterAssembler();

        //GameObject uma = assembler.AssembleCharacter(1001, 0, 0, 1);
        //uma.transform.position = Vector3.zero;
        //uma.transform.rotation = Quaternion.identity;






        //UmaAssetManager.LoadPrerequistes(bodyLogicalPath, true);

        var id = 1032;
        var costumeId = 0;
        var tailId = 1;

        var bodyLogicalPath = UmaAssetManager.QueryBodyPath(1100, costumeId);
        var headLogicalPath = UmaAssetManager.QueryHeadPath(id, 0);
        var tailLogicalPath = UmaAssetManager.QueryTailPath(tailId);

        UmaAssetManager.PreLoadPrerequistes(bodyLogicalPath);
        UmaAssetManager.PreLoadPrerequistes(headLogicalPath);
        UmaAssetManager.PreLoadPrerequistes(tailLogicalPath);

        Debug.Log(UmaAssetManager.loadedAssets.Count());
        UmaAssetManager.LoadPrerequistes(bodyLogicalPath);
        UmaAssetManager.LoadPrerequistes(headLogicalPath);
        UmaAssetManager.LoadPrerequistes(tailLogicalPath);


        var bodyInstance = UmaAssembler.CreateBody(id, costumeId);
        headInstance = UmaAssembler.CreateHead(id, costumeId);
        //headInstance;
        var tailInstance = UmaAssembler.CreateTail(tailId);

        var _hairTransform = headInstance.transform.Find("M_Hair");

        /*
        string shaderLogicalPath = "shader";
        string shaderPath = UmaAssetManager.ResolvePath(shaderLogicalPath);

        using (var stream = new UmaAssetBundleStream(shaderPath, UmaDatabaseController.MetaData[shaderLogicalPath].FKey))
        {
            var bundle = AssetBundle.LoadFromStream(stream);

            foreach (Renderer r in _hairTransform.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.sharedMaterials)
                {
                    switch (m.shader.name)
                    {
                        case "Gallop/3D/Chara/MultiplyCheek":
                            m.shader = UmaAssetManager.CheekShader;
                            break;
                        case "Gallop/3D/Chara/ToonFace/TSER":
                            m.shader = UmaAssetManager.FaceShader;
                            m.SetFloat("_CylinderBlend", 0.25f);
                            m.SetColor("_RimColor", new Color(0, 0, 0, 0));
                            break;
                        case "Gallop/3D/Chara/ToonEye/T":
                            m.shader = UmaAssetManager.EyeShader;
                            m.SetFloat("_CylinderBlend", 0.25f);
                            break;
                        case "Gallop/3D/Chara/ToonHair/TSER":
                            Debug.Log("found hair shader");
                            m.shader = null;





                            m.shader = bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertoonhairtser.shader") as Shader;
                            //bundle.Unload(false)
                            m.SetFloat("_CylinderBlend", 0.25f);
                            //shader = m.shader;
                            break;
                        case "Gallop/3D/Chara/ToonMayu":
                            m.shader = UmaAssetManager.EyebrowShader;
                            m.renderQueue += 1;
                            break;

                    }
                }
            }

            //bundle.Unload(false);
        }
        */

        

        //UmaAssembler.ApplyOriginalShader(bodyInstance);
        //UmaAssembler.ApplyOriginalShader(headInstance);
        //UmaAssembler.ApplyOriginalShader(tailInstance);

        //UmaAssembler.ApplyBodyTexture(bodyInstance, id, costumeId);
        //UmaAssembler.ApplyHeadTexture(headInstance, id, costumeId);
        //UmaAssembler.ApplyTailTexture(tailInstance, id);

        var uma = UmaAssembler.Assemble(bodyInstance, headInstance, tailInstance);
        uma.AddComponent<UmaCharacter>();


        /*
        GameObject sgbodyInstance = UmaAssembler.CreateBody(1135, 0);

        UmaAssembler.ApplyBodyTexture(sgbodyInstance, 1135, 0);

        stay_gold = UmaAssembler.Assemble(sgbodyInstance, UmaAssembler.CreateHead(1135, 0), UmaAssembler.CreateTail(1), "Stay_Gold");

        stay_gold.transform.position = new Vector3(2, 0, 0);
        */


        Debug.Log($"EyeShader: {UmaAssetManager.EyeShader}");
        Debug.Log($"FaceShader: {UmaAssetManager.FaceShader}");
        Debug.Log($"HairShader: {UmaAssetManager.HairShader}");

    }


    void LoadShader()
    {
        var entry = UmaDatabaseController.MetaData;

        /*
        string log = "";

        foreach (var item in entry)
        {
            if (item.Key.Contains("shader")) 
            {
                log += item.Key + "\n";
            }

        }
        File.WriteAllText("gallop.txt", log);
        */

        string shaderLogicalPath = "shader";
        string shaderPath = UmaAssetManager.ResolvePath(shaderLogicalPath);


        using (var stream = new UmaAssetBundleStream(shaderPath, UmaDatabaseController.MetaData[shaderLogicalPath].FKey))
        {
            var bundle = AssetBundle.LoadFromStream(stream);

            //var obj = bundle.LoadAllAssets();//.FirstOrDefault();
            //body.AddComponent<Gallop.AssetHolder>();

            Debug.Log(bundle.name);
            //foreach (var i in obj)

            UmaAssetManager.EyeShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertooneyet.shader");
            UmaAssetManager.FaceShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertoonfacetser.shader");
            UmaAssetManager.HairShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertoonhairtser.shader");
            UmaAssetManager.AlphaShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/characteralphanolinetoonhairtser.shader");
            UmaAssetManager.CheekShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactermultiplycheek.shader");
            UmaAssetManager.EyebrowShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertoonmayu.shader");
            UmaAssetManager.BodyAlphaShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/characteralphanolinetoontser.shader");
            UmaAssetManager.BodyBehindAlphaShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/characteralphanolinetoonbehindtser.shader");

            bundle.Unload(false); // penting!
            //return Instantiate(body);
        }
        

    }
    
    void ReloadShader()
    {
        foreach (Renderer r in headInstance.GetComponentsInChildren<Renderer>())
        {


            foreach (Material m in r.sharedMaterials)
            {
                if (r.name.Contains("Hair") && r.name.Contains("Alpha"))
                {
                    m.shader = UmaAssetManager.AlphaShader;
                }

                switch (m.shader.name)
                {
                    case "Gallop/3D/Chara/MultiplyCheek":
                        m.shader = UmaAssetManager.CheekShader; ;
                        break;
                    case "Gallop/3D/Chara/ToonFace/TSER":
                        m.shader = UmaAssetManager.FaceShader;
                        m.SetFloat("_CylinderBlend", 0.25f);
                        m.SetColor("_RimColor", new Color(0, 0, 0, 0));
                        break;
                    case "Gallop/3D/Chara/ToonEye/T":
                        m.shader = UmaAssetManager.EyeShader;
                        m.SetFloat("_CylinderBlend", 0.25f);
                        break;
                    case "Gallop/3D/Chara/ToonHair/TSER":
                        m.shader = UmaAssetManager.HairShader;
                        m.SetColor("_Color", Color.white);
                        m.SetFloat("_CylinderBlend", 0.25f);
                        break;
                    case "Gallop/3D/Chara/ToonMayu":
                        m.shader = UmaAssetManager.EyebrowShader;
                        m.renderQueue += 1; //fix eyebrows disappearing sometimes
                        break;
                    default:
                        Debug.Log(m.shader.name);
                        // m.shader = Shader.Find("Nars/UmaMusume/Body");
                        break;
                }

                m.SetFloat("_StencilMask", 1032);
            }
        }
    }

    void Test()
    {
        var entry = UmaDatabaseController.MetaData;

        
        string log = "";

        foreach (var item in entry)
        {
            /*
            if (item.Key.StartsWith(UmaAssetManager.BodyPath) && item.Key.Contains("pfb")) 
            {
                log += item.Key + " : " + item.Value.Prerequisites + "\n";
            }
            */

            if (item.Key.Contains("1100")) 
            {
                log += item.Key + " : " + item.Value.Prerequisites + "\n";
            }

        }
        File.WriteAllText("textures.txt", log);
     
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ReloadShader();//transform.GetComponent<UmaViewerGlobalShader>().setGlobal();
            Debug.Log("Reset global shader");
        }
    }
}
