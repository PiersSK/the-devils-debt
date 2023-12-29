using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Tome : Equipment
{
    [SerializeField] private float manaCost;

    [SerializeField] private float firerate = 10f;

    public override void PerformAbility()
    {
        if (!onCooldown && equippedPlayer.playerMana.currentMana.Value >= manaCost)
        {
            Debug.Log("Triggered tome attack");
            UIManager.Instance.hotbarOff.PutOnCooldown(firerate);

            equippedPlayer.playerMana.IncrementPlayerManaServerRpc(-(int)manaCost);

            Debug.Log("Calling RPC, IsServer="+IsServer+", IsOwner="+IsOwner+", IsSpawned="+IsSpawned);
            ShootFireballServerRpc(Player.LocalInstance.GetComponent<NetworkObject>());


        }
    }

    public override void ResetAbility()
    {
    }

    public override void SetAnimations()
    {
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootFireballServerRpc(NetworkObjectReference playerNOR)
    {
        Debug.Log("Shoot Fireball");
        playerNOR.TryGet(out NetworkObject playerNO);
        Player player = playerNO.GetComponent<Player>();

        Transform fireballPrefab = Resources.Load<Transform>("Projectiles/FireProjectile");
        fireballPrefab.position = player.playerLook.cam.transform.position + (player.playerLook.cam.transform.forward * 2);

        Transform fireballObj = Instantiate(fireballPrefab);
        fireballObj.GetComponent<BaseProjectile>().playerSourceNO = player.GetComponent<NetworkObject>();
        fireballObj.GetComponent<NetworkObject>().Spawn();

        fireballObj.GetComponent<Rigidbody>().velocity = player.playerLook.GetLookDirection() * 20;
        fireballObj.GetComponent<BaseProjectile>().hasBeenfired = true;

    }
}
