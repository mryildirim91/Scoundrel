namespace Scoundrel.Core.Enums
{
    /// <summary>
    /// Represents the four suits in a standard deck of cards.
    /// Spades and Clubs are monsters, Diamonds are shields, Hearts are potions.
    /// </summary>
    public enum CardSuit
    {
        Spades,   // Monster - 100% shield block efficiency (Blades)
        Clubs,    // Monster - 50% shield block efficiency (Bludgeons)
        Diamonds, // Shield - Static armor
        Hearts    // Potion - Healing with overdose mechanic
    }
}
