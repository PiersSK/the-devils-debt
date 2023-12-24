using System;
using Unity.Netcode;

public class PlayerMana : NetworkBehaviour
{
    private int maxMana = 10;
    private int currentMana;
    public int CurrentMana
    {
        get
        {
            return currentMana;
        }
        set
        {
            currentMana = value;
            currentMana = Math.Clamp(currentMana, 0, maxMana);
            UpdateManaUI();
        }
    }

    private void Start()
    {
        CurrentMana = maxMana;

    }

    private void UpdateManaUI()
    {
        UIManager.Instance.playerUI_manaVal.text = currentMana.ToString();
        UIManager.Instance.playerUI_manaBar.fillAmount = (float)currentMana / maxMana;
    }
}
