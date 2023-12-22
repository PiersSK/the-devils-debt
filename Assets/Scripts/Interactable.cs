using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string promptMessage;

    public void BaseInteract()
    {
        Interact();
    }
    protected virtual void Interact()
    {

    }
}
