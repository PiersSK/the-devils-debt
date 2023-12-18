using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PlayerLook : NetworkBehaviour
{
    [SerializeField] private GameObject camHolder;
    public Camera cam;
    private float xRotation = 0f;

    public float xSensitivity = 30f;
    public float ySensitivity = 30;

    public override void OnNetworkSpawn()
    {
        camHolder.SetActive(IsOwner);
        base.OnNetworkSpawn();
    }

    private void Awake()
    { 
        //Cursor.lockState = CursorLockMode.Locked;    
    }

    public void ProcessLook(Vector2 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * (mouseX) * xSensitivity);
    }
}
