using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
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

    public void UpdateContent(string contentText, bool stream = true, bool append = false, float duration = 2.5f)
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

    public string title
    {
        set
        {
            if (DialogueTitle)
            {
                DialogueTitle.text = value;
            }
        }
        get
        {
            if (DialogueTitle)
            {
                return DialogueTitle.text;
            }
            return null;
        }
    }

    public bool active
    {
        set
        {
            gameObject.SetActive(value);
        }
        get
        {
            return gameObject.activeSelf;
        }
    }
}
