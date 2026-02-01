namespace Scoundrel.Core.Interfaces
{
    /// <summary>
    /// Interface for game configuration settings.
    /// Implemented by GameSettings ScriptableObject for easy balancing.
    /// </summary>
    public interface IGameSettings
    {
        /// <summary>
        /// Starting HP for a new game (default: 20).
        /// </summary>
        int StartingHP { get; }

        /// <summary>
        /// Maximum HP cap (default: 20).
        /// </summary>
        int MaxHP { get; }

        /// <summary>
        /// Starting shield value (default: 0).
        /// </summary>
        int StartingShield { get; }

        /// <summary>
        /// HP cost to run from a room (default: 1).
        /// </summary>
        int RunCost { get; }

        /// <summary>
        /// Number of cards dealt per room (default: 4).
        /// </summary>
        int RoomSize { get; }

        /// <summary>
        /// Shield efficiency against Clubs (default: 0.5 = 50%).
        /// </summary>
        float ClubsShieldEfficiency { get; }

        /// <summary>
        /// Minimum cards required in room to run (default: 3).
        /// </summary>
        int MinCardsToRun { get; }
    }
}
