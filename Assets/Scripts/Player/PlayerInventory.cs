using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
     public Equipment currentEquipped;

    [SerializeField] private Equipment mainHand;
    [SerializeField] private Equipment offHand;
    [SerializeField] private Equipment accessory;

    public override void OnNetworkSpawn()
    {

    }

    private void Update()
    {
        if(GetComponent<InputManager>().onFoot.Attack.IsPressed())
        {
            currentEquipped.PerformAbility();
        } else
        {
            currentEquipped.ResetAbility();
        }

        currentEquipped.SetAnimations();
    }
}
