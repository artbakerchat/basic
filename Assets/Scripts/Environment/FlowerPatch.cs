using UnityEngine;
using System;
#if UNITY_2D_LIGHTS
using UnityEngine.Rendering.Universal;
#endif

namespace Beeplex.Environment
{
    /// <summary>
    /// Implements prd.md > Feature 2: Nectar Harvesting & Capacity Management
    /// Implements prd.md > Feature 4: Real-Time 3:00 AM Dream Bloom Event
    /// </summary>
    public class FlowerPatch : MonoBehaviour
    {
        [Header("Flower Configuration")]
        [SerializeField] private bool isDreamBloomNode = false;
        [SerializeField] private int nectarYield = 5;
        [SerializeField] private float cooldownTime = 6f;

        [Header("State")]
        [SerializeField] private bool isDepleted = false;
        [SerializeField] private bool isDreamBloomActive = false;

        public bool IsDreamBloomNode => isDreamBloomNode;
        public bool IsActiveDreamBloom => isDreamBloomNode && isDreamBloomActive;
        public bool IsDepleted => isDepleted;
        public int NectarYield => nectarYield;

        [Header("VFX & Lighting")]
        [SerializeField] private ParticleSystem sporeParticles;
        [SerializeField] private Color standardColor = new Color(0.98f, 0.75f, 0.14f); // Golden Amber
        [SerializeField] private Color dreamColor = new Color(0.22f, 0.74f, 0.97f);    // Luminous Cyan
        [SerializeField] private SpriteRenderer flowerRenderer;

        public static event Action<FlowerPatch> OnFlowerHarvested;

        private void Awake()
        {
            if (flowerRenderer == null)
            {
                flowerRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        public void Harvest()
        {
            if (isDepleted) return;

            isDepleted = true;
            OnFlowerHarvested?.Invoke(this);

            if (flowerRenderer != null)
            {
                // Dim slightly while recovering
                Color c = flowerRenderer.color;
                c.a = 0.5f;
                flowerRenderer.color = c;
            }

            Invoke(nameof(ResetFlower), cooldownTime);
        }

        private void ResetFlower()
        {
            isDepleted = false;
            if (flowerRenderer != null)
            {
                Color c = flowerRenderer.color;
                c.a = 1f;
                flowerRenderer.color = c;
            }
        }

        public void SetDreamActive(bool active)
        {
            if (!isDreamBloomNode) return;

            isDreamBloomActive = active;

            if (sporeParticles != null)
            {
                if (active) sporeParticles.Play();
                else sporeParticles.Stop();
            }

            if (flowerRenderer != null)
            {
                flowerRenderer.color = active ? dreamColor : standardColor;
            }
        }
    }
}
