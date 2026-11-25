using System;
using UnityEngine;

namespace Vitqra
{
    [Serializable]
    public sealed class VitqraSettings
    {
        public bool automaticResearch = true;
        public bool automaticTranslation = true;
        public string language = "device";
        public string provider = "local";
        public int dailyBudgetCents = 0;
        private const string Key = "vitqra.settings.v1";

        public static VitqraSettings Load()
        {
            if (!PlayerPrefs.HasKey(Key)) return new VitqraSettings();
            try
            {
                var value = new VitqraSettings();
                JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(Key, "{}"), value);
                value.dailyBudgetCents = Math.Max(0, value.dailyBudgetCents);
                return value;
            }
            catch (ArgumentException) { return new VitqraSettings(); }
        }

        public void Save()
        {
            dailyBudgetCents = Math.Max(0, dailyBudgetCents);
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(this));
            PlayerPrefs.Save();
        }

        // A future adapter needs a cumulative spending ledger with atomic reservations.
        public bool CanStartResearch(bool providerVerified, bool isLocal, bool paidRequest, int estimatedCostCents)
        {
            if (!automaticResearch || !providerVerified || estimatedCostCents < 0) return false;
            if (isLocal) return !paidRequest && estimatedCostCents == 0;
            if (!paidRequest) return estimatedCostCents == 0;
            return dailyBudgetCents > 0 && estimatedCostCents > 0 && estimatedCostCents <= dailyBudgetCents;
        }
    }
}
