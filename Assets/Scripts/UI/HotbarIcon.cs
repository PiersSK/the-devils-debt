using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarIcon : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private Image cooldown;
    [SerializeField] private TextMeshProUGUI cooldownDisplay;

    public Equipment itemInSlot;

    private float cooldownTimer = 0f;
    private float cooldownLength = 0f;

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            if (itemInSlot != null) itemInSlot.onCooldown = true;
            Debug.Log("On Cooldown: " + cooldownTimer + "/" + cooldownLength);
            cooldownTimer -= Time.deltaTime;
            cooldown.fillAmount = cooldownTimer / cooldownLength;
            cooldownDisplay.text = cooldownTimer.ToString("0.0");
        }
        else
        {
            if(itemInSlot != null) itemInSlot.onCooldown = false;
            Debug.Log("Off Cooldown");
            cooldownTimer = 0f;
            cooldown.fillAmount = 0;
            cooldownDisplay.text = string.Empty;
        }
    }

    public void SetEquipped(bool isActive)
    {
        if (isActive) {
            background.color = new Color(1, 1, 1, 0.2f);
            icon.color = new Color(1, 1, 1, 1);
        } else
        {
            background.color = new Color(0, 0, 0, 0.2f);
            icon.color = new Color(1, 1, 1, 0.5f);
        }
    }

    public void PutOnCooldown(float cooldownLength)
    {
        itemInSlot.onCooldown = true;

        this.cooldownLength = cooldownLength;
        cooldownTimer = cooldownLength;

        cooldown.fillAmount = 1;
        cooldownDisplay.text = cooldownLength.ToString();
    }
}
