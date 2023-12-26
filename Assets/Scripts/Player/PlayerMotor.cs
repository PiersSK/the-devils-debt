using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMotor : NetworkBehaviour
{
    public static PlayerMotor LocalInstance { get; private set; }

    private float speed = 6f;
    [SerializeField] private float baseSpeed = 6f;
    [SerializeField] private float sprintSpeed = 10f;

    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpHeight = 1.0f;

    private CharacterController controller;

    [SerializeField] private float staminaMax = 6f;
    private float currentStamina;
    private bool isSprinting;
    

    public bool isMoving;
    private bool isGrounded;
    private Vector3 playerVelocity;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner) LocalInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentStamina = staminaMax;
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;

        if(isSprinting)
        {
            currentStamina -= Time.deltaTime;
            if (currentStamina <= 0 || !isMoving)
            {
                ToggleSprint();
            }
        } else
        {
            currentStamina += Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, staminaMax);
        }

        UIManager.Instance.stamina.UpdateBar(currentStamina, staminaMax);
    }

    public void ProcessMove(Vector2 input)
    {
        isMoving = input != Vector2.zero;
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;

        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);
        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void Jump()
    {
        Debug.Log("Jump");
        if (isGrounded)
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
    }

    public void ToggleSprint()
    {
        isSprinting = !isSprinting;

        speed = isSprinting ? sprintSpeed : baseSpeed;
    }
}
