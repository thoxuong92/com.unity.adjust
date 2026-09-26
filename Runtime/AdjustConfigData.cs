using System;
using UnityEngine;
using Unity.Core.Logging;

namespace Unity.Adjust
{
    [Serializable]
    public class AdjustConfigData
    {
        public string AppToken = "";
        public bool IsProduction = true;
        public bool IsDeferredDeeplinkOpeningEnabled = true;
        public bool IsAdServicesEnabled = true;
        public bool IsIdfaReadingEnabled = true;
        public bool IsSkanAttributionEnabled = true;

        public static AdjustConfigData LoadFromResources()
        {
            var config = new AdjustConfigData();
            try
            {
                string platformSuffix = Application.platform == RuntimePlatform.Android ? "_Android" : (Application.platform == RuntimePlatform.IPhonePlayer ? "_iOS" : "");
                TextAsset data = Resources.Load<TextAsset>($"Info{platformSuffix}") ?? Resources.Load<TextAsset>("Info");

                if (data != null && !string.IsNullOrEmpty(data.text))
                {
                    string text = data.text;
                    config.AppToken = ExtractJsonValue(text, "AdjustToken", "AdjustAppToken", "AppToken");
                    string env = ExtractJsonValue(text, "AdjustEnvironment", "Environment");
                    if (!string.IsNullOrEmpty(env) && env.Equals("Sandbox", StringComparison.OrdinalIgnoreCase))
                    {
                        config.IsProduction = false;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[AdjustConfigData] Lỗi đọc cấu hình từ Resources: {ex.Message}");
            }
            return config;
        }

        private static string ExtractJsonValue(string json, params string[] keys)
        {
            if (string.IsNullOrEmpty(json)) return "";
            foreach (var key in keys)
            {
                string searchKey = $"\"{key}\"";
                int idx = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    int colon = json.IndexOf(':', idx + searchKey.Length);
                    if (colon >= 0)
                    {
                        int q1 = json.IndexOf('"', colon + 1);
                        if (q1 >= 0)
                        {
                            int q2 = json.IndexOf('"', q1 + 1);
                            if (q2 > q1)
                            {
                                return json.Substring(q1 + 1, q2 - q1 - 1);
                            }
                        }
                    }
                }
            }
            return "";
        }
    }
}
