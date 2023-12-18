using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    private TextMeshProUGUI promptText;

    private void Start()
    {
        promptText = GameObject.Find("PromptText").GetComponent<TextMeshProUGUI>();
    }

    public void UpdateText(string promptMessage)
    {
        if (promptMessage != string.Empty)
        {
            promptText.text = "[E] " + promptMessage;
        }
        else
        {
            promptText.text = string.Empty;
        }
    }
}
