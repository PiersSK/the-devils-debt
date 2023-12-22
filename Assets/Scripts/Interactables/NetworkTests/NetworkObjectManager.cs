using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkObjectManager : NetworkBehaviour
{
    private Transform currentlyInspectedObject;

    public Transform SpawnObjectUnderParent(Transform objToSpawn, Transform parent, Vector3 offset, bool callerIsServer=true)
    {
        currentlyInspectedObject = Instantiate(objToSpawn, parent);
        currentlyInspectedObject.localPosition = offset;
        if (callerIsServer)
            currentlyInspectedObject.GetComponent<NetworkObject>().Spawn();
        else
            SpawnObjectServerRpc();

        return currentlyInspectedObject;

    }

    public Transform SpawnObjectAtLocation(Transform objToSpawn, Vector3 loc, bool callerIsServer = true)
    {
        Debug.Log("Spawn request received. IsServer=" + callerIsServer);
        objToSpawn.position = loc;
        currentlyInspectedObject = Instantiate(objToSpawn);
        if (callerIsServer)
            currentlyInspectedObject.GetComponent<NetworkObject>().Spawn();
        else
            SpawnObjectServerRpc();

        return currentlyInspectedObject;

    }

    public void DestroyObject(GameObject objToSpawn, bool callerIsServer = true)
    {
        Destroy(objToSpawn);
        if (callerIsServer)
            currentlyInspectedObject.GetComponent<NetworkObject>().Despawn();
        else
            DespawnObjectServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnObjectServerRpc()
    {
        Debug.Log("SpawnObjectServerRpc called!");
        currentlyInspectedObject.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnObjectServerRpc()
    {
        Debug.Log("SpawnObjectServerRpc called!");
        currentlyInspectedObject.GetComponent<NetworkObject>().Despawn();
    }
}
