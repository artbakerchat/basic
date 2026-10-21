using UnityEngine;

namespace Beeplex.Core
{
    /// <summary>
    /// Coordinates high-level managers, initializes scene defaults, and handles session lifecycle.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Target Frame Rate")]
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Application.targetFrameRate = targetFrameRate;
        }
    }
}
