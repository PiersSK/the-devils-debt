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
        EquipItemServerRpc(currentEquipped.inventorySlot);
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Deal with switching equipment
        if (input.EquipMain.IsPressed()) EquipItemServerRpc(Equipment.InventorySlot.MainHand);
        if (input.EquipOff.IsPressed()) EquipItemServerRpc(Equipment.InventorySlot.OffHand);
        if (input.EquipAccessory.IsPressed()) EquipItemServerRpc(Equipment.InventorySlot.Accessory);

        // Check for equipment use
        if (input.Attack.IsPressed())
        {
            currentEquipped.PerformAbility();
        } else
        {
            currentEquipped.ResetAbility();
        }

        currentEquipped.SetAnimations();
    }

    [ServerRpc(RequireOwnership = false)]
    private void EquipItemServerRpc(Equipment.InventorySlot slot)
    {
        EquipItemClientRpc(slot);
    }

    [ClientRpc]
    private void EquipItemClientRpc(Equipment.InventorySlot slot)
    {
        ResetEquipmentActiveStates();

        switch (slot) {
            case Equipment.InventorySlot.MainHand:
                currentEquipped = mainHand;
                UIManager.Instance.hotbarMain.SetEquipped(true);
                UIManager.Instance.hotbarMain.itemInSlot = mainHand;
                mainHand.equippedPlayer = Player.LocalInstance;
                break;
            case Equipment.InventorySlot.OffHand:
                currentEquipped = offHand;
                UIManager.Instance.hotbarOff.SetEquipped(true);
                UIManager.Instance.hotbarOff.itemInSlot = offHand;
                offHand.equippedPlayer = Player.LocalInstance;
                break;
            case Equipment.InventorySlot.Accessory:
                currentEquipped = accessory;
                UIManager.Instance.hotbarAccessory.SetEquipped(true);
                UIManager.Instance.hotbarAccessory.itemInSlot = accessory;
                accessory.equippedPlayer = Player.LocalInstance;
                break;
        }

        currentEquipped.gameObject.SetActive(true);
    }

    private void ResetEquipmentActiveStates()
    {
        mainHand.gameObject.SetActive(false);
        offHand.gameObject.SetActive(false);
        accessory.gameObject.SetActive(false);

        mainHand.equippedPlayer = Player.LocalInstance;
        offHand.equippedPlayer = Player.LocalInstance;
        accessory.equippedPlayer = Player.LocalInstance;

        UIManager.Instance.hotbarMain.SetEquipped(false);
        UIManager.Instance.hotbarOff.SetEquipped(false);
        UIManager.Instance.hotbarAccessory.SetEquipped(false);


    }
}
