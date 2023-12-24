using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BoonRoom : Room
{
    private NetworkVariable<bool> isManaRegen = new(false);

    public override void OnNetworkSpawn()
    {
        isManaRegen.OnValueChanged += SyncManaRegen;
        if (IsServer) isManaRegen.Value = Random.Range(0, 10) < 3;
        if (IsServer) isManaRegen.Value = true;
    }

    private void SyncManaRegen(bool prevVal, bool newVal)
    {
        Debug.Log("Syncing manaFount to " + newVal);
        isManaRegen.Value = newVal;
        isManaRegen.OnValueChanged -= SyncManaRegen;

    }

    public override void SpawnRoomContents() {
        Debug.Log("Spawning fountain: " + isManaRegen.Value);

        if (isManaRegen.Value) SpawnFountainServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnFountainServerRpc()
    {
        Transform fountainObj = Instantiate(Resources.Load<Transform>("Fountain"));
        fountainObj.localPosition = transform.position;
        fountainObj.GetComponent<NetworkObject>().Spawn();
    }


    public override Transform GetPrefab()
    {
        return Resources.Load<Transform>("Rooms/BoonRoom");
    }
}
