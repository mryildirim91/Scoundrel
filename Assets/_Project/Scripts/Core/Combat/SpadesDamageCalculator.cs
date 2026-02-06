using Scoundrel.Core.Interfaces;
using UnityEngine;

namespace Scoundrel.Core.Combat
{
    /// <summary>
    /// Damage calculator for Spade monsters (Blades).
    /// Spades are blocked at 100% shield efficiency.
    /// Formula: Damage = Max(0, MonsterValue - ShieldValue)
    /// </summary>
    public sealed class SpadesDamageCalculator : IDamageCalculator
    {
        /// <summary>
        /// Calculates damage from a Spade monster.
        /// Shield blocks at full (100%) efficiency.
        /// </summary>
        /// <param name="monsterValue">The monster card's value (2-14).</param>
        /// <param name="shieldValue">The player's current shield value.</param>
        /// <returns>Damage to deal (always >= 0).</returns>
        public int Calculate(int monsterValue, int shieldValue)
        {
            int damage = Mathf.Max(0, monsterValue - shieldValue);

            Debug.Log($"[SpadesDamageCalculator] Monster: {monsterValue}, Shield: {shieldValue} (100%) = {damage} damage");

            return damage;
        }
    }
}
