using System;
using BoneLib;
using FusOpt.Systems;
using LabFusion.Network;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(FusOpt.Core), FusOpt.BuildInfo.Name, FusOpt.BuildInfo.Version, FusOpt.BuildInfo.Author, FusOpt.BuildInfo.DownloadLink)]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
[assembly: MelonColor(255, 40, 200, 230)]
[assembly: MelonAdditionalDependencies("BoneLib", "LabFusion")]
[assembly: MelonPriority(500)]

namespace FusOpt
{
    public class Core : MelonMod
    {
        public static Core Instance { get; private set; }
        public static MelonLogger.Instance Log { get; private set; }

        public PropCuller Props { get; private set; }
        public QualityOptimizer Quality { get; private set; }
        public HostCleaner Host { get; private set; }

        public float CurrentFps { get; private set; } = 72f;

        private float _fpsAccum;
        private int _fpsFrames;
        private float _fpsTimer;
        private float _statusTimer;
        private float _adaptiveTimer;

        public override void OnInitializeMelon()
        {
            Instance = this;
            Log = LoggerInstance;

            Log.Msg("========================================");
            Log.Msg("  FusOpt — Fusion Lobby Optimizer");
            Log.Msg($"  v{BuildInfo.Version}  |  open source");
            Log.Msg("========================================");

            Config.Setup();
            Props = new PropCuller();
            Quality = new QualityOptimizer();
            Host = new HostCleaner();

            Quality.ApplyStaticTweaks();
            BoneMenuUI.Setup();

            try
            {
                Hooking.OnLevelLoaded += _ => OnLevelReady();
            }
            catch (Exception ex)
            {
                Log.Warning($"BoneLib level hook failed: {ex.Message}");
            }

            Log.Msg("FusOpt ready. Open BoneMenu → FusOpt");
        }

        private void OnLevelReady()
        {
            Props.OnLevelLoaded();
            Quality.ApplyStaticTweaks();
            Quality.ApplyOcclusion();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            Props.OnLevelLoaded();
            Quality.ApplyOcclusion();
        }

        public override void OnUpdate()
        {
            float dt = Time.unscaledDeltaTime;
            _fpsAccum += dt;
            _fpsFrames++;
            _fpsTimer += dt;

            if (_fpsTimer >= 0.5f)
            {
                CurrentFps = _fpsFrames / Mathf.Max(_fpsAccum, 0.0001f);
                _fpsFrames = 0;
                _fpsAccum = 0f;
                _fpsTimer = 0f;
            }

            Host.Tick();

            _adaptiveTimer += dt;
            if (_adaptiveTimer >= 1f)
            {
                _adaptiveTimer = 0f;
                float players = EstimatePlayers();
                Quality.TickAdaptive(CurrentFps, players);
            }

            Transform viewer = GetViewer();
            if (viewer != null)
                Props.Tick(Time.unscaledTime, viewer);

            _statusTimer += dt;
            if (_statusTimer >= 2f)
            {
                _statusTimer = 0f;
                BoneMenuUI.RefreshStatus();
            }
        }

        private static float EstimatePlayers()
        {
            try
            {
                if (!NetworkInfo.HasServer)
                    return 1f;
            }
            catch
            {
                return 1f;
            }

            // Conservative default when Fusion player enumeration differs by version.
            return 12f;
        }

        private static Transform GetViewer()
        {
            try
            {
                if (Player.Head != null)
                    return Player.Head;
            }
            catch
            {
            }

            try
            {
                var cam = Camera.main;
                if (cam != null)
                    return cam.transform;
            }
            catch
            {
            }

            return null;
        }
    }
}
