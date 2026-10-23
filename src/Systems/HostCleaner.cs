using System;
using LabFusion.Network;
using LabFusion.Utilities;
using UnityEngine;

namespace FusOpt.Systems
{
    public sealed class HostCleaner
    {
        private float _lastClean = -1f;
        private float _lastThresholdCheck;
        private bool _autoWasEnabled;

        public float SecondsUntilClean
        {
            get
            {
                if (!Config.HostAutoClean.Value || _lastClean < 0f)
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
            if (!Config.Enabled.Value)
            {
                _autoWasEnabled = false;
                return;
            }

            // Arm the timer when auto-clean is turned on — never wipe instantly.
            if (Config.HostAutoClean.Value && !_autoWasEnabled)
            {
                _lastClean = Time.unscaledTime;
                _autoWasEnabled = true;
                Core.Log.Msg("[Host] Auto clean armed — waiting for first interval.");
            }
            else if (!Config.HostAutoClean.Value)
            {
                _autoWasEnabled = false;
            }

            if (!Config.HostAutoClean.Value)
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

            if (_lastClean >= 0f && now - _lastClean >= Mathf.Max(30f, Config.HostCleanInterval.Value))
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
                // Fusion's DespawnAll clears networked props. Prefer manual use;
                // auto-clean should stay off unless you accept lobby wipes.
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
                Rigidbody[] bodies = UnityEngine.Object.FindObjectsOfType<Rigidbody>();
                if (bodies == null)
                    return 0;

                int count = 0;
                for (int i = 0; i < bodies.Length; i++)
                {
                    Rigidbody rb = bodies[i];
                    if (rb == null)
                        continue;

                    try
                    {
                        Transform t = rb.transform;
                        if (t == null)
                            continue;

                        string n = (t.name ?? string.Empty).ToLowerInvariant();
                        if (n.Contains("rigmanager") || n.Contains("playerrep") || n.Contains("controller"))
                            continue;

                        // Skip heavy static scenery.
                        if (rb.isKinematic && rb.mass > 80f)
                            continue;

                        count++;
                    }
                    catch
                    {
                    }
                }

                return count;
            }
            catch
            {
                return 0;
            }
        }
    }
}
