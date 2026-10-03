# FateDeck — Handoff to Claude Code

Written 2026-10-03, at the end of a long Cowork (cloud) collaboration. This is the context
a fresh Claude Code session needs: what was decided and why, the rules the owner set, how
the code is shaped, what's open, and the traps. Repo: `C:\Users\lorda\Documents\Github\FateDeck`.
As of handoff, everything described here is committed through `31e22f7` ("Add the balance
lab..."); the only uncommitted changes are Unity-side churn (scene, URP settings, .slnx).

## 1. What this project is

A single-player roguelike for Unity 6000.3 (URP) where **the deck is the health bar, the
luck, and the build** — every random outcome in the game is a flip from one visible,
countable, sculptable deck. It is built on the owner's **OmniCard** package
(`com.astergio.omnicard`), referenced in `Packages/manifest.json` as a local sibling:
`"file:../../omni-card"`.

**Theme (load-bearing, not decoration):** you died owing Fate, and the House collateralized
your soul into a deck. Doom is **Debt**, the reshuffle tax is **Interest**, the wound row is
**Escrow**, and surfaced Debt banks **Grit**. Every player-facing name, bark, and description
is voiced as the House talking to a debtor. New content must keep this voice, per the owner:
*"go with the theme you choose but also it should make sense what it does"* — thematic names
that still telegraph mechanics.

The GDD (`fatedeckgdd.md`) lives with the owner, not in the repo.

## 2. Rules the owner gave (standing instructions)

1. **"Best practices, modular, extensible, easy to mod, design, iterate and grow."** This is
   the project's constitution. Concretely it became: data-driven everything (content is
   generated OmniCard assets, not code), small effect/trigger atoms over monoliths, partial
   classes for large surfaces, and tunables centralized in one rules asset.
2. **OmniCard is a read-only dependency.** Never edit `../omni-card`. Gaps found in it are
   documented in `Assets/FateDeck/Documentation/FateDeck-OmniCard-Integration.md`.
3. **No junk in the repo root.** In the cloud era, deliveries were tars extracted into the
   repo; the owner asked that no tar files be left lying around. They go to `_to_delete/`
   (gitignored), which the owner empties by hand. In Claude Code you edit files directly, so
   this mostly retires — but `_to_delete/` still exists and currently holds old tars plus a
   `stray-root-Documentation/` folder; all safe to purge.
4. **Explain the game to the player, in the UI.** Recurring owner complaint; the standing
   answer is hover tooltips everywhere, per-context second-person law text, badges, and the
   Dealer explaining mechanics the first time they fire. Any new mechanic ships with its
   tooltip/glossary text, not just its code.
5. Owner playtests and reports by feel ("it feels unfair", "I could not figure out X").
   Treat these reports as real signals and chase the mechanism — every one so far has had a
   measurable cause.

## 3. Architecture and the decisions behind it

### Core seam
- `FateSession : IFateSession, IGameContext` (`Runtime/Core/`) is the game context all
  OmniCard atoms resolve against — mirrors OmniCard's own `context.Game` casting pattern.
  **Why:** every package atom (effects, triggers, conditions) runs against FateDeck
  unchanged, and FateDeck's own atoms stay interchangeable with package ones.
- `FateDeckService` owns the five fate zones (Draw, Discard, Wound/Escrow, Pocket, Exile),
  reshuffle + Interest, milling, healing. `CombatEngine` owns fights (intents, patterns,
  Mantle, statuses). `RunController` owns the run (doors, rooms, events, shops, saves,
  rewards). `ShopService`, `DoorDealer` are small and single-purpose.
- `OddsCalculator` is **pure static** and context-aware. `DescribeLaw(force, lawField,
  context)` is the single source of law text for tooltips, odds rows, and the glossary.
  **Why:** the UI's explanation and the engine's execution derive from the same data, so
  they cannot drift.

### The perspective fix (a key decision)
The owner's deepest confusion was "when the enemy draws a card that says 'you get X', am I
'you'?" The fix: laws are stored per context (`LawContext` → Offense/Defense/Enemy/Loot/
Ritual fields on each force), and atoms implement `IContextDescribed.DescribeFor(context)`
to emit second-person text from the reader's seat ("YOU suffer 2 Burn" vs "your target
suffers 2 Burn"). New law atoms must implement `IContextDescribed` (and usually
`IActionLawPreview`) or they'll read ambiguously in tooltips.

