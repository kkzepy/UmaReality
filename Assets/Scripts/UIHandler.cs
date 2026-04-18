using Gallop;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler : MonoBehaviour
{
    public TMP_InputField charaId;
    public TMP_InputField costumeIdField;
    public TMP_InputField headIdField;
    public TMP_Text progressBar;
    int costumeId = 0;
    int headId = 0;
    int morphIndex = 0;
    float morphWeight = 0f;

    UmaCharacter controller;
    List<FacialMorph> morphs;

    private void Start()
    {
        controller = Main.uma.GetComponent<UmaCharacter>();
        morphs = controller.FaceDrivenKeyTarget.AllMorphs;
    }

    public void OnButtonClick()
    {
        if (!string.IsNullOrEmpty(costumeIdField.text))
        {
            costumeId = Convert.ToInt32(costumeIdField.text);
        }
        else
        {
            costumeId = 0;
        }

        if (!string.IsNullOrEmpty(headIdField.text))
        {
            headId = Convert.ToInt32(headIdField.text);
        }
        else
        {
            headId = 0;
        }

        CharaIdFieldOnEndEdit();
    }

    public void CharaIdFieldOnEndEdit()
    {
        //f (!charaId) { return; }

        if (!string.IsNullOrEmpty(charaId.text))
        {
            Destroy(Main.uma);

            var chara = UmaDatabase.GetCharaEntry(Convert.ToInt32(charaId.text));

            GameObject root = new GameObject();
            root.name = $"uma_{charaId.text}";
            var umachar = root.AddComponent<UmaCharacter>();

            //int costumeId = 0;
            //int headId = 0;
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

            umachar.FaceDrivenKeyTarget.ChangeMorphWeight(umachar.FaceDrivenKeyTarget.AllMorphs.Where(a => a.name == "Mouth_5_0").FirstOrDefault(), 1);

            root = UmaAssembler.AssembleToExistingRoot(umachar.bodyInstance, umachar.headInstance, umachar.tailInstance, root);

            Main.uma = root;

            controller = umachar;
            morphs = controller.FaceDrivenKeyTarget.AllMorphs;
        }

        Debug.LogWarning($"No input!");
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            controller.FaceDrivenKeyTarget.ChangeMorphWeight(morphs[morphIndex], 0); //resets
            morphIndex++;
            if (morphIndex+1 > morphs.Count) { morphIndex--; return; }
            controller.FaceDrivenKeyTarget.ChangeMorphWeight(morphs[morphIndex], morphWeight);
            Debug.Log($"Morph index: {morphIndex} | {morphs[morphIndex].name} | {morphs[morphIndex].tag}");
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            controller.FaceDrivenKeyTarget.ChangeMorphWeight(morphs[morphIndex], 0); //resets
            morphIndex--;
            if (morphIndex == -1) { morphIndex++; return; }
            controller.FaceDrivenKeyTarget.ChangeMorphWeight(morphs[morphIndex], morphWeight);
            Debug.Log($"Morph index: {morphIndex} | {morphs[morphIndex].name} | {morphs[morphIndex].tag}");
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            morphWeight += 0.1f;
            controller.FaceDrivenKeyTarget.ChangeMorphWeight(morphs[morphIndex], morphWeight);
            Debug.Log($"Morph weight: {morphWeight}");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            morphWeight -= 0.1f;
            controller.FaceDrivenKeyTarget.ChangeMorphWeight(morphs[morphIndex], morphWeight);
            Debug.Log($"Morph weight: {morphWeight}");
        }
    }
}