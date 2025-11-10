
using UnityEngine;
using System.Collections.Generic;
using System;
public class WwiseSoundbankManager : ISoundbankService
{
      private HashSet<string> loadedBanks = new HashSet<string>();

      //Load soundbank synchronously
      public void LoadBank(string bankName, System.Action onComplete = null)
      {
            if (string.IsNullOrEmpty(bankName)) return;

            if (loadedBanks.Contains(bankName))
            {
                  Debug.Log($"Bank {bankName} already loaded");
                  onComplete?.Invoke();
                  return;
            }

            uint bankId;
            AKRESULT result = AkUnitySoundEngine.LoadBank(bankName, out bankId);

            if (result == AKRESULT.AK_Success)
            {
                  loadedBanks.Add(bankName);
                  Debug.Log($"Bank loaded: {bankName}");
                  onComplete?.Invoke();
            }
            else
            {
                  Debug.LogError($"Failed to load bank: {bankName} - {result}");
            }
      }

      //Unload soundbank 
      public void UnloadBank(string bankName)
      {
            if (string.IsNullOrEmpty(bankName)) return;

            if (!loadedBanks.Contains(bankName))
            {
                  Debug.LogWarning($"Bank {bankName} not loaded");
                  return;
            }

            AKRESULT result = AkUnitySoundEngine.UnloadBank(bankName, IntPtr.Zero);

            if (result == AKRESULT.AK_Success)
            {
                  loadedBanks.Remove(bankName);
                  Debug.Log($"Bank unloaded: {bankName}");
            }
      }

      // Verify if a soundbank is loaded
      public bool IsBankLoaded(string bankName)
      {
            return loadedBanks.Contains(bankName);
      }
}