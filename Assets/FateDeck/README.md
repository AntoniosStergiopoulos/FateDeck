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

The complete fate engine — 22 forces with data-driven per-context laws (the original 7 families
plus Tempest, Serpent, Glass, Gloom, Key, Mirror, Anchor, Rust and Wisp), flip/mill/wound/heal,
the Pocket (bank & replace), the reshuffle Doom tax, doom laundering, Echo/Void, Burn/Weak, and
the Odds Panel with exact fractions. Biome 1 is a 9-step track with weighted doors, 16 enemies,
3 elites (Toll Collector, Underwriter, Notary), THE COLLECTOR boss (capped Confiscate, the
Mantle, and spill-on-heavy-hit counterplay), chests, shrines, 14 events, rest & shops, 18
charms, 18 relics, 5 playable heroes with distinct decks and passives (Gambler, Stoker,
Actuary, Debtor, Sexton) behind a hero-select screen, between-room run saves, and the
Honest-Ledger death screen.

Upgrading an existing project: run `Tools → Fate Deck → Rebuild Content From Scratch` once
(regenerates every asset, including the expanded hero decks), then `Create Game Scene` to
relink the table.

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
