using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class ChatController
{
    public string EndpointURL;
    public string APIKey;

    public ChatHistory chatHistory = new ChatHistory();
    public CharDefinition user;
    public CharDefinition bot;

    public string rules;
    public ExpressionVocab expressionVocab;

    public void LoadUserDefinition(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            var file = File.ReadAllText(path);
            user = JsonConvert.DeserializeObject<CharDefinition>(file);
        }
    }

    public void LoadBotDefinition(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log(File.Exists(path));
            var file = File.ReadAllText(path);
            bot = JsonConvert.DeserializeObject<CharDefinition>(file);
        }
    }

    public void LoadExpressionVocab(string path)
    {
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            expressionVocab = JsonConvert.DeserializeObject<ExpressionVocab>(File.ReadAllText(path));
        }
        //return null;
    }

    public void AddToHistory(string role, string content)
    {
        chatHistory.messages.Add(new ChatMessage { role = role, content = content });
    }

    public List<ChatMessage> BuildPrompt(string userInput, int historyLimit = 10, bool regenerate = false)
    {
        if (regenerate)
        {
            chatHistory.RemoveLast();
        }

        // Set system message
        string availableAnim = "Available ANIM:\n";
        string availableEmote = "Available EMOTE:\n";

        if (expressionVocab != null && expressionVocab.anim_map != null)
        {
            if (expressionVocab.anim_map.Count == 0)
            {
                availableAnim = "Available ANIM:\nNone";
            }
            else
            {
                foreach (string anim in expressionVocab.anim_map.Keys)
                {
                    availableAnim += $"- {anim}\n";
                }
            }
        }

        if (expressionVocab != null && expressionVocab.face_morph_map != null)
        {
            if (expressionVocab.face_morph_map.Count == 0)
            {
                availableAnim = "Available EMOTE:\nNone";
            }
            else
            {
                foreach (string emote in expressionVocab.face_morph_map.Keys)
                {
                    availableEmote += $"- {emote}\n";
                }
            }
        }

        string systemText = $"{rules}\n\n{availableEmote}\n{availableAnim}\nCharacter Profile (for internal use only, DO NOT expose unless relevant):\n{bot.definition}\n\nUser Profile ({user.name}):\n{user.definition}";
        ChatMessage systemMessage = new ChatMessage { role = "system", content = systemText };

        List<ChatMessage> mergedList = new List<ChatMessage>();
        mergedList.Add(systemMessage);
        mergedList.AddRange(chatHistory.messages.TakeLast(historyLimit));
        mergedList.Add(new ChatMessage { role = "user", content = userInput });


        return mergedList;
    }

    public IEnumerator Generate(List<ChatMessage> messages, System.Action<string> callback, bool addToHistory = true, string model = "meta-llama/llama-4-scout-17b-16e-instruct")
    {
        /*
        if (regenerate)
        {
            chatHistory.RemoveLast();
        }

        // Set system message
        string availableAnim = "Available ANIM:\n";
        string availableEmote = "Available EMOTE:\n";

        if (expressionVocab != null && expressionVocab.anim_map != null)
        {
            if (expressionVocab.anim_map.Count == 0)
            {
                availableAnim = "Available ANIM:\nNone";
            }
            else
            {
                foreach (string anim in expressionVocab.anim_map.Keys)
                {
                    availableAnim += $"- {anim}\n";
                }
            }   
        }

        if (expressionVocab != null && expressionVocab.face_morph_map != null)
        {
            if (expressionVocab.face_morph_map.Count == 0)
            {
                availableAnim = "Available EMOTE:\nNone";
            }
            else
            {
                foreach (string emote in expressionVocab.face_morph_map.Keys)
                {
                    availableEmote += $"- {emote}\n";
                }
            }
        }

        string systemText = $"{rules}\n\n{availableEmote}\n{availableAnim}\nCharacter Profile (for internal use only, DO NOT expose unless relevant):\n{bot.definition}\n\nUser Profile ({user.name}):\n{user.definition}";
        ChatMessage systemMessage = new ChatMessage { role = "system", content = systemText };

        List<ChatMessage> mergedList = new List<ChatMessage>();
        mergedList.Add(systemMessage);
        mergedList.AddRange(chatHistory.messages.TakeLast(historyLimit));
        mergedList.Add(new ChatMessage { role = "user", content = prompt });
        */

        // 1. Setup the request body
        OpenAIChatCompletion requestData = new OpenAIChatCompletion
        {
            model = model,
            messages = messages
        };

        string jsonPayload = JsonUtility.ToJson(requestData);

        // 2. Create the web request
        using (UnityWebRequest request = new UnityWebRequest(EndpointURL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 3. Set Headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + APIKey);

            yield return request.SendWebRequest();

            // 4. Handle Response
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                OpenRouterResponse response = JsonUtility.FromJson<OpenRouterResponse>(request.downloadHandler.text);
                string responseText = response.choices[0].message.content;

                if (addToHistory)
                {
                    AddToHistory("user", messages.TakeLast(1).FirstOrDefault().content);
                    Debug.Log(messages.TakeLast(1).FirstOrDefault().role);
                    AddToHistory("assistant", responseText);
                }

                callback?.Invoke(responseText);
            }
        }
    }
}

public static class ExpressiveResponseParser
{
    public static ExpressiveResponse Parse(string input)
    {
        var response = new ExpressiveResponse();

        response.Emote = ExtractValue(input, "EMOTE");
        response.Anim = ExtractValue(input, "ANIM");
        response.Dialogue = ExtractValue(input, "DIALOGUE");

        return response;
    }

