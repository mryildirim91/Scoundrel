namespace Scoundrel.Core.Interfaces
{
    /// <summary>
    /// Interface for managing player state (HP, Shield, game flags).
    /// Implementations should fire events via IGameEvents when state changes.
    /// </summary>
    public interface IPlayerState
    {
        /// <summary>
        /// Current health points (0 to MaxHP).
        /// </summary>
        int CurrentHP { get; }

        /// <summary>
        /// Maximum health points (typically 20).
        /// </summary>
        int MaxHP { get; }

        /// <summary>
        /// Current shield value (static armor from Diamonds).
        /// </summary>
        int ShieldValue { get; }

        /// <summary>
        /// Whether hearts are locked due to overdose rule.
        /// True after drinking a potion, false after interacting with non-heart.
        /// </summary>
        bool IsHeartLocked { get; }

        /// <summary>
        /// Whether the player can currently use the Run action.
        /// False after running, true after clearing or interacting with next room.
        /// </summary>
        bool CanRun { get; }

        /// <summary>
        /// Whether the player is still alive (HP > 0).
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Reduces HP by the specified amount. Clamps to 0.
        /// </summary>
        void TakeDamage(int amount);

        /// <summary>
        /// Increases HP by the specified amount. Clamps to MaxHP.
        /// </summary>
        void Heal(int amount);

        /// <summary>
        /// Sets the shield value (from Diamond cards).
        /// </summary>
        void SetShield(int value);

        /// <summary>
        /// Sets the heart lock state (overdose mechanic).
        /// </summary>
        void SetHeartLock(bool locked);

        /// <summary>
        /// Sets whether the player can run.
        /// </summary>
        void SetCanRun(bool canRun);

        /// <summary>
        /// Pays the run cost (typically -1 HP).
        /// </summary>
        void PayRunCost();

        /// <summary>
        /// Resets player state to initial values for a new game.
        /// </summary>
        void Reset();
    }
}