### The fairness set (why the game stopped feeling rigged)
Decisions made in the "rethink balance" pass, all data-confirmed later:
- Enemy-context laws cost **~60–70% of the player version** (Iron: +2 for you, +1 against
  you). Symmetric laws made every deck improvement empower enemies equally — never reintroduce
  a symmetric law without pricing it.
- **Outnumbered Guard** (`OutnumberedGuardDamage = 2`): vs 2+ enemies, Guard also strikes —
  answers "two attackers feel undefendable."
- **Squad purse** (`SquadPursePerExtraEnemy = 2`) and **VictoryMend = 1** (win a fight, one
  card leaves Escrow): fights visibly pay.
- **Void refunds the Main Action once per combat** (`ClaimOnce("void-refund")`): a voided
  turn is a tempo event, not a wasted turn.
- **Depth gating** (`RoomDefinition.MinStep`): heavy rooms can't appear early.
- **Grit** (`GritPerDebtFlip = 1`, spend 3, max 6): Debt flips bank agency; spends are
  Foresight (Scry 2 + reorder), Momentum (+2 next action), Mend (heal 1). This is the
  anti-frustration engine — bad luck accrues into controllable resources.

### Data-driven content (the most important contract in the repo)
All game content is generated by `Editor/FateDeckContentGenerator.cs` + partials
(`.Fields/.Forces/.Enemies/.Items/.Heroes/.Rooms/.Catalog/.Layouts`) into
`Assets/FateDeck/Generated/`, catalogued by one `FateContentCatalog` asset.

**Idempotency contract — read this twice:**
- `GetOrCreate<T>` returns an existing asset untouched (its `initialize` lambda runs on
  **create only**). User edits to generated assets survive re-running the generator.
- Force **entries** re-sync their law/text data on every run — that's why theme re-voicing
  reached existing projects without a rebuild.
- But **card/enemy/hero/room stats and patterns are create-only.** Changing a number in the
  generator does nothing for an existing project until
  `Tools → Fate Deck → Rebuild Content From Scratch` (deletes `Generated/` + run save,
  regenerates, relinks the scene). This is deliberate: edits-survive beats auto-sync for a
  moddable game, per rule 1.
- `EnsureKindFields`/`EnsureSchemaFields` append newly-added fields to existing kinds/schemas
  so old assets upgrade safely. Follow that pattern when adding fields.

