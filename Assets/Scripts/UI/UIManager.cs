using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("PlayerUI")]
    public TextMeshProUGUI playerUI_promptText;
    public TextMeshProUGUI playerUI_joinCode;
    public Image playerUI_staminaBar;
    public Image playerUI_manaBar;
    public Image playerUI_manaOverlay;
    public TextMeshProUGUI playerUI_manaVal;


    private void Awake()
    {
        Instance = this;
    }
}
