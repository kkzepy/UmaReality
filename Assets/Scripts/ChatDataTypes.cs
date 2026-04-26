using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor.VersionControl;

[System.Serializable]
public class ChatMessage
{
    public string role;    // "system", "user", or "assistant"
    public string content;
}

public class MorphSet
{
    public string morphName;
    public float startWeight;
    public float endWeight;
    public float duration;
}

public class ExpressionVocab
{
    public Dictionary<string, List<string>> anim_map;
    public Dictionary<string, List<MorphSet>> face_morph_map;
}

[System.Serializable]
public class ChatHistory
{
    public List<ChatMessage> messages = new List<ChatMessage>();

    public int Count() { return messages.Count; }
    public void RemoveLast() { messages.RemoveAt(messages.Count - 1); messages.RemoveAt(messages.Count - 1); }
    public string ToText()
    {
        string result = "";

        foreach(ChatMessage message in messages)
        {
            result += $"{message.role}: {message.content}\n";
        }

        return result;
    }
}
public class ExpressiveResponse
{
    public string Emote;
    public string Anim;
    public string Dialogue;
}

[Serializable]
public class OpenRouterRequest
{
    public string model;
    public List<ChatMessage> messages;
    public int max_tokens = 500;
}

[System.Serializable]
public class CharDefinition
{
    public string name;
    public string nickname;
    public string definition;
    public string additional_rules;
}

[Serializable]
public class Choice
{
    public ChatMessage message;
}

[Serializable]
public class OpenRouterResponse
{
    public Choice[] choices;
}