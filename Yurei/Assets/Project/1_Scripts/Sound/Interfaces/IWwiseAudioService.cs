using UnityEngine;

public interface IWwiseAudioService
{
    IAudioEventService Events { get; }
    IRTPCService RTPC { get; }
    IStateService States { get; }
    ISwitchService Switches { get; }
    ISoundbankService Soundbanks { get; }
}