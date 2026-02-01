namespace Scoundrel.Core.Enums
{
    /// <summary>
    /// Categorizes cards by their gameplay function.
    /// Derived from suit: Spades/Clubs = Monster, Diamonds = Shield, Hearts = Potion.
    /// </summary>
    public enum CardType
    {
        Monster, // Deals damage to player (Spades & Clubs)
        Shield,  // Provides static armor (Diamonds)
        Potion   // Heals player with overdose restriction (Hearts)
    }
}
