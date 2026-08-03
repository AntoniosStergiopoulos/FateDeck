# FATE DECK

A single-player roguelike where your deck is your health bar, your luck, and your build —
built on the **OmniCard** package (`com.astergio.omnicard`). All randomness in the game is a
flip from one visible, countable, sculptable deck.

## Quick start

1. Open the project in Unity 6000.3 (the manifest references omni-card as a local sibling:
   `"com.astergio.omnicard": "file:../../omni-card"`).
2. `Tools → Fate Deck → Create Game Scene` (generates all content assets on first run).
3. Press Play.

## What's in the slice

The complete fate engine — all 7 forces (+4 upgraded tiers) with data-driven per-context laws,
flip/mill/wound/heal, the Pocket (bank & replace), the reshuffle Doom tax, doom laundering,
Echo/Void, Burn/Weak, the Odds Panel with exact fractions — plus a playable Biome 1: 9-step
track with weighted doors, 5 enemies, the Toll Collector elite, THE COLLECTOR boss (Confiscate
and the Mantle), chests, shrines, 3 events, rest & shops, 6 charms, 6 relics, The Gambler,
between-room run saves, and the Honest-Ledger death screen.

## Layout

- `Runtime/Core` — session, deck service, action pipeline, odds math
- `Runtime/Effects|Triggers|Conditions` — the GDD's Appendix-A atoms as OmniCard subclasses
- `Runtime/Combat` — combat engine + enemy pattern field kind
- `Runtime/Run` — doors, rooms, events, shops, saves
- `Runtime/Views` — the one-screen table, built entirely with runtime UI Toolkit: crisp
  screen-space text, a scrollable color-coded event log (right column) that narrates every
  flip/mill/purchase, force-colored composition + odds panels (left), the deck tableau
  (bottom), and modal overlays for pile inspection, zone picks and scrying. Card faces are
  rendered through omni-card's `UIToolkitCardViewBuilder`; the generator creates the
  `PanelSettings` + default runtime theme automatically
- `Editor` — idempotent content generator + scene builder
- `Tests/Editor` — headless engine tests (in-memory content, no assets)
- `Documentation/FateDeck-OmniCard-Integration.md` — full analysis: what maps where,
  design decisions, and gaps found in omni-card while building this

The game design document lives with the project owner (`fatedeckgdd.md`).
