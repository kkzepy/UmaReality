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
        //UmaDatabaseController.persistentPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\";
        //UmaDatabaseController.masterDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\master\\master.mdb";
        //UmaDatabaseController.metaDbPath = "G:\\DMM\\Umamusume\\umamusume_Data\\Persistent\\meta";

        UmaDatabaseController.CreateConnection();
        UmaDatabaseController.Initialize();

        //Test();

        //UmaCharacterAssembler assembler = new UmaCharacterAssembler();

        //GameObject uma = assembler.AssembleCharacter(1001, 0, 0, 1);
        //uma.transform.position = Vector3.zero;
        //uma.transform.rotation = Quaternion.identity;

        
        var llBodyInstance = UmaAssembler.CreateBody(1130, 0);
        var llHeadInstance = UmaAssembler.CreateHead(1130, 0);

        UmaAssembler.ApplyBodyTexture(llBodyInstance, 1130, 0);
        UmaAssembler.ApplyHeadTexture(llHeadInstance, 1130, 0);

        GameObject lucky_lilac = UmaAssembler.Assemble(llBodyInstance, llHeadInstance, UmaAssembler.CreateTail(1), "LuckyLilac");
        lucky_lilac.AddComponent<UmaCharacter>();
        var controller = lucky_lilac.GetComponent<UmaCharacter>();
        controller.AddComponent<Animation>();
        controller.PlayAnimatiion("3d/motion/raceresult/camera/chara/chr1105_00/anm_res_crd110502_001_cam");


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
            if (item.Key.Contains("motion")) 
            {
                log += item.Key + "\n";
            }

        }
        File.WriteAllText("anim.txt", log);
    }

    private void Update()
    {
        //stay_gold.transform.Rotate(Vector3.up, 20 * Time.deltaTime);
    }
}
