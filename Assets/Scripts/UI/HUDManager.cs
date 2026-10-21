using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Beeplex.Bee;
using Beeplex.Environment;

namespace Beeplex.UI
{
    /// <summary>
    /// Implements prd.md > Canvas HUD
    /// Implements prd.md > Feature 2: Nectar Harvesting & Capacity Management
    /// Implements prd.md > Feature 3: Hive Return & Resource Deposition
    /// Manages real-time clock, golden nectar slider, dream event banner, and hive counters.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("Canvas Hierarchy References")]
        [Tooltip("Top Center Clock: Clock_Text")]
        [SerializeField] private TextMeshProUGUI clockText;

        [Tooltip("Event Banner below Clock: DreamEvent_Banner")]
        [SerializeField] private GameObject dreamEventBanner;

        [Tooltip("Capacity Slider: Nectar_Slider")]
        [SerializeField] private Slider nectarSlider;

        [Tooltip("Slider Label: Nectar_Label (e.g. '15 / 20 Nectar')")]
        [SerializeField] private TextMeshProUGUI nectarLabel;

        [Tooltip("Standard Honey text in HiveScore_Panel")]
        [SerializeField] private TextMeshProUGUI standardHoneyText;

        [Tooltip("Dream Drops text in HiveScore_Panel")]
        [SerializeField] private TextMeshProUGUI dreamNectarText;

        [Header("Styling")]
        [SerializeField] private Color goldenFillColor = new Color(0.96f, 0.62f, 0.07f, 1f); // #f59e0b

        private void Awake()
        {
            if (dreamEventBanner != null)
            {
                dreamEventBanner.SetActive(false); // Default disabled per PRD
            }

            if (nectarSlider != null)
            {
                nectarSlider.minValue = 0f;
                nectarSlider.maxValue = 20f;
                nectarSlider.value = 0f;

                Image fillImage = nectarSlider.fillRect != null ? nectarSlider.fillRect.GetComponent<Image>() : null;
                if (fillImage != null)
                {
                    fillImage.color = goldenFillColor;
                }
            }

            UpdateNectarDisplay(0, 20);
            UpdateHiveScore(0, 0);
        }

        private void OnEnable()
        {
            BeeAgent.OnNectarChanged += UpdateNectarDisplay;
            HiveBase.OnHiveInventoryChanged += UpdateHiveScore;
            Core.TimeEventManager.OnDreamBloomStart += HandleDreamBloomStart;
            Core.TimeEventManager.OnDreamBloomEnd += HandleDreamBloomEnd;
        }

        private void OnDisable()
        {
            BeeAgent.OnNectarChanged -= UpdateNectarDisplay;
            HiveBase.OnHiveInventoryChanged -= UpdateHiveScore;
            Core.TimeEventManager.OnDreamBloomStart -= HandleDreamBloomStart;
            Core.TimeEventManager.OnDreamBloomEnd -= HandleDreamBloomEnd;
        }

        private void HandleDreamBloomStart()
        {
            SetDreamBannerActive(true);
        }

        private void HandleDreamBloomEnd()
        {
            SetDreamBannerActive(false);
        }

        private void Update()
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
            if (clockText != null)
            {
                clockText.text = DateTime.Now.ToString("hh:mm tt");
            }
        }

        public void UpdateNectarDisplay(int current, int max)
        {
            if (nectarSlider != null)
            {
                nectarSlider.maxValue = max;
                nectarSlider.value = current;
            }

            if (nectarLabel != null)
            {
                nectarLabel.text = $"{current} / {max} Nectar";
            }
        }

        public void UpdateHiveScore(int standardHoney, int dreamDrops)
        {
            if (standardHoneyText != null)
            {
                standardHoneyText.text = $"Honey: {standardHoney}";
            }

            if (dreamNectarText != null)
            {
                dreamNectarText.text = $"Dream Drops: {dreamDrops}";
            }
        }

        public void SetDreamBannerActive(bool active)
        {
            if (dreamEventBanner != null)
            {
                dreamEventBanner.SetActive(active);
            }
        }
    }
}
