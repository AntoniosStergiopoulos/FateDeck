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
the Odds Panel with exact fractions. Biome 1 is a 9-step track with weighted doors, 22 enemies,
3 elites (Toll Collector, Underwriter, Notary), THE COLLECTOR boss (capped Confiscate, the
Mantle, and spill-on-heavy-hit counterplay), chests, shrines, 18 events, rest & shops, 24
charms, 24 relics, 5 playable heroes with distinct decks and passives behind a hero-select
screen, between-room run saves, and the Honest-Ledger death screen.

The UI teaches itself: hover anything — force tiles, status chips, buttons, odds rows, enemy
intents, shop items — for a runtime tooltip explaining the rule behind it, with every law
written per-context in second person ("YOU suffer 2 Burn"), generated from the same data the
engine executes. Shop and reward rows carry CARD / RELIC / CHARM / SERVICE badges, the pre-flip
window pauses whenever you hold Pocket cards (POCKET IT / HONOR IT), every loss is attributed
by name in the ledger log, and the Dealer explains each mechanic the first time it fires.

The theme, stated plainly: you died owing Fate, and the House collateralized your soul into a
deck. Doom is **Debt**, the reshuffle tax is **Interest** (telegraphed on the draw pile before
it's due), wounds sit in **Escrow**, and surfaced Debt banks **Grit** — spend 3 between actions
on Scry 2, +2 Force, or a free mend. Fairness set: enemy-context laws cost ~60-70% of yours
(Iron is +2 for you, +1 against you), Guard strikes for 2 when you're outnumbered, a voided
action refunds your Main Action once per fight, multi-enemy rooms pay a squad purse, winning a
fight releases one page from escrow, heavy rooms are depth-gated out of early steps, and every
door advertises its stakes. See `Documentation/FateDeck-Design-Review.md` for the full
reasoning.

## Balance lab

`Tools → Fate Deck → Run Balance Simulation (Quick/Full)` plays a baseline bot (the
`AutoPlayer`) through hundreds of seeded runs per hero — headless, save-suppressed, a few
seconds — and writes `Documentation/BalanceReport.md` with win rates, death-by-step
histograms and killer attribution. The bot ignores charms and scry ordering on purpose:
its numbers are a floor, and tuning targets the floor band (roughly 10-25% per hero).
The current numbers are already lab-tuned: the original COLLECTOR killed 72-87% of every
run that reached it (bot floor 0-2%); softening the compound spiral (26 HP, opening
Confiscate 2, attacks 3/4, +1 Force per 4 held) moved the floor to 14-22%, and the same
data drove the victory-mend rule and the Debtor's Grit interest. Method and evidence live
in `Documentation/BalanceLab.md`.

Upgrading an existing project: run `Tools → Fate Deck → Rebuild Content From Scratch` once
(regenerates every asset, including the expanded hero decks), then `Create Game Scene` to
relink the table.

## Layout

- `Runtime/Core` — session, deck service, action pipeline, odds math
- `Runtime/Effects|Triggers|Conditions` — the GDD's Appendix-A atoms as OmniCard subclasses
- `Runtime/Combat` — combat engine + enemy pattern field kind
- `Runtime/Run` — doors, rooms, events, shops, saves
- `Runtime/Simulation` — the balance lab: AutoPlayer bot, event policy, run aggregator
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
