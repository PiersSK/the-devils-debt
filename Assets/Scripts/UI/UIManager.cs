using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("PlayerUI")]
    public TextMeshProUGUI playerUI_promptText;
    public TextMeshProUGUI playerUI_joinCode;

    public ResourceBarUI stamina;
    public ResourceBarUI mana;
    public ResourceBarUI health;

    private void Awake()
    {
        Instance = this;
    }
}
