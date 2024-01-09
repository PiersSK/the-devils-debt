using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private Image overlayImage;
    [SerializeField] private int plateIndex;
    private bool isActive = false;

    public bool canToggle = true;

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
        if(other.TryGetComponent(out Player player) && canToggle)
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
}
