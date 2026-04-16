using UnityEngine;
using TMPro;
using System;

public class UIHandler : MonoBehaviour
{
    public TMP_InputField charaId;
    public TMP_InputField costumeIdField;
    int costumeId = 0;

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
        Debug.Log($"{costumeIdField.text} : {costumeId}");

        CharaIdFieldOnEndEdit();
    }

    public void CharaIdFieldOnEndEdit()
    {
        //f (!charaId) { return; }

        if (!string.IsNullOrEmpty(charaId.text))
        {
            Destroy(Main.uma);

            var chara = UmaDatabase.GetCharaEntry(Convert.ToInt32(charaId.text));

            if (chara == null) { return; }

            //costumeId = 0;

            //var bodyLogicalPath = UmaDatabase.QueryBodyPath(chara.Id, costumeId);
            var headLogicalPath = UmaDatabase.QueryHeadPath(chara.Id, 0);
            var tailLogicalPath = UmaDatabase.QueryTailPath(chara.TailModelId);

            //UmaAssetManager.PreLoadPrerequistes(bodyLogicalPath);
            UmaAssetManager.PreLoadPrerequistes(headLogicalPath);
            UmaAssetManager.PreLoadPrerequistes(tailLogicalPath);

            //UmaAssetManager.LoadPrerequistes(bodyLogicalPath);
            UmaAssetManager.LoadPrerequistes(headLogicalPath);
            UmaAssetManager.LoadPrerequistes(tailLogicalPath);


            var bodyInstance = UmaAssembler.CreateGenericBody(2, 0, 0, 0, 0, 0);//UmaAssembler.CreateBody(chara.Id, costumeId);
            var headInstance = UmaAssembler.CreateHead(chara.Id, 0);
            var tailInstance = UmaAssembler.CreateTail(chara.TailModelId);

            UmaAssembler.ApplyTailTexture(tailInstance, chara.Id, chara.TailModelId);

            Main.uma = UmaAssembler.Assemble(bodyInstance, headInstance, tailInstance);
            Main.uma.AddComponent<UmaCharacter>();

            Debug.Log($"Loaded char: {chara.Id}");

            return;
        }

        Debug.LogWarning($"No input!");
    }
}