using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Debug = UnityEngine.Debug;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject stay_gold;
    int i;
    //public GameObject obj;
    void Start()
    {
        UmaDatabaseController.persistentPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\";
        UmaDatabaseController.masterDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\master\\master.mdb";
        UmaDatabaseController.metaDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\meta";

        UmaDatabaseController.CreateConnection();
        UmaDatabaseController.Initialize();

        //Test();

        //UmaCharacterAssembler assembler = new UmaCharacterAssembler();

        //GameObject uma = assembler.AssembleCharacter(1001, 0, 0, 1);
        //uma.transform.position = Vector3.zero;
        //uma.transform.rotation = Quaternion.identity;

        var texLogical = "3d/chara/head/chr1001_00/textures/tex_chr1001_00_face_diff_wet";

        using (var stream = new UmaAssetBundleStream(UmaAssetManager.ResolvePath(texLogical), UmaDatabaseController.MetaData[texLogical].FKey))
        {
            var bundle = AssetBundle.LoadFromStream(stream);
            UnityEngine.Object[] asset = bundle.LoadAllAssets();//<GameObject>().FirstOrDefault();
            Debug.Log($"--- Contents of: {bundle.name} ({asset.Length} items) ---");
            foreach (UnityEngine.Object obj in asset)
            {
                // Logs the name and the System.Type of the object
                Debug.Log($"Name: {obj.name} | Type: {obj.GetType()}");
            }

            bundle.Unload(false); // penting!
            //return Instantiate(head);
        }

        var llBodyInstance = UmaAssembler.CreateBody(1130, 0);
        var llHeadInstance = UmaAssembler.CreateHead(1130, 0);

        UmaAssembler.ApplyBodyTexture(llBodyInstance, 1130, 0);
        UmaAssembler.ApplyHeadTexture(llHeadInstance, 1130, 0);

        GameObject lucky_lilac = UmaAssembler.Assemble(llBodyInstance, llHeadInstance, UmaAssembler.CreateTail(1), "LuckyLilac");

        /*
        GameObject sgbodyInstance = UmaAssembler.CreateBody(1135, 0);

        UmaAssembler.ApplyBodyTexture(sgbodyInstance, 1135, 0);

        stay_gold = UmaAssembler.Assemble(sgbodyInstance, UmaAssembler.CreateHead(1135, 0), UmaAssembler.CreateTail(1), "Stay_Gold");

        stay_gold.transform.position = new Vector3(2, 0, 0);
        */
        

    }


    void Test()
    {
        var entry = UmaDatabaseController.MetaData;

        string log = "";

        foreach (var item in entry)
        {
            if (item.Key.Contains("face") || item.Key.Contains("cheek") || item.Key.Contains("mouth") || item.Key.Contains("eye") || item.Key.Contains("mayu")) 
            {
                log += item.Key + "\n";
            }

        }
        File.WriteAllText("head_tex.txt", log);
    }

    private void Update()
    {
        //stay_gold.transform.Rotate(Vector3.up, 20 * Time.deltaTime);
    }
}
