using UnityEngine;

public interface ISoundbankService
{
    void LoadBank(string bankName, System.Action onComplete = null);
    void UnloadBank(string bankName);
    bool IsBankLoaded(string bankName);
    //void LoadBankAsync(string bankName, System.Action<bool> callback);
}