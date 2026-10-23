using System;
using LabFusion.Network;
using LabFusion.Utilities;
using UnityEngine;

namespace FusOpt.Systems
{
    public sealed class HostCleaner
    {
        private float _lastClean = -999f;
        private float _lastThresholdCheck;

        public float SecondsUntilClean
        {
            get
            {
                if (!Config.HostAutoClean.Value)
                    return -1f;
                float remain = Config.HostCleanInterval.Value - (Time.unscaledTime - _lastClean);
                return Mathf.Max(0f, remain);
            }
        }

        public bool CanHostAct()
        {
            try
            {
                return NetworkInfo.HasServer && NetworkInfo.IsHost;
            }
            catch
            {
                return false;
            }
        }

        public void Tick()
        {
            if (!Config.Enabled.Value || !Config.HostAutoClean.Value)
                return;

            if (!CanHostAct())
                return;

            float now = Time.unscaledTime;

            if (now - _lastThresholdCheck >= 5f)
            {
                _lastThresholdCheck = now;
                int count = EstimatePropCount();
                if (count >= Config.HostPropThreshold.Value)
                {
                    Core.Log.Msg($"[Host] Prop threshold hit ({count}). Cleaning...");
                    CleanNow("threshold");
                    return;
                }
            }

            if (now - _lastClean >= Mathf.Max(30f, Config.HostCleanInterval.Value))
                CleanNow("interval");
        }

        public void CleanNow(string reason = "manual")
        {
            if (!CanHostAct())
            {
                Core.Log.Warning("Host clean requires Fusion host privileges.");
                return;
            }

            try
            {
                PooleeUtilities.DespawnAll();
                _lastClean = Time.unscaledTime;
                Core.Log.Msg($"[Host] DespawnAll completed ({reason}).");

                if (Config.HostMemoryClean.Value)
                    Core.Instance.Quality.SoftMemoryClean();
            }
            catch (Exception ex)
            {
                Core.Log.Error($"Host clean failed: {ex.Message}");
            }
        }

        public void MemoryCleanOnly()
        {
            Core.Instance.Quality.SoftMemoryClean();
            Core.Log.Msg("Memory clean requested.");
        }

        private static int EstimatePropCount()
        {
            try
            {
                var bodies = UnityEngine.Object.FindObjectsOfType<Rigidbody>();
                return bodies == null ? 0 : bodies.Length;
            }
            catch
            {
                return 0;
            }
        }
    }
}
