using UnityEngine;
using System;

namespace Beeplex.Bee
{
    public enum TargetType
    {
        Flower,
        Hive
    }

    /// <summary>
    /// Implements prd.md > Feature 1: Gaze-Directed Scent Beacon & Pathfinding
    /// Translates mouse gaze into world-space scent beacons to guide autonomous bees.
    /// </summary>
    public class ScentBeaconController : MonoBehaviour
    {
        [Header("Layer Masks")]
        [SerializeField] private LayerMask interactiveLayers;
        [SerializeField] private string flowerTag = "Flower";
        [SerializeField] private string hiveTag = "Hive";

        [Header("Beacon Settings")]
        [SerializeField] private bool showDebugVisuals = true;
        [SerializeField] private Color beaconColor = new Color(0.96f, 0.62f, 0.07f, 0.5f); // Amber

        public static event Action<Vector2, TargetType, GameObject> OnBeaconPlaced;

        private Camera mainCamera;
        private Vector2 lastValidBeaconPosition;
        private GameObject currentTargetObject;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (interactiveLayers == 0)
            {
                // Default to all layers if not configured in inspector
                interactiveLayers = ~0;
            }
        }

        private void Update()
        {
            HandleGazeRaycast();
        }

        /// <summary>
        /// Casts an invisible scent raycast from the mouse cursor into the 2D world.
        /// </summary>
        public void HandleGazeRaycast()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            Vector3 mouseScreenPos = Input.mousePosition;
            Vector3 worldPos3D = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            Vector2 worldPos2D = new Vector2(worldPos3D.x, worldPos3D.y);

            // Raycast against 2D colliders
            RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero, 0f, interactiveLayers);

            if (hit.collider != null)
            {
                GameObject hitObj = hit.collider.gameObject;

                if (hitObj.CompareTag(flowerTag))
                {
                    PlaceBeacon(worldPos2D, TargetType.Flower, hitObj);
                }
                else if (hitObj.CompareTag(hiveTag))
                {
                    PlaceBeacon(worldPos2D, TargetType.Hive, hitObj);
                }
            }
            // Empty ground gaze intentionally does not overwrite active beacon (PRD: Gaze on Empty Ground)
        }

        private void PlaceBeacon(Vector2 position, TargetType targetType, GameObject targetObj)
        {
            if (currentTargetObject == targetObj && Vector2.Distance(lastValidBeaconPosition, position) < 0.1f)
            {
                return; // Avoid spamming identical events
            }

            lastValidBeaconPosition = position;
            currentTargetObject = targetObj;
            OnBeaconPlaced?.Invoke(position, targetType, targetObj);
        }

        private void OnDrawGizmos()
        {
            if (!showDebugVisuals || !Application.isPlaying) return;

            if (currentTargetObject != null)
            {
                Gizmos.color = beaconColor;
                Gizmos.DrawWireSphere(lastValidBeaconPosition, 0.5f);
            }
        }
    }
}
