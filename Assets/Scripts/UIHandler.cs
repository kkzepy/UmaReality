using Gallop;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class UIHandler : MonoBehaviour
{
    public TMP_InputField charaId;
    public TMP_InputField costumeIdField;
    public TMP_InputField headIdField;
    public TMP_Text progressBar;
    public TMP_InputField animField;
    public GameObject Dialogue;
    public TMP_InputField Chat;

    public GameObject uma;

    int costumeId = 0;
    int headId = 0;
    int morphIndex = 0;
    float morphWeight = 0f;

    UmaCharacter controller;
    List<FacialMorph> morphs;

    ChatController chatController;
    ExpressionVocab expVoc;
    ExpressiveController expCon;
    string prevMorph;
    public string APIKey;
    public string Model = "meta-llama/llama-4-scout-17b-16e-instruct";
    public string DefinitionPath="bots/tokai_teio.json";

    private void Start()
    {
        chatController = new ChatController();

        chatController.LoadUserDefinition("bots/persona.json");
        chatController.LoadBotDefinition(DefinitionPath);
        Dialogue.GetComponent<DialogueController>().DialogueTitle.text = chatController.bot.name;

        chatController.EndpointURL = "https://api.groq.com/openai/v1/chat/completions";//"https://openrouter.ai/api/v1/chat/completions";
        chatController.rules = File.ReadAllText("bots/rules.txt");
        chatController.format = File.ReadAllText("bots/format.txt");
        chatController.LoadExpressionVocab("bots/expression_dict.json");
        expVoc = chatController.expressionVocab;

        //var motsets = ExpressiveController.GetCharacterMotionSets(1003);
        /*foreach (var item in motsets)
        {
            Debug.Log($"{item.Key} : {item.Value.Count} : {item.Value[0]}");
        }*/
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
            if (uma) Destroy(uma);

            var chara = UmaDatabase.GetCharaEntry(Convert.ToInt32(charaId.text));

            /*
            GameObject root = new GameObject();
            root.name = $"uma_{charaId.text}";
            var umachar = root.AddComponent<UmaCharacter>();

            //int costumeId = 0;
            //int headId = 0;
            umachar.charaEntry = chara;
            umachar.costumeId = costumeId;
            umachar.headId = headId;
            umachar.FaceOverrideController = Resources.Load<AnimatorOverrideController>("Animations/Face Override Controller");
            umachar.UmaFaceAnimator = Resources.Load<Animator>("Animations/Face Controller");

            var bodyLogicalPath = UmaDatabase.QueryBodyPath(chara.Id, costumeId);
            var headLogicalPath = UmaDatabase.QueryHeadPath(chara.Id, headId);
            var tailLogicalPath = UmaDatabase.QueryTailPath(chara.TailModelId);

            UmaAssetManager.PreLoadPrerequistes(bodyLogicalPath);
            UmaAssetManager.PreLoadPrerequistes(headLogicalPath);
            if (chara.TailModelId != -1) UmaAssetManager.PreLoadPrerequistes(tailLogicalPath);

            UmaAssetManager.LoadPrerequistes(bodyLogicalPath);
            UmaAssetManager.LoadPrerequistes(headLogicalPath);
            if (chara.TailModelId != -1) UmaAssetManager.LoadPrerequistes(tailLogicalPath);

            umachar.bodyInstance = UmaAssembler.CreateBody(chara.Id, costumeId, false, root);
            umachar.headInstance = UmaAssembler.CreateHead(chara.Id, headId, false, root);
            if (chara.TailModelId != -1)
            {
                umachar.tailInstance = UmaAssembler.CreateTail(chara.TailModelId, false, root);
                UmaAssembler.ApplyTailTexture(umachar.tailInstance, chara.Id);
            }

            umachar.Initialize();
            umachar.LoadPhysics();
            umachar.SetupPhysics();
            umachar.InitializeFaceMorph();

            //umachar.FaceDrivenKeyTarget.ChangeMorphWeight(umachar.FaceDrivenKeyTarget.AllMorphs.Where(a => a.name == "Mouth_5_0").FirstOrDefault(), 1);

            root = UmaAssembler.AssembleToExistingRoot(umachar.bodyInstance, umachar.headInstance, umachar.tailInstance, root);

            //if (!umachar.UmaAnimator) umachar.UmaAnimator = root.AddComponent<Animator>();
            umachar.UmaAnimator = root.GetComponent<Animator>();
            umachar.UmaAnimator.avatar = AvatarBuilder.BuildGenericAvatar(root, root.name);
            umachar.OverrideController = Resources.Load<AnimatorOverrideController>("Animations/Override Controller");
            umachar.UmaAnimator.runtimeAnimatorController = umachar.OverrideController;
            */

            uma = UmaAssembler.CreateUma(chara, costumeId, headId);

            controller = uma.GetComponent<UmaCharacter>();
            morphs = controller.FaceDrivenKeyTarget.AllMorphs;

            controller.SetRandomBlink(true);
            controller.SetRandomEarTwitch(true);
            controller.PlaySignatureAnimation();

            uma.AddComponent<ExpressiveController>().Chat = chatController;
            expCon = uma.GetComponent<ExpressiveController>();
            expCon.UserInputField = Chat;
            expCon.DialogueObject = Dialogue;
            chatController.expressionVocab.anim_map = new Dictionary<string, List<string>>();
            expCon.MergeAnimMapWithMotionSets(ExpressiveController.GetCharacterMotionSets(chara.Id));

            return;
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

        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log(animField.text);
            if (animField && !string.IsNullOrEmpty(animField.text))
            {
                /*
                controller.OverrideController["clip_s"] = UmaAssetManager.LoadAsset<AnimationClip>(animField.text);
                controller.UmaAnimator.Play("motion_s");
                */

                Debug.Log($"Playing anim : {UmaDatabase.MetaData[animField.text]}");
                controller.PlayAnimation(UmaDatabase.MetaData[animField.text]);
                return;
            }

            //expCon.MergeAnimMapWithMotionSets( ExpressiveController.GetCharacterMotionSets( uma.GetComponent<UmaCharacter>().charaEntry.Id ) );
            
        }

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            
            chatController.APIKey = APIKey;
            if (Dialogue)
            {
                string prompt = "Hi!";

                if (Chat)
                {
                    if (!string.IsNullOrEmpty(Chat.text))
                    {
                        prompt = Chat.text;
                    }
                }

                uma.GetComponent<ExpressiveController>().GenerateResponse(10, true, false, Model);
                Debug.Log($"prompt: {prompt}");
            }
            

            //uma.GetComponent<ExpressiveController>().PlayMorphSet("embarassed");
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            controller.FaceDrivenKeyTarget.ResetLocator();
        }
    }
}