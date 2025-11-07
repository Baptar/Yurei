using StarterAssets;
using UnityEngine;

public class InteractExemple : MonoBehaviour, IInteractable
{
    public string interactText;
    public bool isInteractable = true;
    
    private Renderer renderer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InteractionPrompt = interactText;
        CanInteract = isInteractable;
        
        renderer = GetComponent<Renderer>();
    }

    private void SetShowMaterialColor(bool show)
    {
        if (show)
            renderer.material.EnableKeyword("_SHOWCOLOR");
        else renderer.material.DisableKeyword("_SHOWCOLOR");
    }
    

    public string InteractionPrompt { get; set; }
    public bool CanInteract { get; set; }
    
    public void Hovered()
    {
        SetShowMaterialColor(true);
    }

    public void UnHovered()
    {
        SetShowMaterialColor(false);
    }

    public void Interact(ThirdPersonController player)
    { 
        Destroy(gameObject);
    }

}
