using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectiveRoom : Room
{
    private const string ROOMPREFAB = "Rooms/ObjectiveRoom";
    private const string CHESTPREFAB = "ObjectiveChest";

    public override void SpawnRoomContents()
    {
        SpawnObjectiveServerRpc();
    }

    public override Transform GetPrefab()
    {
        return Resources.Load<Transform>(ROOMPREFAB);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnObjectiveServerRpc()
    {

        Transform chestPrefab = Resources.Load<Transform>(CHESTPREFAB);
        Transform chestObj = Instantiate(chestPrefab);
        chestObj.position = new Vector3(transform.position.x, transform.position.y - 1.7f, transform.position.z);
        chestObj.GetComponent<NetworkObject>().Spawn();
        //chestObj.GetComponent<NetworkObject>().TrySetParent(transform);

    }

    //[ClientRpc]
    //private void SetEnemyPatrolClientRpc(NetworkObjectReference enemy, NetworkObjectReference path)
    //{
    //    enemy.TryGet(out NetworkObject enemyNO);
    //    path.TryGet(out NetworkObject pathNO);

    //    enemyNO.GetComponent<Enemy>().path = pathNO.GetComponent<Path>();
    //}
}
