namespace Scoundrel.Core.Interfaces
{
    /// <summary>
    /// Strategy interface for calculating combat damage.
    /// Different implementations handle different suit affinities:
    /// - Spades: 100% shield efficiency
    /// - Clubs: 50% shield efficiency
    /// </summary>
    public interface IDamageCalculator
    {
        /// <summary>
        /// Calculates the damage dealt to the player.
        /// </summary>
        /// <param name="monsterValue">The monster card's value (2-14).</param>
        /// <param name="shieldValue">The player's current shield value.</param>
        /// <returns>Damage to deal (always >= 0).</returns>
        int Calculate(int monsterValue, int shieldValue);
    }
}
