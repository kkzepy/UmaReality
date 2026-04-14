using System;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
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
        //UmaDatabaseController.persistentPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\";
        //UmaDatabaseController.masterDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\master\\master.mdb";
        //UmaDatabaseController.metaDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\meta";

        UmaDatabaseController.CreateConnection();
        UmaDatabaseController.Initialize();
        UmaAssetManager.LoadShaders();

        //Test();

        
        var id = 1074;
        var costumeId = 0;
        var tailId = 1;

        var bodyLogicalPath = UmaAssetManager.QueryBodyPath(1017, costumeId);
        var headLogicalPath = UmaAssetManager.QueryHeadPath(id, 0);
        var tailLogicalPath = UmaAssetManager.QueryTailPath(tailId);

        UmaAssetManager.PreLoadPrerequistes(bodyLogicalPath);
        UmaAssetManager.PreLoadPrerequistes(headLogicalPath);
        UmaAssetManager.PreLoadPrerequistes(tailLogicalPath);

        UmaAssetManager.LoadPrerequistes(bodyLogicalPath);
        UmaAssetManager.LoadPrerequistes(headLogicalPath);
        UmaAssetManager.LoadPrerequistes(tailLogicalPath);


        var bodyInstance = UmaAssembler.CreateBody(1017, costumeId);
        var headInstance = UmaAssembler.CreateHead(id, costumeId);
        var tailInstance = UmaAssembler.CreateTail(tailId);

        uma = UmaAssembler.Assemble(bodyInstance, headInstance, tailInstance);
        uma.AddComponent<UmaCharacter>();

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
        

        

        //string animLogicalPath = $"3d/motion/event/body/chara/chr{id}_00/anm_eve_chr{id}_00_idle01_loop";
        //string animPath = UmaAssetManager.ResolvePath(animLogicalPath);

        
        using (var stream = new UmaAssetBundleStream(animPath, UmaDatabaseController.MetaData[animLogicalPath].FKey))
        {
            var bundle = AssetBundle.LoadFromStream(stream);
            clip = bundle.LoadAllAssets<AnimationClip>().FirstOrDefault();
            bundle.Unload(false);
        }
        */

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

            if (item.Key.StartsWith("3d/motion/event/body")) 
            {
                log += item.Key + "\n";
            }

        }
        File.WriteAllText("anim.txt", log);
     
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            var controller = uma.GetComponent<UmaCharacter>();
            controller.PlayAnimation(clip);
            Debug.Log("Playing animation");
        }
    }
}
