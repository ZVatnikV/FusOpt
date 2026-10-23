using System;
using BoneLib.BoneMenu;
using UnityEngine;

namespace FusOpt
{
    public static class BoneMenuUI
    {
        private static Page _root;
        private static FunctionElement _status;

        public static void Setup()
        {
            try
            {
                _root = Page.Root.CreatePage("FusOpt", new Color(0.15f, 0.85f, 0.95f), 0, true);

                _root.CreateBool("Enabled", Color.white, Config.Enabled.Value, v =>
                {
                    Config.Enabled.Value = v;
                    Config.Save();
                    if (!v)
                        Core.Instance.Props.RestoreAll();
                    else
                        Core.Instance.Quality.ApplyStaticTweaks();
                    RefreshStatus();
                });

                _root.CreateInt("Preset", new Color(1f, 0.85f, 0.2f), Config.Preset.Value, 1, 0, 3, v =>
                {
                    Config.ApplyPreset(v);
                    Core.Instance.Quality.ApplyStaticTweaks();
                    RefreshStatus();
                });

                _status = _root.CreateFunction("Status", Color.gray, RefreshStatus);

                var client = _root.CreatePage("Client Optimizations", new Color(0.4f, 0.9f, 0.6f));
                client.CreateBool("Prop Cull", Color.white, Config.PropCull.Value, v => { Config.PropCull.Value = v; Config.Save(); });
                client.CreateBool("Rigidbody Sleep", Color.white, Config.RigidbodySleep.Value, v => { Config.RigidbodySleep.Value = v; Config.Save(); });
                client.CreateBool("Particle Cull", Color.white, Config.ParticleCull.Value, v => { Config.ParticleCull.Value = v; Config.Save(); });
                client.CreateBool("Shadow Cull", Color.white, Config.ShadowCull.Value, v => { Config.ShadowCull.Value = v; Config.Save(); });
                client.CreateBool("Quality Tweaks", Color.white, Config.QualityTweaks.Value, v =>
                {
                    Config.QualityTweaks.Value = v;
                    Config.Save();
                    if (v)
                        Core.Instance.Quality.ApplyStaticTweaks();
                });
                client.CreateBool("Adaptive Quality", Color.white, Config.AdaptiveQuality.Value, v => { Config.AdaptiveQuality.Value = v; Config.Save(); });
                client.CreateBool("Incremental GC", Color.white, Config.IncrementalGc.Value, v =>
                {
                    Config.IncrementalGc.Value = v;
                    Config.Save();
                    if (v)
                        Core.Instance.Quality.ApplyStaticTweaks();
                });
                client.CreateBool("Occlusion Culling", Color.white, Config.OcclusionCulling.Value, v =>
                {
                    Config.OcclusionCulling.Value = v;
                    Config.Save();
                    if (v)
                        Core.Instance.Quality.ApplyOcclusion();
                });

                client.CreateFloat("Near Distance", Color.cyan, Config.NearDistance.Value, 1f, 4f, 60f, v =>
                {
                    Config.NearDistance.Value = v;
                    Config.Save();
                });
                client.CreateFloat("Far Distance", Color.cyan, Config.FarDistance.Value, 1f, 10f, 120f, v =>
                {
                    Config.FarDistance.Value = v;
                    Config.Save();
                });
                client.CreateFloat("Scan Interval", Color.yellow, Config.ScanInterval.Value, 0.05f, 0.1f, 2f, v =>
                {
                    Config.ScanInterval.Value = v;
                    Config.Save();
                });
                client.CreateInt("Max Active Props", Color.yellow, Config.MaxActiveProps.Value, 5, 20, 300, v =>
                {
                    Config.MaxActiveProps.Value = v;
                    Config.Save();
                });
                client.CreateFloat("Target FPS", Color.green, Config.TargetFps.Value, 1f, 30f, 144f, v =>
                {
                    Config.TargetFps.Value = v;
                    Config.Save();
                });

                var host = _root.CreatePage("Host Tools", new Color(1f, 0.45f, 0.35f));
                host.CreateBool("Auto Clean", Color.white, Config.HostAutoClean.Value, v =>
                {
                    Config.HostAutoClean.Value = v;
                    Config.Save();
                });
                host.CreateFloat("Clean Interval (s)", Color.yellow, Config.HostCleanInterval.Value, 10f, 30f, 900f, v =>
                {
                    Config.HostCleanInterval.Value = v;
                    Config.Save();
                });
                host.CreateInt("Prop Threshold", Color.yellow, Config.HostPropThreshold.Value, 10, 40, 500, v =>
                {
                    Config.HostPropThreshold.Value = v;
                    Config.Save();
                });
                host.CreateBool("Memory Clean After", Color.white, Config.HostMemoryClean.Value, v =>
                {
                    Config.HostMemoryClean.Value = v;
                    Config.Save();
                });
                host.CreateFunction("Clean Server Now (wipes props)", new Color(1f, 0.3f, 0.3f), () =>
                {
                    Core.Instance.Host.CleanNow("menu");
                    RefreshStatus();
                });
                host.CreateFunction("Memory Clean Only", new Color(0.8f, 0.6f, 1f), () =>
                {
                    Core.Instance.Host.MemoryCleanOnly();
                    RefreshStatus();
                });

                _root.CreateFunction("Apply Preset Now", new Color(0.5f, 0.8f, 1f), () =>
                {
                    Config.ApplyPreset(Config.Preset.Value);
                    Core.Instance.Quality.ApplyStaticTweaks();
                    RefreshStatus();
                });

                RefreshStatus();
            }
            catch (Exception ex)
            {
                Core.Log.Warning($"BoneMenu setup failed: {ex.Message}");
            }
        }

        public static void RefreshStatus()
        {
            if (_status == null)
                return;

            try
            {
                string preset = Config.PresetName(Config.Preset.Value);
                string host = Core.Instance.Host.CanHostAct() ? "HOST" : "client";
                _status.ElementName =
                    $"FPS {Core.Instance.CurrentFps:0} | props {Core.Instance.Props.TrackedCount} (culled {Core.Instance.Props.CulledCount}) | {preset} | {host}";
            }
            catch
            {
            }
        }
    }
}
