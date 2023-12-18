using UnityEngine;
using Unity.Netcode;

public class PlayerMotor : NetworkBehaviour
{
    public float speed = 5.0f;
    public float gravity = -9.8f;
    public float jumpHeight = 1.0f;

    private CharacterController controller;

    private bool isMoving;
    private bool isGrounded;
    private Vector3 playerVelocity;

    // Start is called before the first frame update
    void Start()
    {

        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        isGrounded = controller.isGrounded;
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
        if (isGrounded)
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
    }
}
