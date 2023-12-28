using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("PlayerUI")]
    public TextMeshProUGUI playerUI_promptText;
    public TextMeshProUGUI playerUI_joinCode;
    public GameObject playerUI_hitmarker;

    public ResourceBarUI stamina;
    public ResourceBarUI mana;
    public ResourceBarUI health;

    public HotbarIcon hotbarMain;
    public HotbarIcon hotbarOff;
    public HotbarIcon hotbarAccessory;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowHitmarker()
    {
        playerUI_hitmarker.SetActive(true);
        Invoke(nameof(HideHitmarker), 0.2f);
    }

    private void HideHitmarker()
    {
        playerUI_hitmarker.SetActive(false);
    }
}
