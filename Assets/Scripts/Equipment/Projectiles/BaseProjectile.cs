using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseProjectile : MonoBehaviour
{
    private float damage = 5f;
    public NetworkObject playerSourceNO;
    public bool hasBeenfired = false;
    

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenfired)
        {
            if (other.GetComponent<Enemy>() != null)
            {
                other.GetComponent<Enemy>().DamageToEnemyServerRpc((int)damage, playerSourceNO);
            }
            DestroyProjectileServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyProjectileServerRpc()
    {
        Destroy(gameObject);
        GetComponent<NetworkObject>().Despawn();
    }
}
