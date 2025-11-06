using StarterAssets;

public interface IGrabbable
{
    bool IsHeld { get; }
    void Grab(ThirdPersonController player);
    void Release(ThirdPersonController player);
}