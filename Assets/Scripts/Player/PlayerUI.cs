using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Collections;
using System.Linq;
using Unity.Netcode;
using System;

public class PlayerUI : NetworkBehaviour
{
    public TextMeshProUGUI playerNameUI;

    private TextMeshProUGUI promptText;

    private int playersInScene;

    public EventHandler<PlayerCountChangedArgs> PlayerCountChanged;
    public class PlayerCountChangedArgs : EventArgs { };

    private void Start()
    {
        promptText = GameObject.Find("PromptText").GetComponent<TextMeshProUGUI>();
        PlayerCountChanged += UpdatePlayerNametag;
    }

    private void Update()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length > playersInScene)
            PlayerCountChanged?.Invoke(this, new PlayerCountChangedArgs { });

        playersInScene = GameObject.FindGameObjectsWithTag("Player").Length;

        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            player.GetComponent<PlayerUI>().playerNameUI.gameObject.transform.parent.LookAt(transform);
        }
    }

    public void UpdateText(string promptMessage)
    {
        if (promptText != null) { 
            if (promptMessage != string.Empty)
                promptText.text = "[E] " + promptMessage;
            else
                promptText.text = string.Empty;
        }
    }

    public void UpdatePlayerNametag(object obj, PlayerCountChangedArgs e)
    {
        string name = gameObject.GetComponent<PlayerInfo>().PlayerName.Value.ToString();

        Debug.Log("Updating nametag due to player join");
        if (IsServer)
            UpdateNametagClientRpc(name);
        else if (IsClient)
            UpdateNametagServerRpc(name);

    }


    public void UpdatePlayerNametag(string name)
    {
        Debug.Log("IsHost:" + IsHost + " IsServer:" + IsServer + " IsClient:" + IsClient);
        if (IsServer)
            UpdateNametagClientRpc(name);
        else if (IsClient)
            UpdateNametagServerRpc(name);

    }

    [ClientRpc]
    private void UpdateNametagClientRpc(string name)
    {
        playerNameUI.text = name;
    }

    [ServerRpc]
    private void UpdateNametagServerRpc(string name)
    {
        playerNameUI.text = name;
    }
}
