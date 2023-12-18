using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkSpawnTest : Interactable
{
    protected override void Interact()
    {
        SpawnSphereServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnSphereServerRpc()
    {
        Debug.Log("ServerRPC called by : " + OwnerClientId);
        Transform sphere = Instantiate(Resources.Load<Transform>("Sphere"));
        sphere.GetComponent<NetworkObject>().Spawn(true);
        sphere.transform.position = Vector3.zero;
    }
}
