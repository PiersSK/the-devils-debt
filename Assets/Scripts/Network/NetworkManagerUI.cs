using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    [SerializeField] private TextMeshProUGUI playerList;

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            UpdatePlayerListServerRpc();
        });
        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            UpdatePlayerListServerRpc();
        });
    }

    [ServerRpc]
    private void UpdatePlayerListServerRpc()
    {
        Debug.Log("Player joined with client id: " + OwnerClientId);
        playerList.text += OwnerClientId + "\n";
    }
}
