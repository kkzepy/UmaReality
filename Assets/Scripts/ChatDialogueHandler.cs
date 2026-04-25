using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
}
