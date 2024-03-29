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
        Transform defaultPath = Instantiate(Resources.Load<Transform>("RoomPatrol"));
        defaultPath.position = transform.position + new Vector3(0, -1.6f, 0);
        defaultPath.GetComponent<NetworkObject>().Spawn(true);

        Transform enemyPrefab = Resources.Load<Transform>(prefabName);

        for (int i = 0; i < Random.Range(1, 3); i++)
        {
            enemyPrefab.position = transform.position + new Vector3(Random.Range(-5f, 5f), -1f, Random.Range(-5f, 5f));

            Transform enemyObj = Instantiate(enemyPrefab);
            enemyObj.GetComponent<NetworkObject>().Spawn(true);

            SetEnemyPatrolClientRpc(enemyObj.GetComponent<NetworkObject>(), defaultPath.GetComponent<NetworkObject>());
        }
    }

    [ClientRpc]
    private void SetEnemyPatrolClientRpc(NetworkObjectReference enemy, NetworkObjectReference path)
    {
        enemy.TryGet(out NetworkObject enemyNO);
        path.TryGet(out NetworkObject pathNO);

        enemyNO.GetComponent<Enemy>().path = pathNO.GetComponent<Path>();
    }
}
