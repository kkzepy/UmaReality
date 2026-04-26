using Gallop;
using Newtonsoft.Json;
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

    private void Start()
    {
        chatController = new ChatController();

        chatController.LoadUserDefinition("persona.json");
        chatController.LoadBotDefinition("tokai_teio.json");
        Dialogue.GetComponent<DIalogueController>().DialogueTitle.text = chatController.bot.name;

        chatController.EndpointURL = "https://openrouter.ai/api/v1/chat/completions";
        chatController.rules = File.ReadAllText("rules.txt");
        chatController.LoadExpressionVocab("expression_dict.json");
        expVoc = chatController.expressionVocab;
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

            uma = root;

            controller = umachar;
            morphs = controller.FaceDrivenKeyTarget.AllMorphs;

            umachar.SetRandomBlink(true);
            umachar.SetRandomEarTwitch(true);
            umachar.PlaySignatureAnimation();

            uma.AddComponent<ExpressiveController>().Chat = chatController;
            expCon = uma.GetComponent<ExpressiveController>();
            expCon.UserInputField = Chat;
            expCon.DialogueObject = Dialogue;

            return;
        }

        Debug.LogWarning($"No input!");
    }

    void HandleResponse(string value)
    {
        ExpressiveResponse response = ExpressiveResponseParser.Parse(value);
        if (response != null)
        {
            Debug.Log($"{value}\n\n{response.Emote}\n{response.Anim}\n{response.Dialogue}\n");
            
            if (expVoc.anim_map.ContainsKey(response.Anim))
            {
                if (expVoc.anim_map.TryGetValue(response.Anim, out List<string> animations))
                {
                    int randomIndex = Random.Range(0, animations.Count);
                    controller.PlayAnimation(animations[randomIndex]);
                }
            }

            if (response.Emote == "smile")
            {
                controller.SetSmile(true, 1f, .14f, true, true, false);
            }

            if (expVoc.face_morph_map.ContainsKey(response.Emote))
            {
                if (expVoc.face_morph_map.TryGetValue(response.Emote, out List<MorphSet> morphs))
                {
                    if (prevMorph == null)
                    {
                        prevMorph = response.Emote;
                        foreach (MorphSet morph in morphs)
                        {
                            controller.PlayMorph(morph.morphName, morph.startWeight, morph.endWeight, morph.duration);
                        }
                    }

                    else
                    {
                        foreach (MorphSet morph in expVoc.face_morph_map[prevMorph])
                        {
                            controller.PlayMorph(morph.morphName, morph.endWeight, morph.startWeight, morph.duration);
                        }

                        foreach (MorphSet morph in morphs)
                        {
                            controller.PlayMorph(morph.morphName, morph.startWeight, morph.endWeight, morph.duration);
                        }

                        prevMorph = null;
                    }
                    
                }
            }

            Dialogue.GetComponent<DIalogueController>().UpdateContent(response.Dialogue);
        }
        else
        {
            Debug.LogWarning(value);
        }
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
            }
            
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

                uma.GetComponent<ExpressiveController>().GenerateResponse();
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