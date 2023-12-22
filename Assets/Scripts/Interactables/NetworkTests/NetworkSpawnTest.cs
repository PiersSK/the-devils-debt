using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkSpawnTest : NetworkInteractable
{
    protected override void Interact()
    {
        Transform testObj = Resources.Load<Transform>("SphereObj");
        netSpawner.SpawnObjectAtLocation(testObj, Vector3.zero, PlayerInfo.GetPlayerInfo().IsServer);
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void SpawnSphereServerRpc()
    //{
    //    Debug.Log("ServerRPC called by : " + OwnerClientId);
    //    Transform sphere = Instantiate(Resources.Load<Transform>("Sphere"));
    //    sphere.GetComponent<NetworkObject>().Spawn(true);
    //    sphere.transform.position = Vector3.zero;
    //}
}
