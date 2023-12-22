using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private RelayManager relay;

    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            //NetworkManager.Singleton.StartHost();
            relay.CreateRelay();
        });
        clientButton.onClick.AddListener(() =>
        {
            //NetworkManager.Singleton.StartClient();
            relay.JoinRelay("test");
        });
    }
}
