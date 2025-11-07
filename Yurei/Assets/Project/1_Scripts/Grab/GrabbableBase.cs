using DG.Tweening;
using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class GrabbableBase : MonoBehaviour, IInteractable, IGrabbable
{
    protected Rigidbody rb;
    protected Transform holder;
    
    protected virtual void Awake() => rb = GetComponent<Rigidbody>();
    
    // IInteractable
    public virtual string InteractionPrompt
    {
        get => IsHeld ? "Release (E)" : "Take (E)";
        set { }
    }

    public void Hovered()
    {
        Debug.Log("Hovered");
    }

    public void UnHovered()
    {
        Debug.Log("UnHovered");
    }

    public bool CanInteract { get; set; }
    
    public void Interact(ThirdPersonController player)
    {
        if (IsHeld)
            Release(player);
        else
            Grab(player);
    }
    
    // IGrabbable
    public bool IsHeld { get; protected set; }
    public void Grab(ThirdPersonController player)
    {
        holder = player.holdPoint;
        rb.isKinematic = true;
        transform.SetParent(holder);
        transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutFlash);
        IsHeld = true;
    }

    public virtual void Release(ThirdPersonController player)
    {
        rb.isKinematic = false;
        transform.SetParent(null);
        IsHeld = false;
    }
}