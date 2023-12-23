public interface IInteractable
{
    public void BaseInteract();

    protected virtual void Interact() { }

    public string GetPromptMessage();
    public void SetPromptMessage(string promptMessage);
}