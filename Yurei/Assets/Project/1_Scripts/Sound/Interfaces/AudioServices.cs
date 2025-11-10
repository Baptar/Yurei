using UnityEngine;

public static class AudioServices
{
    private static IWwiseAudioService audioService;
    
    // Accès aux sous-services
    public static IAudioEventService Events => audioService?.Events;
    public static IRTPCService RTPC => audioService?.RTPC;
    public static IStateService States => audioService?.States;
    public static ISwitchService Switches => audioService?.Switches;
    public static ISoundbankService Soundbanks => audioService?.Soundbanks;
    
    // Vérification de disponibilité
    public static bool IsInitialized => audioService != null;

    // Enregistrement du service
    public static void Register(IWwiseAudioService service)
    {
        if (audioService != null)
        {
            Debug.LogWarning("AudioServices already registered.");
        }
        audioService = service;
        Debug.Log("Wwise Audio Services registered successfully");
    }
    
    public static void Reset()
    {
        audioService = null;
    }
}
