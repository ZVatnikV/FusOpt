using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace FusOpt.Systems
{
    public sealed class QualityOptimizer
    {
        private float _appliedShadowDistance = -1f;
        private int _appliedPixelLights = -1;

        public void ApplyStaticTweaks()
        {
            if (!Config.Enabled.Value || !Config.QualityTweaks.Value)
                return;

            try
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
                Application.backgroundLoadingPriority = ThreadPriority.Low;
                QualitySettings.asyncUploadTimeSlice = 2;
                QualitySettings.asyncUploadBufferSize = 32;
                QualitySettings.asyncUploadPersistentBuffer = true;
                QualitySettings.particleRaycastBudget = 128;
                QualitySettings.shadowCascades = 1;
                QualitySettings.softParticles = false;
            }
            catch (Exception ex)
            {
                Core.Log.Warning($"Quality static tweaks failed: {ex.Message}");
            }

            if (Config.IncrementalGc.Value)
            {
                try
                {
                    GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
                    GarbageCollector.incrementalTimeSliceNanoseconds = 2_000_000UL;
                }
                catch (Exception ex)
                {
                    Core.Log.Warning($"Incremental GC unavailable: {ex.Message}");
                }
            }
        }

        public void ApplyOcclusion()
        {
            if (!Config.Enabled.Value || !Config.OcclusionCulling.Value)
                return;

            try
            {
                var cameras = Camera.allCameras;
                for (int i = 0; i < cameras.Length; i++)
                {
                    var cam = cameras[i];
                    if (cam != null && cam.enabled)
                        cam.useOcclusionCulling = true;
                }
            }
            catch (Exception ex)
            {
                Core.Log.Warning($"Occlusion tweak failed: {ex.Message}");
            }
        }

        public void TickAdaptive(float fps, float playerScale)
        {
            if (!Config.Enabled.Value || !Config.QualityTweaks.Value)
                return;

            float target = Mathf.Max(30f, Config.TargetFps.Value);
            float pressure = 1f;

            if (Config.AdaptiveQuality.Value)
            {
                // Below target -> more aggressive; above target -> relax a bit.
                pressure = Mathf.Clamp(target / Mathf.Max(fps, 1f), 0.65f, 1.75f);
                pressure *= Mathf.Lerp(1f, 1.35f, Mathf.Clamp01((playerScale - 8f) / 20f));
            }

            float shadowDist = Mathf.Clamp(55f / pressure, 12f, 70f);
            int pixelLights = pressure > 1.25f ? 1 : (pressure > 1.05f ? 2 : 4);

            if (Mathf.Abs(shadowDist - _appliedShadowDistance) > 0.5f)
            {
                try
                {
                    QualitySettings.shadowDistance = shadowDist;
                    _appliedShadowDistance = shadowDist;
                }
                catch
                {
                }
            }

            if (pixelLights != _appliedPixelLights)
            {
                try
                {
                    QualitySettings.pixelLightCount = pixelLights;
                    _appliedPixelLights = pixelLights;
                }
                catch
                {
                }
            }
        }

        public void SoftMemoryClean()
        {
            try
            {
                Resources.UnloadUnusedAssets();
                GC.Collect(0);
            }
            catch (Exception ex)
            {
                Core.Log.Warning($"Memory clean failed: {ex.Message}");
            }
        }
    }
}
