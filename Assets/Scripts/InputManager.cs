using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : NetworkBehaviour
{
    private PlayerInput playerInput;
    public PlayerInput.OnFootActions onFoot;

    private PlayerMotor motor;
    private PlayerLook look;
    private PlayerAttack attack;
    // Start is called before the first frame update

    private void Awake()
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;

        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();
        attack = GetComponent<PlayerAttack>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;

        onFoot.Jump.performed += ctx => motor.Jump();
        //onFoot.Attack.performed += ctx => attack.Attack();
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
