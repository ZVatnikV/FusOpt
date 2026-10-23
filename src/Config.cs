using MelonLoader;
using MelonLoader.Preferences;

namespace FusOpt
{
    public static class Config
    {
        public static MelonPreferences_Category Category;

        public static MelonPreferences_Entry<bool> Enabled;
        public static MelonPreferences_Entry<int> Preset;

        public static MelonPreferences_Entry<bool> PropCull;
        public static MelonPreferences_Entry<bool> RigidbodySleep;
        public static MelonPreferences_Entry<bool> ParticleCull;
        public static MelonPreferences_Entry<bool> ShadowCull;
        public static MelonPreferences_Entry<bool> QualityTweaks;
        public static MelonPreferences_Entry<bool> AdaptiveQuality;
        public static MelonPreferences_Entry<bool> IncrementalGc;
        public static MelonPreferences_Entry<bool> OcclusionCulling;

        public static MelonPreferences_Entry<float> NearDistance;
        public static MelonPreferences_Entry<float> FarDistance;
        public static MelonPreferences_Entry<float> ScanInterval;
        public static MelonPreferences_Entry<int> MaxActiveProps;
        public static MelonPreferences_Entry<float> TargetFps;

        public static MelonPreferences_Entry<bool> HostAutoClean;
        public static MelonPreferences_Entry<float> HostCleanInterval;
        public static MelonPreferences_Entry<int> HostPropThreshold;
        public static MelonPreferences_Entry<bool> HostMemoryClean;

        public static void Setup()
        {
            Category = MelonPreferences.CreateCategory("FusOpt");

            Enabled = Category.CreateEntry("Enabled", true, "Master enable");
            Preset = Category.CreateEntry("Preset", 2, "0 Balanced, 1 Performance, 2 Crowded, 3 Extreme");

            PropCull = Category.CreateEntry("PropCull", true, "Cull distant prop renderers");
            RigidbodySleep = Category.CreateEntry("RigidbodySleep", true, "Sleep distant rigidbodies");
            ParticleCull = Category.CreateEntry("ParticleCull", true, "Pause distant particles");
            ShadowCull = Category.CreateEntry("ShadowCull", true, "Disable distant shadows");
            QualityTweaks = Category.CreateEntry("QualityTweaks", true, "Apply crowded-lobby quality tweaks");
            AdaptiveQuality = Category.CreateEntry("AdaptiveQuality", true, "Scale quality from FPS");
            IncrementalGc = Category.CreateEntry("IncrementalGC", true, "Use incremental GC");
            OcclusionCulling = Category.CreateEntry("OcclusionCulling", true, "Force occlusion culling");

            NearDistance = Category.CreateEntry("NearDistance", 18f, "Full detail distance");
            FarDistance = Category.CreateEntry("FarDistance", 55f, "Cull / sleep distance");
            ScanInterval = Category.CreateEntry("ScanInterval", 0.35f, "Seconds between prop scans");
            MaxActiveProps = Category.CreateEntry("MaxActiveProps", 80, "Soft budget for nearby props");
            TargetFps = Category.CreateEntry("TargetFPS", 72f, "Adaptive quality target FPS");

            HostAutoClean = Category.CreateEntry("HostAutoClean", false, "Host: auto despawn clutter");
            HostCleanInterval = Category.CreateEntry("HostCleanInterval", 180f, "Host: auto clean seconds");
            HostPropThreshold = Category.CreateEntry("HostPropThreshold", 120, "Host: clean when prop count exceeds");
            HostMemoryClean = Category.CreateEntry("HostMemoryClean", true, "Unload unused assets after clean");

            ApplyPreset(Preset.Value, writeDistances: false);
        }

        public static void Save()
        {
            try
            {
                MelonPreferences.Save();
            }
            catch
            {
                // Preferences file may be locked briefly.
            }
        }

        public static void ApplyPreset(int preset, bool writeDistances = true)
        {
            Preset.Value = preset;

            switch (preset)
            {
                case 0: // Balanced
                    if (writeDistances)
                    {
                        NearDistance.Value = 24f;
                        FarDistance.Value = 70f;
                        MaxActiveProps.Value = 120;
                    }
                    PropCull.Value = true;
                    RigidbodySleep.Value = true;
                    ParticleCull.Value = true;
                    ShadowCull.Value = true;
                    QualityTweaks.Value = true;
                    AdaptiveQuality.Value = false;
                    break;

                case 1: // Performance
                    if (writeDistances)
                    {
                        NearDistance.Value = 20f;
                        FarDistance.Value = 60f;
                        MaxActiveProps.Value = 90;
                    }
                    PropCull.Value = true;
                    RigidbodySleep.Value = true;
                    ParticleCull.Value = true;
                    ShadowCull.Value = true;
                    QualityTweaks.Value = true;
                    AdaptiveQuality.Value = true;
                    break;

                case 3: // Extreme
                    if (writeDistances)
                    {
                        NearDistance.Value = 12f;
                        FarDistance.Value = 35f;
                        MaxActiveProps.Value = 40;
                    }
                    PropCull.Value = true;
                    RigidbodySleep.Value = true;
                    ParticleCull.Value = true;
                    ShadowCull.Value = true;
                    QualityTweaks.Value = true;
                    AdaptiveQuality.Value = true;
                    break;

                default: // Crowded (2)
                    if (writeDistances)
                    {
                        NearDistance.Value = 16f;
                        FarDistance.Value = 48f;
                        MaxActiveProps.Value = 70;
                    }
                    PropCull.Value = true;
                    RigidbodySleep.Value = true;
                    ParticleCull.Value = true;
                    ShadowCull.Value = true;
                    QualityTweaks.Value = true;
                    AdaptiveQuality.Value = true;
                    break;
            }

            Save();
        }

        public static string PresetName(int preset)
        {
            return preset switch
            {
                0 => "Balanced",
                1 => "Performance",
                3 => "Extreme",
                _ => "Crowded"
            };
        }
    }
}
