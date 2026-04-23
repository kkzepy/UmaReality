using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OpenAIHandler : MonoBehaviour
{
    public List<ChatMessage> conversationHistory = new List<ChatMessage>();

    void Start()
    {
        // Define the Character Persona
        conversationHistory.Add(new ChatMessage
        {
            role = "system",
            content = "You are a grumpy innkeeper in a high-fantasy world."
        });
    }

    public void SendPlayerMessage(string playerText)
    {
        // 1. Add player message to memory
        conversationHistory.Add(new ChatMessage { role = "user", content = playerText });

        // 2. Send the FULL history to the API
        StartCoroutine(PostRequest(conversationHistory));
    }

    // Inside your PostRequest, you would now serialize 'conversationHistory' 
    // instead of just a single string prompt.
}

public class ChatDialogueHandler : MonoBehaviour
{
    public TMP_Text DialogueTitle;
    public TMP_Text DialogueContent;

    // Start is called before the first frame update
    void Start()
    {
        if (DialogueTitle && DialogueContent)
        {
        
        }
    }

    IEnumerator StreamContentUpdate(string contentText, float duration = 2.5f)
    {
        foreach (char c in contentText)
        {
            DialogueContent.text += c; // Tambahkan karakter satu per satu

            // Memberikan jeda agar terlihat seperti mengetik
            yield return new WaitForSeconds(duration * Time.deltaTime);
        }
    }

    public void UpdateContent(string contentText, bool append = false, bool stream = true, float duration = 2.5f)
    {
        if (!append) DialogueContent.text = "";

        if (stream)
        {
            StartCoroutine(StreamContentUpdate(contentText, duration));
        }
        else
        {

            DialogueContent.text += contentText;
        }
    }

    public void SetTitle(string titleText)
    {
        DialogueTitle.text = titleText;
    }

    public void SetActive(bool state)
    {
        DialogueContent.gameObject.SetActive(state);
        DialogueTitle.gameObject.SetActive(state);
        gameObject.SetActive(state);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            string content = "\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\"";
            UpdateContent(content);
        }
    }
}
