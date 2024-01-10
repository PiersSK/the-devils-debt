using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PressurePlate : NetworkBehaviour
{
    [SerializeField] private Image overlayImage;
    [SerializeField] private int plateIndex;
    private bool isActive = false; // Sync

    public bool canToggle = true; // Sync

    private void Start()
    {
        PuzzleController.Instance.OnPuzzleComplete += Instance_OnPuzzleComplete;
    }

    private void Instance_OnPuzzleComplete(object sender, System.EventArgs e)
    {
        canToggle = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player player) && player == Player.LocalInstance && canToggle)
        {
            TogglePlateActiveServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TogglePlateActiveServerRpc() // Client toggles off immediately?
    {

        TogglePlateActiveClientRpc();
    }

    [ClientRpc]
    private void TogglePlateActiveClientRpc()
    {

        isActive = !isActive;
        if (overlayImage != null)
        {
            overlayImage.color = isActive ? Color.white : new Color(0.2f, 0.2f, 0.2f);
        }

        if (isActive)
            PuzzleController.Instance.AddToPlayerInput(plateIndex);
        else
            PuzzleController.Instance.RemoveFromPlayerInput(plateIndex);
    }

}
