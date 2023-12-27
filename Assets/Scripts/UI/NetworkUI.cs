using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;
using TMPro;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private TextMeshProUGUI joinCodeInpt;

    private void Awake()
    {
        hostBtn.onClick.AddListener(() =>
        {
            Debug.Log("Starting relay and becoming host");
            RelayManager.Instance.CreateRelay();
            Hide();
        });
        clientBtn.onClick.AddListener(() =>
        {
            Debug.Log("Joining relay as client");
            try
            {
                RelayManager.Instance.JoinRelay(joinCodeInpt.text.Substring(0,6));
                Hide();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
