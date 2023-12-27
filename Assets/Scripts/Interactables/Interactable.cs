using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptMessage;

    public void BaseInteract()
    {
        Interact();
    }

    public virtual bool CanInteract()
    {
        return true;
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
