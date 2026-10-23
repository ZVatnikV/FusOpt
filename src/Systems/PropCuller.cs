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
            _props.Clear();

            Rigidbody[] bodies;
            try
            {
                bodies = UnityEngine.Object.FindObjectsOfType<Rigidbody>();
            }
            catch
            {
                return;
            }

            for (int i = 0; i < bodies.Length; i++)
            {
                var rb = bodies[i];
                if (rb == null)
                    continue;

                var go = rb.gameObject;
                if (go == null || !go.activeInHierarchy)
                    continue;

                // Skip player rigs / hands / heads — keep interaction intact.
                string name = go.name ?? string.Empty;
                if (IsProtectedName(name) || IsUnderPlayerHierarchy(go.transform))
                    continue;

                var renderers = go.GetComponentsInChildren<Renderer>(true);
                if (renderers == null || renderers.Length == 0)
                    continue;

                // Prefer dynamic clutter (spawnable-ish) over huge static scenery.
                if (rb.isKinematic && rb.mass > 80f)
                    continue;

                var tracked = new TrackedProp
                {
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
                    var ren = renderers[r];
                    if (ren == null)
                        continue;
                    tracked.OriginalShadows[r] = ren.shadowCastingMode;
                    tracked.OriginalRendererEnabled[r] = ren.enabled;
                }

                _props.Add(tracked);
            }

            _trackedCount = _props.Count;
        }

        private void Evaluate()
        {
            float near = Mathf.Max(4f, Config.NearDistance.Value);
            float far = Mathf.Max(near + 2f, Config.FarDistance.Value);
            int budget = Mathf.Max(10, Config.MaxActiveProps.Value);

            // Sort by distance so nearby props keep budget first.
            _props.Sort((a, b) =>
            {
                float da = DistanceSq(a);
                float db = DistanceSq(b);
                return da.CompareTo(db);
            });

            int kept = 0;
            int culled = 0;

            for (int i = 0; i < _props.Count; i++)
            {
                var prop = _props[i];
                if (prop.Body == null)
                    continue;

                float dist = Mathf.Sqrt(DistanceSq(prop));
                bool overBudget = kept >= budget;
                bool farAway = dist > far;
                bool mid = dist > near;

                bool shouldCull = Config.PropCull.Value && (farAway || overBudget);
                bool shouldSleep = Config.RigidbodySleep.Value && (farAway || overBudget || dist > near + ((far - near) * 0.5f));
                bool shouldShadowOff = Config.ShadowCull.Value && (mid || overBudget);
                bool shouldPauseParticles = Config.ParticleCull.Value && (mid || overBudget);

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

        private static void ApplyCull(TrackedProp prop, bool cull)
        {
            if (prop.Culled == cull)
                return;

            for (int i = 0; i < prop.Renderers.Length; i++)
            {
                var ren = prop.Renderers[i];
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
            var rb = prop.Body;
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
                    {
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        rb.Sleep();
                    }
                }
                else if (prop.StoredKinematic)
                {
                    // Wake only if something interacts / we restored visibility.
                    if (!prop.WasKinematic)
                        rb.WakeUp();
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
                var ren = prop.Renderers[i];
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
                var ps = prop.Particles[i];
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
                   || n.Contains("player")
                   || n.Contains("controller")
                   || n.Contains("hand")
                   || n.Contains("pelvis")
                   || n.Contains("head")
                   || n.Contains("avatar")
                   || n.Contains("spectator")
                   || n.Contains("ui");
        }

        private static bool IsUnderPlayerHierarchy(Transform t)
        {
            int depth = 0;
            while (t != null && depth < 8)
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
            for (int i = 0; i < _props.Count; i++)
            {
                var prop = _props[i];
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
