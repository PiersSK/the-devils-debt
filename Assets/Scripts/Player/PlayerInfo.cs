using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerInfo : NetworkBehaviour
{
    private PlayerUI playerUI;

    private NetworkVariable<FixedString32Bytes> playerName = new(string.Empty);
    public NetworkVariable<FixedString32Bytes> PlayerName
    {
        get { return playerName; }
        set { playerName = value; playerUI.UpdatePlayerNametag(value.Value.ToString()); }
    }


    public override void OnNetworkSpawn()
    {
        gameObject.tag = IsOwner ? "Self" : "Player";
        Debug.Log("PlayerInfo.OnNetworkSpawn IsServer=" + IsServer + "IsHost=" + IsHost + "IsClient=" + IsClient);
        base.OnNetworkSpawn();
    }

    private void Awake()
    {
        playerUI = gameObject.GetComponent<PlayerUI>();
    }

    public static PlayerInfo GetPlayerInfo()
    {
        return GameObject.FindGameObjectWithTag("Self").GetComponent<PlayerInfo>();
    }
}
