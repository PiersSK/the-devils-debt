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
        if (!onCooldown && Player.LocalInstance.playerMana.currentMana.Value >= manaCost)
        {
            Debug.Log("Shoot Fireball");
            Player.LocalInstance.playerMana.IncrementPlayerManaServerRpc(-(int)manaCost);

            Transform fireballPrefab = Resources.Load<Transform>("Projectiles/FireProjectile");
            fireballPrefab.position = Player.LocalInstance.playerLook.cam.transform.position + (Player.LocalInstance.playerLook.cam.transform.forward * 2);

            Transform fireballObj = Instantiate(fireballPrefab);
            fireballObj.GetComponent<BaseProjectile>().playerSourceNO = Player.LocalInstance.GetComponent<NetworkObject>();
            fireballObj.GetComponent<NetworkObject>().Spawn();

            fireballObj.GetComponent<Rigidbody>().velocity = Player.LocalInstance.playerLook.GetLookDirection() * 20;

            onCooldown = true;
            UIManager.Instance.hotbarOff.PutOnCooldown(firerate);
        } 
    }

    public override void ResetAbility()
    {
    }

    public override void SetAnimations()
    {
    }
}
