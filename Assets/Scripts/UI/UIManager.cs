using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public TextMeshProUGUI playerUI_promptText;
    public TextMeshProUGUI playerUI_joinCode;
    public Image playerUI_staminaBar;

    private void Awake()
    {
        Instance = this;
    }
}