    private static string ExtractValue(string input, string key)
    {
        var pattern = $@"\[{key}:(.*?)\]";
        var match = Regex.Match(input, pattern, RegexOptions.Singleline);

        if (match.Success)
            return match.Groups[1].Value.Trim();

        return null;
    }
}

public class ExpressiveController : MonoBehaviour
{
    public UmaCharacter UmaController;

    public GameObject DialogueObject;
    
    public TMP_InputField UserInputField;
    public DIalogueController ChatDialogueHandler;

    public ChatController Chat;

    public string prevMorphSetsName;
    private void Start()
    {
        UmaController = GetComponent<UmaCharacter>();

        if (DialogueObject && UserInputField)
        {
            ChatDialogueHandler = UserInputField.GetComponent<DIalogueController>();

            if (!string.IsNullOrEmpty(Chat.bot.name) && ChatDialogueHandler)
            {
                ChatDialogueHandler.SetTitle(Chat.bot.name);
            }
        }
    }

    void PlayRandomAnimations(string key)
    {
        if (Chat.expressionVocab.anim_map.TryGetValue(key, out List<string> anims))
        {
            int randomIndex = Random.Range(0, anims.Count);
            string animationPath = anims[randomIndex];

            UmaController.PlayAnimation(animationPath);
        }
        else
        {
            Debug.Log($"PlayRandomAnimations: no anim with key: {key}");
        }
    }

    IEnumerator HandleMultiAnimatons(List<string> animations)
    {
        foreach (string animation in animations)
        {
            if (!animation.Contains("_")) { PlayRandomAnimations(animation); continue; }

            string animationName = animation.Split("_")[1];
            
            int animationDelay = Convert.ToInt32(animation.Split("_")[0]);

            if (animationDelay!=0) yield return new WaitForSeconds(animationDelay);

            PlayRandomAnimations(animationName);
        }
    }

    IEnumerator HandleMultiEmotes(List<string> emotes)
    {
        foreach (string emote in emotes)
        {
            if (!emote.Contains("_")) { PlayRandomAnimations(emote); continue; }

            string morphSetName = emote.Split("_")[1];

            int morphSetDelay = Convert.ToInt32(emote.Split("_")[0]);

            if (morphSetDelay != 0) yield return new WaitForSeconds(morphSetDelay);

            PlayMorphSets(morphSetName);
        }
    }

    void HandleResponse(string value)
    {
        ExpressiveResponse resp = ExpressiveResponseParser.Parse(value);

        DialogueObject.GetComponent<DIalogueController>().UpdateContent(resp.Dialogue);

        Debug.Log(resp.Anim);

        if (resp.Anim.Contains(",") && resp.Anim.Contains("_"))
        {
            List<string> animations = resp.Anim.Split(",").ToList();

            StartCoroutine(HandleMultiAnimatons(animations));
        }
        else //if (Chat.expressionVocab.anim_map.TryGetValue(resp.Anim, out List<string> animations))
        {
            /*
            int randomIndex = Random.Range(0, animations.Count);
            string animation = animations[randomIndex];

            UmaController.PlayAnimation(animation);*/

            PlayRandomAnimations(resp.Anim);
        }

        if (resp.Emote.Contains(",") && resp.Emote.Contains("_"))
        {
            List<string> emotes = resp.Emote.Split(",").ToList();

            StartCoroutine(HandleMultiEmotes(emotes));
        }
        else
        {
            PlayMorphSets(resp.Emote);
        }
    }
    public void GenerateResponse(int historyLimit = 10, bool addToHistory = true, bool regenerate = false, string model = "mistral-small-creative")
    {
        if (!string.IsNullOrEmpty(UserInputField.text))
        {
            List<ChatMessage> finalPrompt = Chat.BuildPrompt(UserInputField.text, historyLimit, regenerate);

            StartCoroutine(
                Chat.Generate(finalPrompt, HandleResponse, addToHistory, model)
            );
        }
    }

    public void PlayMorphSets(string name, bool ignorePrevious = false)
    {
        if (Chat.expressionVocab.face_morph_map.TryGetValue(name, out List<MorphSet> morphSets) && UmaController)
        {
            if (morphSets.Count==0) { ResetAll(); return; }

            if (prevMorphSetsName != name && prevMorphSetsName != null && !ignorePrevious)
            {
                List<MorphSet> prevMorphSets = Chat.expressionVocab.face_morph_map[prevMorphSetsName];

                foreach (MorphSet prevMorphSet in prevMorphSets)
                {
                    if (prevMorphSet.morphName=="BLUSH")
                    {
                        UmaController.SetBlush(false);
                        continue;
                    }

                    UmaController.PlayMorph(prevMorphSet.morphName, prevMorphSet.endWeight, prevMorphSet.startWeight, prevMorphSet.duration);
                }
            }

            foreach (MorphSet morphSet in morphSets)
            {
                if (morphSet.morphName == "BLUSH")
                {
                    UmaController.SetBlush(true);
                    continue;
                }

                UmaController.PlayMorph(morphSet.morphName, morphSet.startWeight, morphSet.endWeight, morphSet.duration);
                Debug.Log($"Playing morph: {morphSet.morphName}");
            }

            prevMorphSetsName = name;
        }
        else
        {
            Debug.Log($"No MorphSets with the key: {name}");
            ResetAll();
        }
    }

    public void ResetAll()
    {
        UmaController.FaceDrivenKeyTarget.ResetLocator();
        UmaController.SetBlush(false);
    }
}
