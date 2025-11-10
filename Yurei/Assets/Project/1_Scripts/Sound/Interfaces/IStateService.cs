using UnityEngine;

public interface IStateService
{
    void SetState(string stateGroup, string state);
    string GetCurrentState(string stateGroup);
}