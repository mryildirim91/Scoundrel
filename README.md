Scoundrel
-----------------------------

1. Game Overview
Scoundrel is a tactical resource-management game played with a specialized 44-card
standard deck. The player acts as an adventurer navigating a dungeon deck one "room" at a
time. The goal is to reach the end of the deck with at least 1 HP remaining.
2. Deck Composition
The game uses a modified standard deck (44 cards total). All Red Face Cards (Jack, Queen,
King) and Red Aces are removed to maintain difficulty balance.
●
Monsters (Spades & Clubs): 26 Cards. Values: 2–10, J(11), Q(12), K(13), A(14).
●
Armor/Shields (Diamonds): 9 Cards. Values: 2–10.
●
Potions (Hearts): 9 Cards. Values: 2–10.
3. Core Mechanics
3.1 Static Shield (Diamonds)
Unlike traditional variants, shields in this version are permanent armor plates.
●
Equipping: Tapping a Diamond card sets the Player’s Shield value to that number.
●
Persistence: The Shield value never decreases when blocking damage.
●
Replacement: Picking up a new Diamond replaces the current one entirely.
3.2 Elemental Affinity (Combat)
●
Spades (Blades): Blocked at 100% efficiency.
●
Clubs (Bludgeons): Blocked at 50% efficiency (rounded down).
●
3.3 Healing (Hearts)
●
Max HP: 20.
●
Effect: Tapping a Heart card restores HP (Clamped at 20).
●
No Consumption Limit: Players can consume multiple Heart cards at any time.
3.4 The Fleeing Mechanics (Running)
There are two ways to exit/reset a room, each with distinct consequences for the deck and
the table.
A. Tactical Retreat (The Coward's Toll)
●
Trigger: Available when exactly 4 cards are present in the Room.
●
Cost: -1 HP instantly.
●
Action: Moves all 4 cards to the bottom of the deck.
●
Cooldown: Cannot be used twice in a row (the next room must be cleared or exited via
Safe Exit).
B. Safe Exit (The Scout's Departure)
●
Trigger: Available when exactly 1 card remains in the Room.
●
Cost: 0 HP (Free).
●
Strategic Action: The final card stays on the table.
●
Interaction: The game immediately deals 3 new cards from the deck to fill the room
back to a 4-card state.
●
Purpose: This allows players to "carry over" a beneficial card (like a Potion or Shield)
into the next encounter to balance a potentially dangerous draw.
C. Dead Zone
●
Restriction: The player cannot run if there are 2 or 3 cards remaining in the room.
They must interact with at least one more card to reach the "Safe Exit" state.
4. UI/UX & Interaction
4.1 Visual Layout
●
Theme: Classic "Card Table" look.
●
The Room (Center): 2x2 grid. If a "Safe Exit" was used, the carried-over card remains in
its original slot while the other three slots are refilled with new card animations.
●
Footer: * Displays "RUN (-1 HP)" when 4 cards are present.
○
Displays "RUN (FREE)" when 1 card is present.
○
Disabled when 2 or 3 cards are present.
5. Technical Game Loop
1. Initialize: Shuffle 44 cards. Set HP = 20, Shield = 0, RunAvailable = True.
2. The Deal: * If the room is empty: Pop 4 cards from DeckArray.
○
If a card was carried over (Safe Exit): Pop 3 cards from DeckArray.
3. Player Input: Wait for card tap or Run button.
4. Process Logic:
○
Monster/Shield/Potion: Standard resolution as defined in Section 3.
○
Run (4 cards): HP -= 1, Move 4 cards to bottom of DeckArray, set RunAvailable =
False.
○
Run (1 card): Keep current card in RoomList, trigger The Deal (3 cards).
5. Clean Up: If RoomList becomes empty through interaction (not Running), reset
RunAvailable = True.
6.End Conditions: Win if deck/room are empty; Loss if HP 0.

