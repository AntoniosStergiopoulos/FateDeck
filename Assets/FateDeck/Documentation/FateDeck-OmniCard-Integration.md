# Fate Deck on OmniCard — integration analysis & architecture

*How the Fate Deck GDD maps onto the `com.astergio.omnicard` package (v0.12.0), what the game
adds on top, and where the package could grow. Written against GDD v1.0 and the omni-card
source on the neighbouring branch.*

---

## 1. The verdict up front

OmniCard is a strong fit for Fate Deck — not because it ships a roguelike, but because the GDD's
own Appendix A ("definitions composed of polymorphic atoms, `[SerializeReference]`-style effect
lists on ScriptableObject definitions with stable string IDs") *is* OmniCard's architecture,
nearly clause for clause. The mapping is direct:

| GDD Appendix A concept | OmniCard feature used |
|---|---|
| `ForceDefinition { id, laws: ContextLaw[] }` | `MetadataEntry` on a "Force" `MetadataKind` carrying **five effect-list fields** — one per law context (Your Offense / Your Defense / Enemy Action / Loot / Ritual) |
| `ContextLaw { context, effects[] }` | `EffectListFieldValue` of `CardEffect` atoms per context field |
| `CardDefinition { id, forceId, flags }` | `CardDefinition` on the Fate Card schema: a `ReferenceFieldValue` to its force entry; Doom's `NO_POCKET / NO_STACK / EXILE_ON_MILL` flags are boolean fields **on the force entry** |
| `RelicDefinition { triggers: TriggeredAbility[] }` | `CardDefinition` on the Relic schema with a `TriggerListFieldValue`; activated by the package's own `CardTriggerBinding` |
| `TriggeredAbility { trigger, conditions, effects }` | `CardTrigger` subclasses (`Condition` + effect list are on the base class) |
| `EnemyDefinition { hp, bounty, pattern, passives }` | `CardDefinition` on the Enemy schema — *enemies are card instances*, the SpireClimb pattern; the pattern is a one-class custom field kind (`EnemyPatternFieldKind`) |
| `EventDefinition` (nodes flip fate via RITUAL) | `EventDefinition : CardAsset` with options whose `RitualOutcome`s are keyed by force entry |
| `HeroDefinition { startingCards, pocketSlots, passives }` | `CardDefinition` on the Hero schema: number field, `DeckDefinition` reference, trigger list |
| Stable IDs, ID-keyed save DTOs | `CardAsset.Id` (`CardId`) + `CardInstanceSerializer` DTOs, reused verbatim in the run save |
| "Rebalancing a force is a one-asset change" | Literally true here: edit the Iron entry's law lists and every card, enemy preview and odds row inherits it |

The one deliberate divergence: **`GameSession` is not used.** Fate Deck's loop is not a
deck/draw/turn structure — it is *one interactive flip pipeline per action*, with pre-flip
pocket windows, bank windows, and mill-as-damage. `GameStep` lists can't express "pause twice
inside a single enemy attack". The package anticipates this: effects reach the engine through
the `IGameContext` seam, and SpireClimb itself hand-rolls its controller. So Fate Deck's
`FateSession` implements `IGameContext`, which keeps **every** OmniCard atom — effects,
conditions, triggers, trigger bindings, the event bus — running unchanged against a custom
engine. That seam is the package's best architectural decision and this game is its proof.

## 2. What OmniCard provides vs. what Fate Deck adds

**Straight from the package (no changes, no fork):**
`CardSchema` / `CardFieldDefinition` / field kinds & values, `MetadataKind` / `MetadataEntry`,
`CardDefinition` / `CardInstance` / `CardFieldState` (per-instance HP, pattern index, Howl
bonuses — all instance field overrides, all view-refreshing, all serializable),
`CardZone` (all ten zones), `EventBus` (+ polymorphic dispatch), `CardEffect` / `Condition` /
`CardTrigger` / `Subscription` / `CardTriggerBinding`, `DeckDefinition` / `DeckEntry`,
`ZoneDefinition` / visibility, `CardLayout` + `WorldCardViewBuilder` (all card faces),
`ZoneArrangement` poses (row / stack / fan), `ZoneCardClickHandler` + `PointerClickable` +
`CardPointer` (input-backend agnostic clicks), `CardInstanceSerializer` (run save DTOs),
and the editor-side idempotent content-generator pattern lifted from the samples.

**Game-side (in `Assets/FateDeck`, its own asmdefs):**

- `FateDeck.Runtime.Core` — `FateSession` (the `IGameContext`), `FateDeckService` (the five
  zones and every verb: flip, mill, reshuffle+tax, pocket, heal, exile, stack, scry, inject),
  `FateAction` + the interactive resolution phases, `OddsCalculator`, catalog + rules assets.
- `FateDeck.Runtime.Effects` — the law atoms (`ModifyActionForce`, `SetActionForce`,
  `NegateAction`, `EchoFlip`, `BankGold`, `BurnActionVictim`, `GuardRetaliateBurn`,
  `FlameLoot`, `DoomLoot`) and gameplay/enemy atoms (Scry, DoubleDraw, AddFateCard, RigTop,
  Tithe, Howl, Confiscate…). Law atoms implement `IActionLawPreview`, so the Odds Panel
  computes outcomes **from the same data the laws execute** — "No Surprise Math" is structural,
  not duplicated.
- `FateDeck.Runtime.Triggers/Conditions` — GDD A.2/A.3 atoms (`OnFlip` with context/force/owner
  filters, `OnReshuffle`, `OnMill`, `OnCombatStart`, `OnRoomEnd`, `OnPocketPlay`,
  `OnEnemyDeath`; `ZoneCount`, `OncePerCombat`, `HasStatus`).
- `FateDeck.Runtime.Combat` — `CombatEngine` (Main Action, beat-by-beat enemy phase over the
  interactive pipeline, Burn/Weak, enemy Block, bounties + pocketed gold, the Collector's
  Mantle), `EnemyPatternFieldKind`.
- `FateDeck.Runtime.Run` — `DoorDealer` (GDD weights, elite window 5–7, scripted step 1,
  guaranteed Forge), room definitions, events with force-keyed ritual outcomes, shop/shrine/rest
  services, `RunController`, `FateRunSave` (JSON, stable-id keyed, saved at every doors screen).
- `FateDeck.Runtime.Views` — `FateZoneView` (a `ZoneView` sibling that binds a plain `CardZone`)
  and the single-screen `FateTableView` (tableau-as-HUD, odds panels, pre-flip/bank/choice
  prompts, doors/chest/shrine/event/rest/shop/rewards screens, Honest-Ledger-lite death screen,
  Dealer barks).
- `FateDeck.Editor` — `FateDeckContentGenerator` (everything above as assets, idempotent,
  menu-driven) and `FateDeckSceneBuilder`.

## 3. Design decisions worth knowing about

- **Resolution order.** Pipeline per flip: `FateFlipEvent` publishes → relic `OnFlip` triggers
  run → the force's law list runs → (Echo may queue another flip) → Weak applies → commit.
  Relics therefore act *before* the law of the same flip — commutative for force math (Anvil
  Creed +1 stacks with Iron's +2 either way) and it lets future relics intercept a flip. If a
  relic ever needs strict "forces, then relics" ordering (GDD A.6), swap the publish below the
  law resolution in `FateSession.ApplyLaw` — one line.
- **Banking** is offered on the first flip of your own action only (Echo sub-flips can't be
  banked); banking resolves the action at base force, per GDD 3.5.
- **Void** negates the action and stops later laws; side effects already applied by an earlier
  Echo-chained law stand. Rare corner (Echo→Fortune→Void), documented rather than litigated.
- **Enemy Block** clears when that enemy starts its next round of actions; player Block and
  Flame's retaliate-burn clear at your turn start.
- **Locked chests**: spend a Key before the flip, or gamble on Flame (`OpensLock`); any other
  law leaves the chest shut and the card spent — a real bet.
- **Bought fate cards** join the discard pile (they enter your luck on the next shuffle).
- **Saves** happen when doors are dealt; quitting mid-fight resumes at that fight's door with
  the deck as it stood. Save data is `CardInstanceState` lists per zone keyed by `CardId` —
  rename-proof. The RNG stream is re-seeded on resume (`System.Random` state isn't exportable);
  a fresh `ResumeSeed` is stored instead.
- **`seed = 0`** on the table component means "random run"; any other value is a fixed seed
  (`FateSession` never runs unseeded internally, unlike `GameSession(seed: 0)`).

## 4. Gaps found in OmniCard while building this (candidates for the package backlog)

1. **`ZoneView` requires a concrete `GameSession`.** `Bind(GameSession)` + private zone
   resolution means custom engines can't reuse the package's zone presenter, flights, or
   drag-drop; `FateZoneView` re-implements a slice of it. An `IZoneSource` seam (or `Bind(Func<CardZone>)`
   overload) would let every custom-session game keep the package's presentation stack.
2. **`GameSessionDriver`-locked HUD.** `StatTextBinding`, `GameOverText`, `PlayedCardSpotlight`
   and `AudioCueBinding` all take the driver, not an event bus — SpireClimb hand-rolled its HUD
   and so does Fate Deck. Binding them to `IGameContext.Events` would make them universal.
3. **No mid-action interactivity model.** `GameStep` pauses *between* steps;
   `PendingTargets` pauses *inside* a play. Fate Deck needed a third kind of pause (windows
   inside a single resolution). A small reusable "await token" in the package could unify these.
4. **`CardZone` has no insert-at-index / bottom access** — injectors and scry-reorder rebuild
   the pile through `RemoveTop`/`Add` loops. `InsertAt(int, CardInstance)` would be enough.
5. **World builder skips `Color`-kind fields** (uGUI/UIToolkit render swatches) — per-force card
   tinting had to be done view-side. A `SpriteRenderer` swatch quad in `WorldCardViewBuilder`
   would close the gap.
6. **`EventBus` has no unsubscribe-all / scoped child** — long-lived sessions with many
   subscribers (table view) must track every handler to avoid leaks across runs; Fate Deck
   sidesteps it by discarding the whole session (and its bus) per run.
7. **`CardValue.AsInt()` truncation vs `Math.Round`** and `AsDouble()`'s silent `0` for text
   made a couple of law atoms defensive; a `TryAsNumber` would read better.
8. From the SpireClimb review, already known but confirmed here: no run-layer node→content
   binding, saves keyed by asset *name* in the sample (Fate Deck uses `CardId` instead — the
   package primitive was there, the sample just didn't use it).

None of these blocked the game; all have clean game-side workarounds noted above.

## 5. How to run it

1. The FateDeck project's `Packages/manifest.json` references the package locally:
   `"com.astergio.omnicard": "file:../../omni-card"` (sibling checkout). Open the project in
   Unity 6000.3.
2. `Tools → Fate Deck → Create Game Content` — generates every asset under
   `Assets/FateDeck/Generated/` (fields, forces+laws, fate cards, Biome 1 roster, items, rooms,
   layouts, catalog). Idempotent; safe to re-run after balance edits.
3. `Tools → Fate Deck → Create Game Scene` — camera + Fate Table (runs step 2 automatically if
   needed). Save the scene if you want.
4. **Press Play.** Click a door; in combat: Strike / Guard, `[FLIP FATE]`, sleeve a good card,
   click a pocketed card during an enemy's shimmer window to replace its flip. Click the draw
   pile any time for the exact composition. The run auto-saves at each doors screen
   (`Tools → Fate Deck → Delete Run Save` to reset).
5. Tests: `Window → General → Test Runner → EditMode` — the fate engine (tax, doom laundering,
   banking, pocket-replacement, odds math, door guarantees) runs headless in-memory.

Because every number lives in data, tuning is the GDD's intended workflow: edit the *Iron*
force entry or the *Fate Rules* asset and the whole game — including the Odds Panel —
re-balances itself.

## 6. What's next (post-slice roadmap)

Content-only (no new systems): Biome 2/3 rosters and bosses, remaining shrines/events, the
other 18 relics and 4 charms, the three locked heroes. Small-system work: Stakes (a
`StakeDefinition` with run-start effects — the atoms already exist), Glimmers/Pawnshop meta,
the full Honest Ledger (per-flip telemetry is already counted on the session), Turbo flip
ceremony, and the Balance Simulator hook — `GameSimulator` doesn't apply (no `GameSession`),
but `FateSession` is headless-friendly by construction, so a `FateSimulator` over seeded
sessions is an afternoon of work.
