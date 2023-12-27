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
        EquipItem(currentEquipped.inventorySlot);
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
        ResetEquipmentActiveStates();

        switch (slot) {
            case Equipment.InventorySlot.MainHand:
                currentEquipped = mainHand;
                UIManager.Instance.hotbarMain.SetEquipped(true);
                break;
            case Equipment.InventorySlot.OffHand:
                currentEquipped = offHand;
                UIManager.Instance.hotbarOff.SetEquipped(true);
                break;
            case Equipment.InventorySlot.Accessory:
                currentEquipped = accessory;
                UIManager.Instance.hotbarAccessory.SetEquipped(true);
                break;
        }

        currentEquipped.gameObject.SetActive(true);
    }

    private void ResetEquipmentActiveStates()
    {
        mainHand.gameObject.SetActive(false);
        offHand.gameObject.SetActive(false);
        accessory.gameObject.SetActive(false);

        UIManager.Instance.hotbarMain.SetEquipped(false);
        UIManager.Instance.hotbarOff.SetEquipped(false);
        UIManager.Instance.hotbarAccessory.SetEquipped(false);
    }
}
