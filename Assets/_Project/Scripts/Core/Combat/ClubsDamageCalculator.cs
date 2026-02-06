using Scoundrel.Core.Interfaces;
using UnityEngine;

namespace Scoundrel.Core.Combat
{
    /// <summary>
    /// Damage calculator for Club monsters (Bludgeons).
    /// Clubs are blocked at 50% shield efficiency (heavy impact).
    /// Formula: Damage = Max(0, MonsterValue - Floor(ShieldValue / 2))
    /// </summary>
    public sealed class ClubsDamageCalculator : IDamageCalculator
    {
        private readonly float _shieldEfficiency;

        /// <summary>
        /// Creates a new ClubsDamageCalculator with default 50% efficiency.
        /// </summary>
        public ClubsDamageCalculator() : this(0.5f)
        {
        }

        /// <summary>
        /// Creates a new ClubsDamageCalculator with custom efficiency.
        /// </summary>
        /// <param name="shieldEfficiency">Shield efficiency (0.0 to 1.0, default 0.5).</param>
        public ClubsDamageCalculator(float shieldEfficiency)
        {
            _shieldEfficiency = Mathf.Clamp01(shieldEfficiency);
        }

        /// <summary>
        /// Calculates damage from a Club monster.
        /// Shield blocks at reduced (50%) efficiency.
        /// </summary>
        /// <param name="monsterValue">The monster card's value (2-14).</param>
        /// <param name="shieldValue">The player's current shield value.</param>
        /// <returns>Damage to deal (always >= 0).</returns>
        public int Calculate(int monsterValue, int shieldValue)
        {
            // Effective shield = Floor(ShieldValue * Efficiency)
            int effectiveShield = Mathf.FloorToInt(shieldValue * _shieldEfficiency);
            int damage = Mathf.Max(0, monsterValue - effectiveShield);

            Debug.Log($"[ClubsDamageCalculator] Monster: {monsterValue}, Shield: {shieldValue} ({_shieldEfficiency * 100}% = {effectiveShield}) = {damage} damage");

            return damage;
        }
    }
}
