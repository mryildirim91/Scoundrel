namespace Scoundrel.Core.Enums
{
    /// <summary>
    /// Types of run actions available to the player based on room state.
    /// </summary>
    public enum RunType
    {
        /// <summary>
        /// Cannot run (2-3 cards in room, or other restrictions).
        /// </summary>
        None,

        /// <summary>
        /// Tactical Retreat: exactly 4 cards, costs 1 HP, returns cards to deck bottom, has cooldown.
        /// </summary>
        TacticalRetreat,

        /// <summary>
        /// Safe Exit: exactly 1 card, free, card stays on table, 3 new cards dealt.
        /// </summary>
        SafeExit
    }
}
