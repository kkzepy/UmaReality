using JetBrains.Annotations;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
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

        var entry = UmaDatabaseController.MetaData;

        var res = UmaAssetManager.QueryBodyPath(1001, 0);

        


        
        int count = 0;
        string log = "";

        foreach (var item in entry)
        {
            var key = item.Key;
            var value = item.Value;
            if (!value.Name.Contains("1001"))
            {
                continue;
            }

            log += value.Name + " : " + value.QueryPath()+"\n";
            Debug.Log(log);
            count++;
        }
        Debug.Log($"Total: {count}");
        File.WriteAllText("log.txt", log);

        Debug.Log(res);
        Debug.Log($"Exists?: {UmaDatabaseController.MetaData.ContainsKey(res)}");
        
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
