using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FusOpt.Systems
{
    /// <summary>
    /// Client-side visual/physics relief for crowded Fusion lobbies.
    /// Does not despawn networked entities — only reduces local cost.
    /// </summary>
    public sealed class PropCuller
    {
        private sealed class TrackedProp
        {
            public int Id;
            public Rigidbody Body;
            public Renderer[] Renderers;
            public ParticleSystem[] Particles;
            public bool Culled;
            public bool ShadowsOff;
            public bool ParticlesPaused;
            public ShadowCastingMode[] OriginalShadows;
            public bool[] OriginalRendererEnabled;
            public bool WasKinematic;
            public bool StoredKinematic;
        }

        private readonly Dictionary<int, TrackedProp> _byId = new Dictionary<int, TrackedProp>(256);
        private readonly List<TrackedProp> _props = new List<TrackedProp>(256);
        private float _nextScan;
        private float _nextRefresh = -1f;
        private Vector3 _viewer;
        private int _culledCount;
        private int _trackedCount;

        public int CulledCount => _culledCount;
        public int TrackedCount => _trackedCount;

        public void OnLevelLoaded()
        {
            RestoreAll();
            _byId.Clear();
            _props.Clear();
            _nextRefresh = 0f;
            _nextScan = 0f;
            _culledCount = 0;
            _trackedCount = 0;
        }

        public void Tick(float now, Transform viewer)
        {
            if (!Config.Enabled.Value)
                return;

            if (viewer == null)
                return;

            _viewer = viewer.position;

            if (now >= _nextRefresh)
            {
                RefreshCache();
                _nextRefresh = now + 2.5f;
            }

            if (now < _nextScan)
                return;

            _nextScan = now + Mathf.Max(0.1f, Config.ScanInterval.Value);
            Evaluate();
        }

        private void RefreshCache()
        {
            Rigidbody[] bodies;
            try
            {
                bodies = UnityEngine.Object.FindObjectsOfType<Rigidbody>();
            }
            catch
            {
                return;
            }

            var seen = new HashSet<int>();

            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody rb = bodies[i];
                try
                {
                    if (rb == null)
                        continue;

                    GameObject go = rb.gameObject;
                    if (go == null || !go.activeInHierarchy)
                        continue;

                    string name = go.name ?? string.Empty;
                    if (IsProtectedName(name) || IsUnderPlayerHierarchy(go.transform))
                        continue;

                    if (rb.isKinematic && rb.mass > 80f)
                        continue;

                    int id = rb.GetInstanceID();
                    seen.Add(id);

                    if (_byId.TryGetValue(id, out TrackedProp existing) && existing.Body == rb)
                    {
                        existing.Body = rb;
                        continue;
                    }

                    Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
                    if (renderers == null || renderers.Length == 0)
                        continue;

                    var tracked = new TrackedProp
                    {
                        Id = id,
                        Body = rb,
                        Renderers = renderers,
                        Particles = go.GetComponentsInChildren<ParticleSystem>(true),
                        Culled = false,
                        ShadowsOff = false,
                        ParticlesPaused = false,
                        OriginalShadows = new ShadowCastingMode[renderers.Length],
                        OriginalRendererEnabled = new bool[renderers.Length]
                    };

                    for (int r = 0; r < renderers.Length; r++)
                    {
                        Renderer ren = renderers[r];
                        if (ren == null)
                            continue;
                        tracked.OriginalShadows[r] = ren.shadowCastingMode;
                        tracked.OriginalRendererEnabled[r] = ren.enabled;
                    }

                    _byId[id] = tracked;
                }
                catch
                {
                    // Destroyed Il2Cpp wrappers can throw mid-scan.
                }
            }

            // Drop props that vanished; restore them first if still valid.
            var remove = new List<int>();
            foreach (KeyValuePair<int, TrackedProp> kv in _byId)
            {
                if (!seen.Contains(kv.Key))
                {
                    try
                    {
                        ApplyCull(kv.Value, false);
                        ApplyShadows(kv.Value, false);
                        ApplyParticles(kv.Value, false);
                    }
                    catch
                    {
                    }

                    remove.Add(kv.Key);
                }
            }

            for (int i = 0; i < remove.Count; i++)
                _byId.Remove(remove[i]);

            _props.Clear();
            foreach (TrackedProp prop in _byId.Values)
                _props.Add(prop);

            _trackedCount = _props.Count;
        }

        private void Evaluate()
        {
            float near = Mathf.Max(4f, Config.NearDistance.Value);
            float far = Mathf.Max(near + 2f, Config.FarDistance.Value);
            int budget = Mathf.Max(10, Config.MaxActiveProps.Value);

            _props.Sort((a, b) => DistanceSq(a).CompareTo(DistanceSq(b)));

            int kept = 0;
            int culled = 0;

            for (int i = 0; i < _props.Count; i++)
            {
                TrackedProp prop = _props[i];
                if (prop.Body == null)
                    continue;

                float dist = Mathf.Sqrt(DistanceSq(prop));
                bool inNear = dist <= near;
                bool overBudget = !inNear && kept >= budget;
                bool farAway = dist > far;
                bool mid = dist > near;

                // Never strip detail inside the near bubble — keeps grabs usable.
                bool shouldCull = Config.PropCull.Value && !inNear && (farAway || overBudget);
                bool shouldSleep = Config.RigidbodySleep.Value && !inNear && (farAway || overBudget);
                bool shouldShadowOff = Config.ShadowCull.Value && !inNear && (mid || overBudget);
                bool shouldPauseParticles = Config.ParticleCull.Value && !inNear && (mid || overBudget);

                // Don't fight active motion.
                if (shouldSleep && IsMoving(prop.Body))
                    shouldSleep = false;

                ApplyCull(prop, shouldCull);
                ApplySleep(prop, shouldSleep);
                ApplyShadows(prop, shouldShadowOff);
                ApplyParticles(prop, shouldPauseParticles);

                if (shouldCull)
                    culled++;
                else
                    kept++;
            }

            _culledCount = culled;
        }

        private float DistanceSq(TrackedProp prop)
        {
            try
            {
                return (prop.Body.worldCenterOfMass - _viewer).sqrMagnitude;
            }
            catch
            {
                return float.MaxValue;
            }
        }

        private static bool IsMoving(Rigidbody rb)
        {
            try
            {
                return rb.velocity.sqrMagnitude > 0.05f || rb.angularVelocity.sqrMagnitude > 0.05f;
            }
            catch
            {
                return true;
            }
        }

        private static void ApplyCull(TrackedProp prop, bool cull)
        {
            if (prop.Culled == cull)
                return;

            for (int i = 0; i < prop.Renderers.Length; i++)
            {
                Renderer ren = prop.Renderers[i];
                if (ren == null)
                    continue;

                if (cull)
                    ren.enabled = false;
                else
                    ren.enabled = prop.OriginalRendererEnabled[i];
            }

            prop.Culled = cull;
        }

        private static void ApplySleep(TrackedProp prop, bool sleep)
        {
            Rigidbody rb = prop.Body;
            if (rb == null)
                return;

            try
            {
                if (sleep)
                {
                    if (!prop.StoredKinematic)
                    {
                        prop.WasKinematic = rb.isKinematic;
                        prop.StoredKinematic = true;
                    }

                    if (!rb.IsSleeping())
                        rb.Sleep();
                }
                else if (prop.StoredKinematic)
                {
                    if (!prop.WasKinematic)
                        rb.WakeUp();
                    prop.StoredKinematic = false;
                }
            }
            catch
            {
            }
        }

        private static void ApplyShadows(TrackedProp prop, bool off)
        {
            if (prop.ShadowsOff == off)
                return;

            for (int i = 0; i < prop.Renderers.Length; i++)
            {
                Renderer ren = prop.Renderers[i];
                if (ren == null)
                    continue;

                if (off)
                    ren.shadowCastingMode = ShadowCastingMode.Off;
                else
                    ren.shadowCastingMode = prop.OriginalShadows[i];
            }

            prop.ShadowsOff = off;
        }

        private static void ApplyParticles(TrackedProp prop, bool pause)
        {
            if (prop.Particles == null || prop.Particles.Length == 0)
                return;

            if (prop.ParticlesPaused == pause)
                return;

            for (int i = 0; i < prop.Particles.Length; i++)
            {
                ParticleSystem ps = prop.Particles[i];
                if (ps == null)
                    continue;

                try
                {
                    if (pause)
                        ps.Pause(true);
                    else
                        ps.Play(true);
                }
                catch
                {
                }
            }

            prop.ParticlesPaused = pause;
        }

        private static bool IsProtectedName(string name)
        {
            string n = name.ToLowerInvariant();
            return n.Contains("rigmanager")
                   || n.Contains("[rigmanager]")
                   || n.Contains("playerrep")
                   || n.Contains("controller")
                   || n.Contains("hand")
                   || n.Contains("pelvis")
                   || n.Contains("avatar")
                   || n.Contains("spectator");
        }

        private static bool IsUnderPlayerHierarchy(Transform t)
        {
            int depth = 0;
            while (t != null && depth < 12)
            {
                string n = (t.name ?? string.Empty).ToLowerInvariant();
                if (n.Contains("rigmanager") || n.Contains("[rigmanager]") || n.Contains("playerrep"))
                    return true;
                t = t.parent;
                depth++;
            }

            return false;
        }

        public void RestoreAll()
        {
            foreach (TrackedProp prop in _byId.Values)
            {
                if (prop == null)
                    continue;
                ApplyCull(prop, false);
                ApplyShadows(prop, false);
                ApplyParticles(prop, false);
                ApplySleep(prop, false);
            }

            for (int i = 0; i < _props.Count; i++)
            {
                TrackedProp prop = _props[i];
                if (prop == null)
                    continue;
                ApplyCull(prop, false);
                ApplyShadows(prop, false);
                ApplyParticles(prop, false);
                ApplySleep(prop, false);
            }
        }
    }
}
