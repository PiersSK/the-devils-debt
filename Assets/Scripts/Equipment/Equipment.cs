using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Equipment : NetworkBehaviour
{
    public enum InventorySlot
    {
        MainHand,
        OffHand,
        Accessory
    }

    [Header("General")]
    public Transform prefab;
    public InventorySlot inventorySlot;

    public abstract void PerformAbility();
    public abstract void SetAnimations();
    public abstract void ResetAbility();
}
