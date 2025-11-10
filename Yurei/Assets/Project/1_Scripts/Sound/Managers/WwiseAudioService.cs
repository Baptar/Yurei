using UnityEngine;

public class WwiseAudioService : MonoBehaviour, IWwiseAudioService
{
      // Sous-managers
      private WwiseEventManager eventManager;
      private WwiseRTPCManager rtpcManager;
      private WwiseStateManager stateManager;
      private WwiseSwitchManager switchManager;
      private WwiseSoundbankManager soundbankManager;

      public IAudioEventService Events => eventManager;
      public IRTPCService RTPC => rtpcManager;
      public IStateService States => stateManager;
      public ISwitchService Switches => switchManager;
      public ISoundbankService Soundbanks => soundbankManager;

      private void Awake()
      {
            // Initialiser tous les sous-managers
            eventManager = new WwiseEventManager();
            rtpcManager = new WwiseRTPCManager();
            stateManager = new WwiseStateManager();
            switchManager = new WwiseSwitchManager();
            soundbankManager = new WwiseSoundbankManager();

            // S'enregistrer dans le Service Locator
            AudioServices.Register(this);

            DontDestroyOnLoad(gameObject);
      }

      private void OnDestroy()
      {
            // Cleanup
            if (AudioServices.IsInitialized)
            {
                  AudioServices.Reset();
            }
      }
}