using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;

public class LobbyItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI serverName;
    [SerializeField] private TextMeshProUGUI playerCount;
    public Button joinLobbyBtn;

    private NetworkLobby netLobby;
    public Lobby lobby;

    private void Start()
    {
        netLobby = GameObject.Find("LobbyManager").GetComponent<NetworkLobby>();
        joinLobbyBtn.onClick.AddListener(JoinLobby);
    }

    private void JoinLobby()
    {
        netLobby.JoinLobby(lobby);
    }

    public void UpdateServerName(string name)
    {
        serverName.text = name;
    }

    public void UpdatePlayerCount(int current, int max)
    {
        playerCount.text = current + "/" + max;
    }
}
