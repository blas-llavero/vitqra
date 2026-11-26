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
        public string provider = "local"; // Executable provider guard; not the UI preference.
        public string preferredAi = "local";
        public bool LocalAiSelected => preferredAi == "local";
        public void SelectAi(string value){preferredAi=NormalizeAi(value);}
        public static string NormalizeAi(string value)=>value=="chatgpt"?"chatgpt":"local";
        public int avatarProfile = -1;
        public int dailyBudgetCents = 0;
        private const string Key = "vitqra.settings.v1";

        public static VitqraSettings Load()
        {
            if (!PlayerPrefs.HasKey(Key)) return new VitqraSettings();
            try
            {
                var value = new VitqraSettings();
                JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(Key, "{}"), value);
                value.EnforceLocalOnly();
                return value;
            }
            catch (ArgumentException) { return new VitqraSettings(); }
        }

        public void Save()
        {
            EnforceLocalOnly();
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(this));
            PlayerPrefs.Save();
        }

        private void EnforceLocalOnly()
        {
            // Migrate old preferences, including a previously selected commercial provider.
            provider = "local";
            preferredAi=NormalizeAi(preferredAi);
            dailyBudgetCents = 0;
            // Ignore legacy manual language overrides.
            language = "device";
        }

        // Readiness only; a future inference adapter must independently enforce
        // the reviewed dependency/model registry and prohibit remote fallback.
        public bool CanStartResearch(bool providerVerified, bool isLocal, bool paidRequest, int estimatedCostCents)
        {
            return automaticResearch && LocalAiSelected && providerVerified && provider == "local" &&
                isLocal && !paidRequest && estimatedCostCents == 0;
        }
    }
}
