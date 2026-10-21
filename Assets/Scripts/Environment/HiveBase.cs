using UnityEngine;
using System;

namespace Beeplex.Environment
{
    /// <summary>
    /// Implements prd.md > Feature 3: Hive Return & Resource Deposition
    /// Stores the colony's harvested Standard Honey and rare Dream Drops.
    /// </summary>
    public class HiveBase : MonoBehaviour
    {
        [Header("Colony Stores")]
        [SerializeField] private int standardHoney = 0;
        [SerializeField] private int dreamDrops = 0;

        public int StandardHoney => standardHoney;
        public int DreamDrops => dreamDrops;

        public static event Action<int, int> OnHiveInventoryChanged; // standardHoney, dreamDrops

        private void Start()
        {
            // Initial score broadcast
            OnHiveInventoryChanged?.Invoke(standardHoney, dreamDrops);
        }

        public void DepositNectar(int standardAmount, int dreamAmount)
        {
            if (standardAmount <= 0 && dreamAmount <= 0) return;

            standardHoney += standardAmount;
            dreamDrops += dreamAmount;

            OnHiveInventoryChanged?.Invoke(standardHoney, dreamDrops);
        }

        public void ResetStores()
        {
            standardHoney = 0;
            dreamDrops = 0;
            OnHiveInventoryChanged?.Invoke(standardHoney, dreamDrops);
        }
    }
}
