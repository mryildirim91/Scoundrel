namespace Scoundrel.Core.Enums
{
    /// <summary>
    /// Represents the outcome of a completed game.
    /// </summary>
    public enum GameResult
    {
        None,    // Game still in progress
        Victory, // Player cleared the deck with HP > 0
        Defeat   // Player's HP reached 0
    }
}
