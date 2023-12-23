using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public TextMeshProUGUI playerUI_promptText;
    public TextMeshProUGUI playerUI_joinCode;

    private void Awake()
    {
        Instance = this;
    }
}