### UI
Runtime **UI Toolkit, entirely code-built** — no UXML/USS files. `FateTableView` + partials
(`.Combat/.Rooms/.Tableau/.Overlays`), `FateUi` (styles/factories), `CardElementBuilder`
(card faces via OmniCard's `UIToolkitCardViewBuilder`), `GameLogPanel` (the ledger),
`UiFx` (tweens), `FateTip` (runtime hover tooltips — custom because UI Toolkit runtime has
no native tooltip). The generator creates `PanelSettings` + default runtime theme.

### Persistence
`FateRunSave` (JSON in `persistentDataPath/fatedeck-run.json`) captures zones through
OmniCard's `CardInstanceSerializer`, keyed by stable ids. Decisions that matter:
- **`DoorIds` are saved and restored verbatim** — loading must never reroll offered doors
  (was a reported bug; `RestoreDoors` falls back to a fresh deal only if an id no longer
  resolves after a content rebuild).
- `OriginalSeed` is preserved for display/reproducibility; `ResumeSeed` reseeds the RNG on
  each save so save-scumming a flip isn't free.
- **`FateRunSave.Suppressed`** (static bool): any headless/batch play must set it (and
  restore in `finally`) so simulations never touch the real save. `AutoPlayer` does this.

### The balance lab (newest system — Pass 5)
`Runtime/Simulation/`: **AutoPlayer** (baseline bot that reads the same `OddsCalculator`
tables the Odds Panel shows), **EventPolicy** (heuristic event scoring), **RunSimulator**
(N seeded runs per hero, same seed set per hero, aggregates + renders markdown).
`Editor/FateDeckBalanceLab.cs` adds `Tools → Fate Deck → Run Balance Simulation (Quick/Full)`
→ writes `Assets/FateDeck/Documentation/BalanceReport.md`. ~1000 runs ≈ 2s.

**Method decision:** the bot is deliberately weak (no charms, no scry ordering, blind relic
picks) so its win rate is a **floor**. Tuning targets the floor band (~10–25% per hero).
**Do not improve the bot while tuning content** — it's the measuring stick.

**What the data decided** (full evidence: `Assets/FateDeck/Documentation/BalanceLab.md`):
- Pre-tune, 72–87% of all runs died at THE COLLECTOR (bot floor 0–2%). Single-lever sweeps
  barely moved it — the boss was a compounding spiral (steal → stronger → mill more → steal
  more), so four links were softened together: 26 HP, opening Confiscate 2, attacks 3/4,
  +1 Force per **4** held. Floor became 14–22%.
- `VictoryMend` alone moved nothing (the problem was the wall, not attrition) but ships
  enabled at 1 because it makes winning visibly pay — the owner's explicit complaint — at
  negligible cost. It answered the owner's "not sure about that" with evidence.
- The Debtor lagged (5–10%) → passive now pays **3g + 1 Grit** per surfaced Debt (new
  `GainGritEffect` atom). Floor 15%, in band.
- Current floors (1000 runs/hero): Gambler 14, Stoker 16, Actuary 22, Debtor 15, Sexton 17.
  Actuary (control hero) on top, Gambler (variance hero) at the bottom — intended spread.

### Testing
`Tests/Editor/` — real NUnit EditMode tests, **fully headless**: `TestContent.Create()`
builds an in-memory catalog (no AssetDatabase, mirrors the generator's shape), so tests run
without generated assets. 26 tests as of handoff, including `SimulationTests` (every seed
terminates, save stays untouched, aggregator accounts for every run).
`Runtime/AssemblyInfo.cs` grants `InternalsVisibleTo("FateDeck.Tests.Editor")` — internals
used by tests (`RestoreGrit`, `RestoreRunState`, …) depend on it.

**Gotcha:** `TestContent` is a manual mirror. When you add catalog fields/forces/zones, add
them there too or engine tests won't exercise them.

## 4. Verification workflow — what changes in Claude Code

In the cloud there was no Unity, so verification ran through a Roslyn sandbox with
hand-written stubs (UnityEngine/UnityEditor/UIElements/NUnit), a **two-stage build**
(game.dll, then tests referencing it, so `InternalsVisibleTo` was enforced exactly like
Unity — a single-assembly build once hid a real CS1061), and a reflection test runner.
That sandbox lived only in the cloud session; **don't look for it in the repo.**

In Claude Code, replace all of that with the real Editor: the **unity-bridge** plugin's
verifier (compile + Console + EditMode tests through the running Editor) or Unity's Test
Runner directly. The balance lab runs in-Editor from the menu.

One cloud trick worth knowing anyway: the UnityEditor stubs turned `AssetDatabase` into an
in-memory factory (`LoadAssetAtPath → null`, `IsValidFolder → true`,
`CreateInstance` reflect-invokes `OnEnable`), which let
`FateDeckContentGenerator.CreateAssets()` build the **real** content headless, and the sims
sweep tuning candidates via env knobs in seconds (`FATEDECK_SIM`, `FATEDECK_MEND`,
`FATEDECK_BOSS_HP/OPEN/ATK1/ATK2/PER/TAKE`, `FATEDECK_SEED`). If you ever need mass sweeps
outside Unity, that recipe is cheap to rebuild; otherwise the in-Editor menu suffices.

## 5. Code conventions observed in this codebase

- Allman braces, four spaces, explicit types preferred, `_camelCase` private fields,
  `///` XML docs on public API (often *why*, not just *what*), `sealed` where applicable.
- Big classes split into **partials by concern** (`FateTableView.Combat.cs`,
  `FateDeckContentGenerator.Forces.cs`), with `// ---- section` divider comments.
- Effects/triggers are **small serializable atoms** (one job each) with `GetName()`/
  `GetDescription()`; gameplay-facing ones add `IActionLawPreview` and, for laws,
  `IContextDescribed`. New mechanics = new atoms, composed in the generator — content stays
  data, systems stay reusable (e.g. `GainGritEffect` is usable by heroes, events, charms).
- Tunables go in `FateRulesDefinition` (one asset, `[Min]`-guarded, tooltipped) — never
  scatter magic numbers. Current notable values: Strike 3 / Guard 2, Interest 1, Pocket 2,
  TrackSteps 9, DoorsPerStep 3, MantleSpill ≥5, OutnumberedGuard 2, SquadPurse 2,
  Grit 1/3/6, VictoryMend 1.
- Mills route through `MillPlayer(count, reason)` — **always pass a reason**; the ledger
  attributes every loss by name ("the Debt", "the well"), part of the fairness answer.
- Logging goes through the session's injected log sink (`RunController(catalog, log)`);
  sims pass a no-op. Barks (`Session.Bark`) are events, not logs — the Dealer's voice.

## 6. Open tasks and sensible next steps

Nothing is broken or half-finished; Pass 5 closed the roadmap from the design review
(`Assets/FateDeck/Documentation/FateDeck-Design-Review.md`). Open, in rough priority:

1. **Verify the retune landed in the owner's project.** The boss/Debtor changes live in
   generated assets → they require `Tools → Fate Deck → Rebuild Content From Scratch` once,
   then `Create Game Scene`. If the owner's COLLECTOR still shows 30 HP, the rebuild
   hasn't happened. (`VictoryMend = 1` arrives automatically via the C# field-initializer
   default on the existing Fate Rules asset.)
2. **Human playtest the new boss curve.** The lab says floors 14–22%; if humans now crush
   the Collector, restore opening Confiscate 2 → 3 first (it's the least feel-bad lever).
   Re-run the Full sim after any content change and check: floor band 10–25%, no histogram
   cliff at one step, no single killer dominating attribution.
3. **Biome 2.** The victory bark already teases "the Mire". The structure is ready:
   `Biome` is tracked in the run/save, rooms/enemies/forces are data, and the catalog's
   biome-1 fields (`Biome1Rooms` etc.) would generalize to per-biome sets. This is the
   natural next big pass.
4. Smaller candidates: more elite/boss variety, deeper charm/relic synergies (the atoms
   make these cheap), event-weight pass with the sim as judge, meta-progression/unlocks,
   audio (untouched so far).
5. Housekeeping: purge `_to_delete/`; consider committing the pending Unity-side changes
   (scene/settings churn) separately from code.

## 7. Gotchas (the expensive lessons)

- **Generator stats are create-only** (§3). Changing generator numbers without a rebuild
  silently does nothing in an existing project. Conversely, a rebuild **discards manual
  edits to generated assets** and deletes the run save — it warns, but know what it costs.
- **New serialized fields on existing assets read the C# field-initializer default** until
  re-saved. Used deliberately for `VictoryMend = 1`; remember it cuts both ways when you
  add fields and expect 0.
- **`FateRunSave.Suppressed`** around any automated play, restored in `finally` — or a sim
  will eat the player's real run.
- **Never report a mechanic the data can't see:** only the draw pile is visible to the
  Collector's Confiscate (discard/pocket/wounds are safe) — gimmick text promises this, the
  engine honors it; keep such promises synchronized when touching either side.
- **Enemy law pricing:** any new force needs its enemy-context law priced at ~60–70% of the
  player version, or deck improvements feed the enemies again.
- **`TestContent` drift:** new catalog surface → mirror it in `TestContent.Create()`.
- **Doors must survive loads** — if you touch door dealing or saves, keep `DoorIds`
  restoration verbatim (reroll-on-load was a real reported bug).
- **Docs path convention:** project docs live in `Assets/FateDeck/Documentation/` (next to
  the README at `Assets/FateDeck/README.md`), **not** a repo-root `Documentation/` folder.
  The balance menu writes its report there too.
- **AutoPlayer event guard:** repeatable events are capped at 3 takes per visit and runs at
  8000 iterations (`Stalled`) — keep both if you extend the bot, they're the infinite-loop
  insurance.
- The duplicate-looking commits (`5577520`, `31e22f7`) share a message — history quirk from
  the handover era, harmless.

## 8. Starter CLAUDE.md (copy if useful)

```markdown
# FateDeck (Unity 6000.3, URP)

Single-player roguelike; the deck is the health bar. Built on the local OmniCard package
(../omni-card — READ-ONLY dependency, never edit it).

- Read FateDeck-handoff.md first; evidence/design docs in Assets/FateDeck/Documentation/.
- Owner's rules: best practices, modular, extensible, easy to mod/iterate/grow. Keep the
  House/debt theme in every player-facing string; names must telegraph mechanics.
- All content is generated (Editor/FateDeckContentGenerator*): change the generator, not
  Generated/ assets. Stat changes need Tools → Fate Deck → Rebuild Content From Scratch.
- Tunables: FateRulesDefinition. Law text: OddsCalculator.DescribeLaw (single source).
- Verify via the Unity Editor (compile + EditMode tests; 26 tests, headless TestContent).
- Balance by data: Tools → Fate Deck → Run Balance Simulation; target bot floor 10–25%
  per hero; don't improve the bot while tuning content.
- Wrap any automated play in FateRunSave.Suppressed. Always pass a reason to MillPlayer.
```

— End of handoff. The game is in a good, data-backed place; protect the idempotency
contract and the theme, and it will keep growing cleanly.
