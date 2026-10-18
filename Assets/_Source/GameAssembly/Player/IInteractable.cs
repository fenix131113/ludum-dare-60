namespace GameAssembly.Player
{
    public interface IInteractable
    {
        bool IsInteractable();
        void Interact(object sender);
    }
}