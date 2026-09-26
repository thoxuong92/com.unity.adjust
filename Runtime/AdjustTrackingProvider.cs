using System;
using System.Collections.Generic;
using AdjustSdk;
using UnityEngine;
using Unity.Core.Logging;
using Unity.Core.Services.Ads;
using Unity.Core.Services.Analytics;
using Unity.Core.Services.Tracking;

namespace Unity.Adjust
{
    /// <summary>
    /// Adapter tích hợp Adjust Attribution, Deep Linking & Analytics với Unity Core Framework.
    /// Quản lý vòng đời SDK, gửi sự kiện In-App Events, và gửi Impression-Level Ad Revenue.
    /// </summary>
    public class AdjustTrackingProvider : ITrackingProvider, IAnalyticsProvider
    {
        public string ProviderName => "Adjust";
        public AdjustConfigData Config { get; private set; }

        private bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoRegister()
        {
            var provider = new AdjustTrackingProvider();
            TrackingService.AddProvider(provider);
            AnalyticsService.AddProvider(provider);
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            AppLogger.Log("[AdjustTrackingProvider] Khởi tạo Adjust SDK...");
            Config = AdjustConfigData.LoadFromResources();

            if (string.IsNullOrEmpty(Config.AppToken))
            {
                AppLogger.LogWarning("[AdjustTrackingProvider] AppToken rỗng. Chạy ở chế độ Mock.");
                _isInitialized = true;
                return;
            }

            try
            {
#if UNITY_ANDROID && UNITY_2019_2_OR_NEWER
                Application.deepLinkActivated += (deeplink) =>
                {
                    AdjustSdk.Adjust.ProcessDeeplink(new AdjustDeeplink(deeplink));
                };

                if (!string.IsNullOrEmpty(Application.absoluteURL))
                {
                    AdjustSdk.Adjust.ProcessDeeplink(new AdjustDeeplink(Application.absoluteURL));
                }
#endif

                var env = Config.IsProduction ? AdjustEnvironment.Production : AdjustEnvironment.Sandbox;
                var adjustConfig = new AdjustConfig(Config.AppToken, env, false)
                {
                    LogLevel = AdjustLogLevel.Info,
                    IsSendingInBackgroundEnabled = false,
                    IsDeferredDeeplinkOpeningEnabled = Config.IsDeferredDeeplinkOpeningEnabled,
                    IsCoppaComplianceEnabled = false,
                    IsCostDataInAttributionEnabled = false,
                    IsPreinstallTrackingEnabled = false,
                    IsPlayStoreKidsComplianceEnabled = false,
                    IsAdServicesEnabled = Config.IsAdServicesEnabled,
                    IsIdfaReadingEnabled = Config.IsIdfaReadingEnabled,
                    IsLinkMeEnabled = false,
                    IsSkanAttributionEnabled = Config.IsSkanAttributionEnabled
                };

                AdjustSdk.Adjust.InitSdk(adjustConfig);
                _isInitialized = true;
                AppLogger.Log($"[AdjustTrackingProvider] Adjust SDK đã khởi tạo thành công (AppToken: {Config.AppToken.Substring(0, Math.Min(4, Config.AppToken.Length))}***).");
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[AdjustTrackingProvider] Lỗi khởi tạo SDK: {ex.Message}");
            }
        }

        public void Shutdown()
        {
            AppLogger.Log("[AdjustTrackingProvider] Đóng Adjust Adapter.");
            _isInitialized = false;
        }

        #region ITrackingProvider
        public void TrackRevenue(AdRevenueInfo revenueInfo)
        {
            if (revenueInfo == null) return;

            try
            {
                string source = "applovin_max_sdk";
                if (!string.IsNullOrEmpty(revenueInfo.Source) && revenueInfo.Source.IndexOf("AdMob", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    source = "admob_sdk";
                }

                var adRevenue = new AdjustAdRevenue(source);
                adRevenue.SetRevenue(revenueInfo.Revenue, string.IsNullOrEmpty(revenueInfo.Currency) ? "USD" : revenueInfo.Currency);
                adRevenue.AdRevenueNetwork = revenueInfo.NetworkName;
                adRevenue.AdRevenueUnit = revenueInfo.AdUnitId;
                adRevenue.AdRevenuePlacement = revenueInfo.Placement;

                AdjustSdk.Adjust.TrackAdRevenue(adRevenue);
                AppLogger.Log($"[AdjustTrackingProvider] TrackAdRevenue: {revenueInfo.Revenue} {revenueInfo.Currency} ({revenueInfo.Format})");
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[AdjustTrackingProvider] Lỗi log Ad Revenue: {ex.Message}");
            }
        }

        public void TrackEvent(string eventToken, double? revenue = null, string currency = null)
        {
            if (string.IsNullOrEmpty(eventToken)) return;

            try
            {
                var adjustEvent = new AdjustEvent(eventToken);
                if (revenue.HasValue)
                {
                    adjustEvent.SetRevenue(revenue.Value, string.IsNullOrEmpty(currency) ? "USD" : currency);
                }

                AdjustSdk.Adjust.TrackEvent(adjustEvent);
                AppLogger.Log($"[AdjustTrackingProvider] TrackEvent: {eventToken}");
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[AdjustTrackingProvider] Lỗi TrackEvent: {ex.Message}");
            }
        }
        #endregion

        #region IAnalyticsProvider
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            try
            {
                var adjustEvent = new AdjustEvent(eventName);
                if (parameters != null)
                {
                    foreach (var kvp in parameters)
                    {
                        if (kvp.Value != null)
                        {
                            adjustEvent.AddCallbackParameter(kvp.Key, kvp.Value.ToString());
                        }
                    }
                }

                AdjustSdk.Adjust.TrackEvent(adjustEvent);
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[AdjustTrackingProvider] Lỗi LogEvent: {ex.Message}");
            }
        }

        public void SetUserProperty(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            AdjustSdk.Adjust.AddGlobalCallbackParameter(propertyName, propertyValue);
        }

        public void SetUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;
            AdjustSdk.Adjust.AddGlobalPartnerParameter("user_id", userId);
        }
        #endregion
    }
}
