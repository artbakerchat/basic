using UnityEngine;
using System;
using System.Collections;

namespace Beeplex.Bee
{
    public enum BeeState
    {
        Idle,
        FlyingToBeacon,
        Harvesting,
        ReturningToHive
    }

    /// <summary>
    /// Implements prd.md > Feature 1: Gaze-Directed Scent Beacon & Pathfinding
    /// Implements prd.md > Feature 2: Nectar Harvesting & Capacity Management
    /// Implements prd.md > Feature 3: Hive Return & Resource Deposition
    /// </summary>
    public class BeeAgent : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private BeeState currentState = BeeState.Idle;
        public BeeState CurrentState => currentState;

        [Header("Flight & Steering Parameters")]
        [SerializeField] private float flightSpeed = 4.5f;
        [SerializeField] private float arrivalRadius = 0.25f;
        [SerializeField] private float hoverFrequency = 6.0f;
        [SerializeField] private float hoverAmplitude = 0.15f;
        [SerializeField] private float tiltMultiplier = 15f;

        [Header("Inventory & Capacity")]
        [SerializeField] private int currentNectar = 0;
        [SerializeField] private int maxNectar = 20;
        [SerializeField] private int standardNectarHeld = 0;
        [SerializeField] private int dreamDropsHeld = 0;

        public int CurrentNectar => currentNectar;
        public int MaxNectar => maxNectar;
        public int StandardNectarHeld => standardNectarHeld;
        public int DreamDropsHeld => dreamDropsHeld;
        public bool IsFull => currentNectar >= maxNectar;

        [Header("References")]
        [SerializeField] private Transform hiveTransform;

        // Navigation state
        private Vector2 currentTargetPosition;
        private TargetType currentTargetType;
        private GameObject currentTargetObject;

        // Queued retargeting (for mid-harvest shifts)
        private bool hasQueuedTarget = false;
        private Vector2 queuedTargetPosition;
        private TargetType queuedTargetType;
        private GameObject queuedTargetObject;

        // Active harvest coroutine
        private Coroutine activeHarvestCoroutine;

        // Events
        public static event Action<int, int> OnNectarChanged; // current, max
        public static event Action<BeeState> OnStateChanged;

        private void OnEnable()
        {
            ScentBeaconController.OnBeaconPlaced += HandleBeaconPlaced;
        }

        private void OnDisable()
        {
            ScentBeaconController.OnBeaconPlaced -= HandleBeaconPlaced;
        }

        private void Start()
        {
            if (hiveTransform == null)
            {
                GameObject hiveObj = GameObject.FindWithTag("Hive");
                if (hiveObj != null) hiveTransform = hiveObj.transform;
            }

            SetState(BeeState.Idle);
            OnNectarChanged?.Invoke(currentNectar, maxNectar);
        }

        private void Update()
        {
            switch (currentState)
            {
                case BeeState.Idle:
                    UpdateIdleHover();
                    break;
                case BeeState.FlyingToBeacon:
                    UpdateFlight(currentTargetPosition);
                    break;
                case BeeState.ReturningToHive:
                    if (hiveTransform != null)
                    {
                        UpdateFlight(hiveTransform.position);
                    }
                    break;
                case BeeState.Harvesting:
                    // Handled via harvest coroutine; bee locks on and gently bobs
                    UpdateIdleHover();
                    break;
            }
        }

        /// <summary>
        /// Handles scent beacon events dispatched by the player's gaze.
        /// </summary>
        private void HandleBeaconPlaced(Vector2 targetPos, TargetType targetType, GameObject targetObj)
        {
            // If already at full capacity, bee prioritizes hive return over flower beacons
            if (IsFull && targetType == TargetType.Flower)
            {
                return;
            }

            // Hive recall can be triggered manually at any time
            if (targetType == TargetType.Hive)
            {
                TriggerReturnToHive();
                return;
            }

            if (currentState == BeeState.Harvesting)
            {
                // PRD Retargeting Rule: Finish current siphon first, then fly to newly queued target
                hasQueuedTarget = true;
                queuedTargetPosition = targetPos;
                queuedTargetType = targetType;
                queuedTargetObject = targetObj;
                return;
            }

            // Mid-flight redirection
            currentTargetPosition = targetPos;
            currentTargetType = targetType;
            currentTargetObject = targetObj;
            SetState(BeeState.FlyingToBeacon);
        }

