using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Debug = UnityEngine.Debug;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update

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
    }


    // Update is called once per frame
    void Test()
    {
        var entry = UmaDatabaseController.MetaData;

        string log = "";

        foreach (var item in entry)
        {
            if (item.Key.StartsWith("3d/chara/tail/"))
            {
                //Debug.Log("TRUEE");
                log += item.Key + Environment.NewLine;
            }
            
        }
        File.WriteAllText("log.txt", log);
    }
}
