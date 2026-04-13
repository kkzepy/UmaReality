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
    public GameObject stay_gold;
    int i;
    //public GameObject obj;
    void Start()
    {
        //UmaDatabaseController.persistentPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\";
        //UmaDatabaseController.masterDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\master\\master.mdb";
        //UmaDatabaseController.metaDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\meta";

        UmaDatabaseController.CreateConnection();
        UmaDatabaseController.Initialize();

        Shader();
        //Test();

        //UmaCharacterAssembler assembler = new UmaCharacterAssembler();

        //GameObject uma = assembler.AssembleCharacter(1001, 0, 0, 1);
        //uma.transform.position = Vector3.zero;
        //uma.transform.rotation = Quaternion.identity;

        

        
        //var bodyLogicalPath = UmaAssetManager.QueryBodyPath(1100, 0);
        //var bodyPath = UmaAssetManager.ResolvePath(bodyLogicalPath);

        //UmaAssetManager.LoadPrerequistes(bodyLogicalPath, true);

        var id = 1099+1;
        var costumeId = 0;
        var tailId = 1;

        var bodyInstance = UmaAssembler.CreateBody(id, costumeId);
        UmaAssembler.ApplyFallbackShader(bodyInstance);
        var headInstance = UmaAssembler.CreateHead(id, costumeId);
        UmaAssembler.ApplyFallbackShader(headInstance);
        var tailInstance = UmaAssembler.CreateTail(tailId);
        UmaAssembler.ApplyFallbackShader(tailInstance);


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


    }


    void Shader()
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
        //stay_gold.transform.Rotate(Vector3.up, 20 * Time.deltaTime);
    }
}
