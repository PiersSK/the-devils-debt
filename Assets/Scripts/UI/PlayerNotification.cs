using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNotification : MonoBehaviour
{
    [SerializeField] private Image notificationBackground;
    [SerializeField] private TextMeshProUGUI notificationText;

    [SerializeField] private float visibilityTime = 5f;
    [SerializeField] private float fadeTime = 1f;

    private bool isVisible = false;
    private float timer = 0f;

    private void Update()
    {
        if (isVisible)
        {
            timer += Time.deltaTime;
            if (timer >= visibilityTime)
            {
                if (timer < visibilityTime + fadeTime)
                {
                    Color backgroundColor = notificationBackground.color;
                    backgroundColor.a = 1 - ((timer - visibilityTime) / fadeTime);

                    notificationBackground.color = backgroundColor;
                    notificationText.color = new Color(1, 1, 1, 1 - ((timer - visibilityTime) / fadeTime));
                }
                else
                {
                    isVisible = false;
                }
            }
        }
    }

    public void ShowNotification(string message)
    {
        Color backgroundColor = notificationBackground.color;
        backgroundColor.a = 1;

        notificationBackground.color = backgroundColor;
        notificationText.color = new Color(1, 1, 1, 1);

        notificationText.text = message;

        timer = 0f;
        isVisible = true;
    }
}
