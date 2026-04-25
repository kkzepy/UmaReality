using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class ChatController
{
    public string EndpointURL;
    public string APIKey;

    public ChatHistory chatHistory = new ChatHistory();
    public CharDefinition user;
    public CharDefinition bot;

    public string rules;
    public Dictionary<string, List<string>> availableAnim;

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

    public void AddToHistory(string role, string content)
    {
        chatHistory.messages.Add(new ChatMessage { role = role, content = content });
    }

    public IEnumerator Generate(string prompt, System.Action<string> callback, int historyLimit = 10, bool addToHistory = true, bool regenerate = false, string model = "mistralai/mistral-small-creative")
    {

        if (regenerate)
        {
            chatHistory.RemoveLast();
        }

        //string rules = $"IMPORTANT RULES:\n- You are NOT an AI\n- NEVER narrate as User or {user.name}\n- Show emotions\n- ONLY respond to the user's message\n- Keep replies natural and short (1–5 sentences)\n- Goals are NOT fixed. They evolve based on the current conversation.\n- Do NOT repeat or fixate on past intentions unless still relevant.\n- Prioritize the CURRENT interaction over past plans.\n- If the situation changes, adjust behavior naturally.\n- Always include descriptive narration of actions, expressions, and surroundings.\n- Use a mix of dialogue and narration.\n- Narration should be written in third person.\n- Dialogue should feel natural and emotional.\n- Never reply with plain dialogue only.\n";

        // Set system message
        string availableAnimText = "Available ANIM:\n";

        if (availableAnim != null)
        {
            if (availableAnim.Count == 0)
            {
                availableAnimText = "Available ANIM:\nNone";
            }
            else
            {
                foreach (string anim in availableAnim.Keys)
                {
                    availableAnimText += $"- {anim}";
                }
            }
                
        }

        string systemText = $"{rules}\n\nAvailable EMOTE:\n- smile\n\n{availableAnimText}\nCharacter Profile (for internal use only, DO NOT expose unless relevant):\n{bot.definition}\n\nUser Profile ({user.name}):\n{user.definition}";
        ChatMessage systemMessage = new ChatMessage { role = "system", content = systemText };

        List<ChatMessage> mergedList = new List<ChatMessage>();
        mergedList.Add(systemMessage);
        mergedList.AddRange(chatHistory.messages.TakeLast(historyLimit));
        mergedList.Add(new ChatMessage { role = "user", content = prompt });

        // 1. Setup the request body
        OpenRouterRequest requestData = new OpenRouterRequest
        {
            model = model,
            messages = mergedList
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
                    AddToHistory("user", prompt);
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