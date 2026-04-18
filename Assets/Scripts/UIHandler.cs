using UnityEngine;
using TMPro;
using System;
using Gallop;

public class UIHandler : MonoBehaviour
{
    public TMP_InputField charaId;
    public TMP_InputField costumeIdField;
    public TMP_InputField headIdField;
    public TMP_Text progressBar;
    int costumeId = 0;
    int headId = 0;

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

            if (chara == null) { return; }

            /*
            //costumeId = 0;

            var bodyLogicalPath = UmaDatabase.QueryBodyPath(chara.Id, costumeId);
            var headLogicalPath = UmaDatabase.QueryHeadPath(chara.Id, headId);
            var tailLogicalPath = UmaDatabase.QueryTailPath(chara.TailModelId);

            UmaAssetManager.PreLoadPrerequistes(bodyLogicalPath);
            UmaAssetManager.PreLoadPrerequistes(headLogicalPath);
            UmaAssetManager.PreLoadPrerequistes(tailLogicalPath);

            UmaAssetManager.LoadPrerequistes(bodyLogicalPath);
            UmaAssetManager.LoadPrerequistes(headLogicalPath);
            UmaAssetManager.LoadPrerequistes(tailLogicalPath);


            var bodyInstance = UmaAssembler.CreateBody(chara.Id, costumeId);
            var headInstance = UmaAssembler.CreateHead(chara.Id, headId);
            var tailInstance = UmaAssembler.CreateTail(chara.TailModelId);

            UmaAssembler.ApplyTailTexture(tailInstance, chara.Id, chara.TailModelId);

            Main.uma = UmaAssembler.Assemble(bodyInstance, headInstance, tailInstance);
            Main.uma.AddComponent<UmaCharacter>().SetAssetHolder(bodyInstance.GetComponent<AssetHolder>());
            //Main.uma.AddComponent<AnimationLoader>();

            Debug.Log($"Loaded char: {chara.Id}");
            */

            Main.uma = UmaAssembler.CreateCharacter(chara, costumeId, headId);
            var controller = Main.uma.GetComponent<UmaCharacter>();
            controller.LoadPhysics();

            return;
        }

        Debug.LogWarning($"No input!");
    }
}