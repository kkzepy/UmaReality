using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class Main : MonoBehaviour
{
    // Start is called before the first frame update

    //public GameObject obj;
    void Start()
    {
        UmaDatabaseController.CreateConnection();
        UmaDatabaseController.Initialize();

        Debug.Log("CharaData count: " + UmaDatabaseController.CharaData.Count);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
