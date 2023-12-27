using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBarUI : MonoBehaviour
{
    public Image bar;
    public Image overlay;
    public Image altOverlay;
    public TextMeshProUGUI value;

    private float timeTillFade;
    private float overlayFadeTimer;

    private void Update()
    {
        if (overlay != null && overlay.color.a > 0)
            FadeOverlay(overlay);

        if (altOverlay != null && altOverlay.color.a > 0)
            FadeOverlay(altOverlay);
    }

    private void FadeOverlay(Image overlayToFade)
    {
        overlayFadeTimer -= Time.deltaTime;
        overlayFadeTimer = Mathf.Clamp(overlayFadeTimer, 0, timeTillFade);

        overlayToFade.color = new Color(1, 1, 1, overlayFadeTimer / timeTillFade);
    }

    public void UpdateBar(float currentValue, float maxValue)
    {
        bar.fillAmount = currentValue / maxValue;
        if (value != null) value.text = currentValue.ToString();
    }

    public void ShowOverlay(float timeTillFade = 3f, bool useAlt = false)
    {
        this.timeTillFade = timeTillFade;
        overlayFadeTimer = timeTillFade;

        if(!useAlt)
            overlay.color = new Color(1, 1, 1, 1);
        else
            altOverlay.color = new Color(1, 1, 1, 1);
    }
}
