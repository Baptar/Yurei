using StarterAssets;

public interface IInteractable
{
    string InteractionPrompt { get; set;  }

    void Hovered();
    void UnHovered();
    void Interact(ThirdPersonController player);
    bool CanInteract { get; set; }
}