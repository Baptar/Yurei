using UnityEngine;
using System.Collections.Generic;
public class WwiseStateManager : IStateService
{
    private Dictionary<string, string> currentStates = new Dictionary<string, string>();
    
    public void SetState(string stateGroup, string state)
    {
        if (string.IsNullOrEmpty(stateGroup) || string.IsNullOrEmpty(state)) return;
        
        AKRESULT result = AkUnitySoundEngine.SetState(stateGroup, state);
        
        if (result == AKRESULT.AK_Success)
        {
            currentStates[stateGroup] = state;
            Debug.Log($"State set: {stateGroup} -> {state}");
        }
        else
        {
            Debug.LogError($"Failed to set state: {stateGroup} -> {state}");
        }
    }
    
    public string GetCurrentState(string stateGroup)
    {
        return currentStates.ContainsKey(stateGroup) ? currentStates[stateGroup] : null;
    }
}