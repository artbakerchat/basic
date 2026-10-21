using UnityEngine;
using System;
using System.Collections;
using Beeplex.Environment;
using Beeplex.UI;
#if UNITY_2D_LIGHTS
using UnityEngine.Rendering.Universal;
#endif

namespace Beeplex.Core
{
    /// <summary>
    /// Implements prd.md > Feature 4: Real-Time 3:00 AM Dream Bloom Event
    /// Implements spec.md > TimeEventManager
    /// Evaluates system clock, manages 45-second nocturnal event window, controls lighting and VFX.
    /// </summary>
    public class TimeEventManager : MonoBehaviour
    {
        public static TimeEventManager Instance { get; private set; }

        [Header("Event Configuration")]
        [Tooltip("Target hour for event trigger (0-23, default 3 for 3:00 AM)")]
        [SerializeField] private int targetHour = 3;

        [Tooltip("Duration of the active Dream Bloom event in real-world seconds (PRD: 45s)")]
        [SerializeField] private float eventDurationSeconds = 45f;

        [Header("Testing & Video Recording Override")]
        [Tooltip("Check this in the Inspector to immediately trigger the 45s Dream Bloom event for testing or demo capture")]
        [SerializeField] private bool triggerTestEvent = false;

        [Header("Lighting Settings")]
        [SerializeField] private Color dayAmbientColor = new Color(0.95f, 0.95f, 0.9f);
        [SerializeField] private Color midnightBlueColor = new Color(0.06f, 0.09f, 0.16f); // #0f172a
        [SerializeField] private float standardLightIntensity = 1.0f;
        [SerializeField] private float midnightLightIntensity = 0.25f;

        [Header("Scene References")]
        [SerializeField] private HUDManager hudManager;
#if UNITY_2D_LIGHTS
        [SerializeField] private Light2D globalLight2D;
#endif

        [Header("State")]
        [SerializeField] private bool isDreamBloomActive = false;
        [SerializeField] private float timeRemainingInEvent = 0f;

        public bool IsDreamBloomActive => isDreamBloomActive;
        public float TimeRemainingInEvent => timeRemainingInEvent;

        // Events
        public static event Action OnDreamBloomStart;
        public static event Action OnDreamBloomEnd;

        private Coroutine activeEventCoroutine;
        private bool hasTriggeredThisHour = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (hudManager == null)
            {
                hudManager = FindObjectOfType<HUDManager>();
            }
        }

        private void Start()
        {
            SetNormalLighting();
        }

        private void Update()
        {
            CheckClockTrigger();

            if (triggerTestEvent)
            {
                triggerTestEvent = false;
                StartDreamBloomEvent();
            }
        }

        /// <summary>
        /// Reads local system time and triggers event when entering the target 3:00 AM hour.
        /// </summary>
        private void CheckClockTrigger()
        {
            DateTime now = DateTime.Now;

            if (now.Hour == targetHour)
            {
                if (!hasTriggeredThisHour && !isDreamBloomActive)
                {
                    hasTriggeredThisHour = true;
                    StartDreamBloomEvent();
                }
            }
            else
            {
                // Reset trigger lock once outside the target hour
                hasTriggeredThisHour = false;
            }
        }

        /// <summary>
        /// Initiates the 45-second Dream Bloom nocturnal window.
        /// </summary>
        public void StartDreamBloomEvent()
        {
            if (isDreamBloomActive) return;

            if (activeEventCoroutine != null)
            {
                StopCoroutine(activeEventCoroutine);
            }
            activeEventCoroutine = StartCoroutine(DreamBloomRoutine());
        }

        private IEnumerator DreamBloomRoutine()
        {
            isDreamBloomActive = true;
            timeRemainingInEvent = eventDurationSeconds;

            // 1. Notify listeners (HUDManager, etc.)
            OnDreamBloomStart?.Invoke();
            if (hudManager != null)
            {
                hudManager.SetDreamBannerActive(true);
            }

            // 2. Dim world lighting to midnight blue
            ApplyMidnightLighting();

            // 3. Activate all Dream Bloom nodes
            FlowerPatch[] flowers = FindObjectsOfType<FlowerPatch>();
            foreach (var flower in flowers)
            {
                if (flower.IsDreamBloomNode)
                {
                    flower.SetDreamActive(true);
                }
            }

            // 4. Run 45-second real-world countdown
            while (timeRemainingInEvent > 0f)
            {
                timeRemainingInEvent -= Time.deltaTime;
                yield return null;
            }

            // 5. Conclude event
            timeRemainingInEvent = 0f;
            isDreamBloomActive = false;

            // Deactivate dream blooms
            foreach (var flower in flowers)
            {
                if (flower.IsDreamBloomNode)
                {
                    flower.SetDreamActive(false);
                }
            }

            // Disable banner
            if (hudManager != null)
            {
                hudManager.SetDreamBannerActive(false);
            }

            // Smoothly restore normal lighting
            StartCoroutine(RestoreLightingSmoothly(2.0f));

            OnDreamBloomEnd?.Invoke();
            activeEventCoroutine = null;
        }

        private void ApplyMidnightLighting()
        {
            RenderSettings.ambientLight = midnightBlueColor;
#if UNITY_2D_LIGHTS
            if (globalLight2D != null)
            {
                globalLight2D.color = midnightBlueColor;
                globalLight2D.intensity = midnightLightIntensity;
            }
#endif
        }

        private void SetNormalLighting()
        {
            RenderSettings.ambientLight = dayAmbientColor;
#if UNITY_2D_LIGHTS
            if (globalLight2D != null)
            {
                globalLight2D.color = dayAmbientColor;
                globalLight2D.intensity = standardLightIntensity;
            }
#endif
        }

        private IEnumerator RestoreLightingSmoothly(float duration)
        {
            float elapsed = 0f;
            Color startColor = RenderSettings.ambientLight;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                RenderSettings.ambientLight = Color.Lerp(startColor, dayAmbientColor, t);
                yield return null;
            }

            SetNormalLighting();
        }
    }
}
