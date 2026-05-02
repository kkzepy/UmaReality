using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChatMessage
{
    public string role;    // "system", "user", or "assistant"
    public string content;
}

public class MorphSet
{
    public string morphTag;
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
public class OpenAIChatCompletion
{
    public string model;
    public List<ChatMessage> messages;
    public int max_tokens = 500;
    public float temperature = 0.9f;
}

[System.Serializable]
public class CharDefinition
{
    public string name;
    public string nickname;
    public string definition;
    public string additional_rules;
}

public class BotDefinition
{
    public string name;
    public string definition;
    public string additional_rules;
    public string scenario;

    public int id;
    public bool use_builtin_motions;
    public ExpressionVocab exp_voc;
}

public class UmaEnvironment
{
    public string path;
    public int cam_fov = 20;
    public Vector3 cam_rot = new(0, 180, 0);
    public Vector3 cam_xyz = new(0, 0, 0);
    public string light_color = "FFFFFF";
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