        private void UpdateFlight(Vector2 target)
        {
            Vector2 currentPos = transform.position;
            Vector2 toTarget = target - currentPos;
            float distance = toTarget.magnitude;

            if (distance <= arrivalRadius)
            {
                OnArrivedAtDestination();
                return;
            }

            Vector2 direction = toTarget.normalized;

            // Calculate organic perpendicular sine wave bobbing
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            float oscillation = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            Vector2 wobbleOffset = perpendicular * oscillation;

            // Move bee smoothly
            Vector2 newPos = currentPos + (direction * (flightSpeed * Time.deltaTime)) + (wobbleOffset * Time.deltaTime);
            transform.position = newPos;

            // Subtle rotation / tilt facing direction
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, Time.deltaTime * tiltMultiplier));
        }

        private void UpdateIdleHover()
        {
            // Gentle hovering float on Y axis
            float hoverY = Mathf.Sin(Time.time * hoverFrequency) * (hoverAmplitude * 0.5f);
            transform.position += new Vector3(0, hoverY * Time.deltaTime, 0);
        }

        private void OnArrivedAtDestination()
        {
            if (currentState == BeeState.FlyingToBeacon)
            {
                if (currentTargetType == TargetType.Flower)
                {
                    StartHarvesting(currentTargetObject);
                }
                else if (currentTargetType == TargetType.Hive)
                {
                    DepositAtHive();
                }
            }
            else if (currentState == BeeState.ReturningToHive)
            {
                DepositAtHive();
            }
        }

        public void StartHarvesting(GameObject flowerObj)
        {
            if (activeHarvestCoroutine != null)
            {
                StopCoroutine(activeHarvestCoroutine);
            }
            activeHarvestCoroutine = StartCoroutine(HarvestRoutine(flowerObj));
        }

        private IEnumerator HarvestRoutine(GameObject flowerObj)
        {
            SetState(BeeState.Harvesting);

            // Siphon duration: 2.5 seconds (PRD: 2 to 3 seconds)
            yield return new WaitForSeconds(2.5f);

            bool isDream = false;
            // Check if flower patch has dream bloom status
            var flower = flowerObj != null ? flowerObj.GetComponent<Environment.FlowerPatch>() : null;
            if (flower != null)
            {
                isDream = flower.IsActiveDreamBloom;
                flower.Harvest();
            }

            AddNectar(5, isDream);
            activeHarvestCoroutine = null;

            // Check if at capacity
            if (IsFull)
            {
                TriggerReturnToHive();
            }
            else if (hasQueuedTarget)
            {
                // Process target that was queued mid-harvest
                hasQueuedTarget = false;
                currentTargetPosition = queuedTargetPosition;
                currentTargetType = queuedTargetType;
                currentTargetObject = queuedTargetObject;
                SetState(BeeState.FlyingToBeacon);
            }
            else
            {
                SetState(BeeState.Idle);
            }
        }

        public void AddNectar(int amount, bool isDream = false)
        {
            int spaceRemaining = maxNectar - currentNectar;
            int toAdd = Mathf.Min(amount, spaceRemaining);

            if (toAdd > 0)
            {
                currentNectar += toAdd;
                if (isDream)
                {
                    dreamDropsHeld += toAdd;
                }
                else
                {
                    standardNectarHeld += toAdd;
                }
                OnNectarChanged?.Invoke(currentNectar, maxNectar);
            }
        }

        public void TriggerReturnToHive()
        {
            hasQueuedTarget = false;
            if (hiveTransform != null)
            {
                currentTargetPosition = hiveTransform.position;
            }
            SetState(BeeState.ReturningToHive);
        }

        public void DepositAtHive()
        {
            if (hiveTransform != null)
            {
                var hive = hiveTransform.GetComponent<Environment.HiveBase>();
                if (hive != null)
                {
                    hive.DepositNectar(standardNectarHeld, dreamDropsHeld);
                }
            }

            // Reset inventory
            currentNectar = 0;
            standardNectarHeld = 0;
            dreamDropsHeld = 0;
            OnNectarChanged?.Invoke(currentNectar, maxNectar);

            SetState(BeeState.Idle);
        }

        private void SetState(BeeState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
