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

    private PlayerInput.OnFootActions input;

    public override void OnNetworkSpawn()
    {

    }

    private void Start()
    {
        input = GetComponent<InputManager>().onFoot;
        mainHand.gameObject.SetActive(false);
        offHand.gameObject.SetActive(false);
        accessory.gameObject.SetActive(false);
        currentEquipped.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (input.EquipMain.IsPressed()) EquipItem(Equipment.InventorySlot.MainHand);
        if (input.EquipOff.IsPressed()) EquipItem(Equipment.InventorySlot.OffHand);
        if (input.EquipAccessory.IsPressed()) EquipItem(Equipment.InventorySlot.Accessory);

        if (input.Attack.IsPressed())
        {
            currentEquipped.PerformAbility();
        } else
        {
            currentEquipped.ResetAbility();
        }

        currentEquipped.SetAnimations();
    }

    private void EquipItem(Equipment.InventorySlot slot)
    {
        currentEquipped.gameObject.SetActive(false);

        switch(slot) {
            case Equipment.InventorySlot.MainHand:
                currentEquipped = mainHand;
                break;
            case Equipment.InventorySlot.OffHand:
                currentEquipped = offHand;
                break;
            case Equipment.InventorySlot.Accessory:
                currentEquipped = accessory;
                break;
        }

        currentEquipped.gameObject.SetActive(true);
    }
}
