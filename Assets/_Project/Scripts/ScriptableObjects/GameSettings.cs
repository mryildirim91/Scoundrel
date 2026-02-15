using Scoundrel.Core.Interfaces;
using UnityEngine;

namespace Scoundrel.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject containing all game balance settings.
    /// Registered as a service via Init(args) for dependency injection.
    /// Create instance via: Assets > Create > Scoundrel > Game Settings
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Scoundrel/Game Settings")]
    public sealed class GameSettings : ScriptableObject, IGameSettings
    {
        [Header("Player Stats")]
        [SerializeField, Tooltip("Starting HP for a new game")]
        private int startingHP = 20;

        [SerializeField, Tooltip("Maximum HP cap")]
        private int maxHP = 20;

        [SerializeField, Tooltip("Starting shield value")]
        private int startingShield = 0;

        [Header("Run Mechanic")]
        [SerializeField, Tooltip("HP cost to run from a room")]
        private int runCost = 1;

        [SerializeField, Tooltip("Number of cards to deal from deck on Safe Exit (fills room back to RoomSize)")]
        private int safeExitFillCount = 3;

        [Header("Room Settings")]
        [SerializeField, Tooltip("Number of cards dealt per room")]
        private int roomSize = 4;

        [Header("Combat Balance")]
        [SerializeField, Range(0f, 1f), Tooltip("Shield efficiency against Clubs (0.5 = 50%)")]
        private float clubsShieldEfficiency = 0.5f;

        // IGameSettings implementation
        public int StartingHP => startingHP;
        public int MaxHP => maxHP;
        public int StartingShield => startingShield;
        public int RunCost => runCost;
        public int RoomSize => roomSize;
        public float ClubsShieldEfficiency => clubsShieldEfficiency;
        public int SafeExitFillCount => safeExitFillCount;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure valid values
            startingHP = Mathf.Max(1, startingHP);
            maxHP = Mathf.Max(startingHP, maxHP);
            startingShield = Mathf.Max(0, startingShield);
            runCost = Mathf.Max(0, runCost);
            roomSize = Mathf.Clamp(roomSize, 1, 10);
            safeExitFillCount = Mathf.Clamp(safeExitFillCount, 1, roomSize - 1);
        }
#endif
    }
}
