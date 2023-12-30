using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    public enum InventorySlot
    {
        MainHand,
        OffHand,
        Accessory
    }

    [Header("Starting Equipment")]
    [SerializeField] private string startingMain;
    [SerializeField] private string startingOff;
    [SerializeField] private string startingAccessory;

    [Header("Current Equipment")]
    public InventorySlot currentEquipped = InventorySlot.MainHand;

    // Assumes index of equipment always set by InventorySlot enum
    private List<Equipment> equipment;
    private List<HotbarIcon> hotbarIcons;

    [SerializeField] private Vector3 handPos;

    private PlayerInput.OnFootActions input;
    private UIManager ui;

    private void Start()
    {

        // TODO: fix spawning of equipment on player join:
        // Currently, host correctly spawns and parents objects to players
        // BUT the client spawns dupes, and fails to parent. On the client, the parented
        // objects fail to follow the player?

        equipment = new List<Equipment>() { null, null, null };
        hotbarIcons = new List<HotbarIcon>() { null, null, null };

        // Get other class references
        ui = UIManager.Instance;
        input = GetComponent<InputManager>().onFoot;

        hotbarIcons[(int)InventorySlot.MainHand] = ui.hotbarMain;
        hotbarIcons[(int)InventorySlot.OffHand] = ui.hotbarOff;
        hotbarIcons[(int)InventorySlot.Accessory] = ui.hotbarAccessory;

        if (startingMain != null) SpawnStartingGearServerRpc(startingMain);
        if (startingOff != null) SpawnStartingGearServerRpc(startingOff);
        if (startingAccessory != null) SpawnStartingGearServerRpc(startingAccessory);

        // Initial setup of equipment
        UpdateHotbarSprites();
        SetEquipmentToHands();
        FollowCameraRotation();
        EquipItemServerRpc(currentEquipped);
    }

    private void Update()
    {
        if (!IsOwner) return;

        FollowCameraRotation();

        // Check for equipment use
        if (input.Attack.IsPressed())
            equipment[(int)currentEquipped].PerformAbility();
        else
            equipment[(int)currentEquipped].ResetAbility();

        equipment[(int)currentEquipped].SetAnimations();
    }

    private void UpdateHotbarSprites()
    {
        foreach (Equipment item in equipment)
        {
            if (item!= null && item.objectSprite != null)
            {
                hotbarIcons[(int)item.inventorySlot].UpdateSprite(item.objectSprite.sprite);
            }
        }
    }

    private void SetEquipmentToHands()
    {
        foreach (Equipment item in equipment)
        {
            if (item != null)
            {
                item.transform.localPosition = handPos;
            }
        }
    }

    private void FollowCameraRotation()
    {
        foreach (Equipment item in equipment)
        {
            if (item != null)
            {
                item.transform.rotation = GetComponent<PlayerLook>().cam.transform.rotation;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnStartingGearServerRpc(string prefabName)
    {
        Debug.Log("Spawning " + prefabName);
        GameObject startingObj = Resources.Load<GameObject>(prefabName);
        GameObject startingNO = Instantiate(startingObj.gameObject);
        startingNO.GetComponent<NetworkObject>().Spawn();
        PickupItem(startingNO);

    }

    [ServerRpc(RequireOwnership = false)]
    public void PickupItemServerRpc(NetworkObjectReference itemNOR)
    {
        itemNOR.TryGet(out NetworkObject itemNO);
        GameObject itemObj = itemNO.gameObject;
        PickupItem(itemObj);
    }

    private void PickupItem(GameObject itemObj)
    {
        Equipment pickup = itemObj.GetComponent<Equipment>();

        Debug.Log(equipment);
        Debug.Log(equipment.Count);
        Debug.Log(equipment[(int)pickup.inventorySlot]);
        // Drop item in that slot
        if (equipment[(int)pickup.inventorySlot] != null)
        {
            Transform itemToDrop = equipment[(int)pickup.inventorySlot].transform;
            itemToDrop.GetComponent<NetworkObject>().TryRemoveParent();
            itemToDrop.GetComponent<Equipment>().UpdateRootPosition(itemObj.GetComponent<Equipment>().rootPosition);
            itemToDrop.GetComponent<Equipment>().ToggleIsPickedUp();
        }

        // Pick up new item
        equipment[(int)pickup.inventorySlot] = pickup.GetComponent<Equipment>();
        pickup.ToggleIsPickedUp();

        // Put new item in hand
        itemObj.GetComponent<NetworkObject>().TrySetParent(transform);

        // Update equipment positions
        SetEquipmentToHands();
        FollowCameraRotation();
        UpdateHotbarSprites();

        //Refresh equipment
        EquipItemClientRpc(currentEquipped);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EquipItemServerRpc(InventorySlot slot)
    {
        EquipItemClientRpc(slot);
    }

    [ClientRpc]
    private void EquipItemClientRpc(InventorySlot slot)
    {
        ResetEquipmentActiveStates();

        currentEquipped = slot;
        hotbarIcons[(int)slot].SetEquipped(true);
        hotbarIcons[(int)slot].itemInSlot = equipment[(int)slot];
        equipment[(int)slot].equippedPlayer = Player.LocalInstance;

        equipment[(int)slot].gameObject.SetActive(true);
    }

    private void ResetEquipmentActiveStates()
    {
        foreach (Equipment item in equipment)
        {
            if (item != null)
            {
                item.gameObject.SetActive(false);
                item.equippedPlayer = Player.LocalInstance;
                hotbarIcons[(int)item.inventorySlot].SetEquipped(false);
            }
        }
    }
}
