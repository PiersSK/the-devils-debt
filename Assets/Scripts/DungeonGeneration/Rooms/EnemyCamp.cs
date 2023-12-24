using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyCamp : Room
{
    public override void SpawnRoomContents() {
        SpawnEnemyServerRpc("Enemy");
    }

    public override Transform GetPrefab()
    {
        return Resources.Load<Transform>("Rooms/EnemyRoom");
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnEnemyServerRpc(string prefabName)
    {
        Transform enemyObj = Instantiate(Resources.Load<Transform>(prefabName));
        enemyObj.position = transform.position + new Vector3(Random.Range(-5f, 5f), -1f, Random.Range(-5f, 5f));
        enemyObj.GetComponent<NetworkObject>().Spawn();
    }
}
