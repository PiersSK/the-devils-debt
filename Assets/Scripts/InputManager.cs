using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : NetworkBehaviour
{
    private PlayerInput playerInput;
    public PlayerInput.OnFootActions onFoot;

    private PlayerMotor motor;
    private PlayerLook look;

    private PlayerInventory inventory;
    // Start is called before the first frame update

    private bool cursorIsLocked = true;

    private void Awake()
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;

        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();
        inventory = GetComponent<PlayerInventory>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;

        onFoot.Jump.performed += ctx => motor.Jump();
        onFoot.Sprint.performed += ctx => motor.ToggleSprint();
        onFoot.LockCursor.performed += ctx => ToggleCursorLock();

        onFoot.EquipMain.performed += ctx => inventory.EquipItemServerRpc(PlayerInventory.InventorySlot.MainHand);
        onFoot.EquipOff.performed += ctx => inventory.EquipItemServerRpc(PlayerInventory.InventorySlot.OffHand);
        onFoot.EquipAccessory.performed += ctx => inventory.EquipItemServerRpc(PlayerInventory.InventorySlot.Accessory);

    }

    private void ToggleCursorLock()
    {
        Cursor.lockState = cursorIsLocked ? CursorLockMode.None : CursorLockMode.Locked;
        cursorIsLocked = !cursorIsLocked;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        look.ProcessLook(onFoot.Look.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        onFoot.Enable();
    }

    private void OnDisable()
    {
        onFoot.Disable();
    }
}
