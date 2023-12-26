using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBarUI : MonoBehaviour
{
    public Image bar;
    public Image overlay;
    public TextMeshProUGUI value;

    private float timeTillFade;
    private float overlayFadeTimer;

    private void Update()
    {
        if (overlay != null)
        {
            overlayFadeTimer -= Time.deltaTime;
            overlayFadeTimer = Mathf.Clamp(overlayFadeTimer, 0, timeTillFade);

            overlay.color = new Color(1, 1, 1, overlayFadeTimer / timeTillFade);
        }
    }

    public void UpdateBar(float currentValue, float maxValue)
    {
        bar.fillAmount = currentValue / maxValue;
        if (value != null) value.text = currentValue.ToString();
    }

    public void ShowOverlay(float timeTillFade = 3f)
    {
        this.timeTillFade = timeTillFade;
        overlayFadeTimer = timeTillFade;

        overlay.color = new Color(1, 1, 1, 1);
    }
}
