using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class ChatMessage
{
    public string role;    // "system", "user", or "assistant"
    public string content;
}

[System.Serializable]
public class ChatHistory
{
    public List<ChatMessage> messages = new List<ChatMessage>();
}