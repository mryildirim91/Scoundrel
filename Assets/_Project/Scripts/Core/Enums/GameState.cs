namespace Scoundrel.Core.Enums
{
    /// <summary>
    /// Represents the current state of the game flow.
    /// Used by GameManager's state machine to control game logic.
    /// </summary>
    public enum GameState
    {
        Initializing, // Setting up deck, player stats
        Dealing,      // Dealing cards to room (animation state)
        PlayerTurn,   // Waiting for player input
        Processing,   // Processing player action
        GameOver      // Game ended (win or loss)
    }
}
