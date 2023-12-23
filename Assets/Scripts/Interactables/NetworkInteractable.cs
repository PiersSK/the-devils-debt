using UnityEngine;
using Unity.Netcode;

public class NetworkInteractable : NetworkBehaviour, IInteractable
{
    [SerializeField] private string promptMessage;

    public void BaseInteract()
    {
        Interact();
    }

    public string GetPromptMessage()
    {
        return promptMessage;
    }

    public void SetPromptMessage(string promptMessage)
    {
        this.promptMessage = promptMessage;
    }

    protected virtual void Interact()
    {
    }
}
