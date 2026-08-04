# Fate Deck — Balance Lab: Method & Evidence

This documents how the simulation lab works and the evidence behind the current tuning.
The companion file `BalanceReport.md` is the auto-generated data snapshot; regenerate it any
time with `Tools → Fate Deck → Run Balance Simulation`.

## The lab

Three runtime pieces (in `Runtime/Simulation/`, no editor dependencies, fully headless):

- **AutoPlayer** — a deliberately-simple baseline bot. It reads the same `OddsCalculator`
  tables the Odds Panel shows, strikes for lethal, guards big incoming, pockets bad reveals,
  plays pocket interrupts against heavy enemy flips, spends Grit, heals when hurt, and shops
  greedily. It ignores charms, scry ordering, and relic choice entirely — so its win rate is
  a **floor**. A human should beat it comfortably; tuning targets the floor, not the human.
- **EventPolicy** — a heuristic score for event options (gold and heals good, taking Debt
  bad), so the bot makes non-degenerate event choices.
- **RunSimulator** — plays N seeded runs per hero (the same seed set for every hero, so
  heroes compare fairly), aggregates win rate, death-by-step histograms, Debt pressure,
  reshuffle counts, escrow, and killer attribution, and renders `BalanceReport.md`.

Simulated runs set `FateRunSave.Suppressed`, so they never touch the real save. A thousand
runs take about two seconds. `SimulationTests.cs` keeps the bot honest in CI: every seed
must terminate (no stalls), and the save file must stay untouched.

## Why the game felt unfair: the data

Baseline before tuning (300 runs/hero, seeds 101+):

| Hero | Win rate | Avg death step | Where deaths happened |
| --- | ---: | ---: | --- |
| The Gambler | 0% | 8.4 | 238 of 300 at the boss |
| The Stoker | 2% | 8.3 | 231 of 300 at the boss |
| The Actuary | 0% | 8.6 | 261 of 300 at the boss |
| The Debtor | 1% | 8.1 | 217 of 300 at the boss |
| The Sexton | 0% | 8.3 | 231 of 300 at the boss |

The run itself was healthy — steps 1–7 killed only ~15–25% of runs, concentrated in the
elite band (5–7), and nobody died at step 8 (rest + shop, as designed). Then **72–87% of
every run that reached THE COLLECTOR died there**, arriving with ~0.2 cards of deck left.
That matches the played experience ("the boss just grabbed my whole deck") exactly.

## No single lever fixes a compound spiral

Single-factor sweeps, 300 runs/hero, best hero's win rate shown:

| Change (alone) | Best floor |
| --- | ---: |
| Boss HP 30 → 26 | 4% |
| Opening Confiscate 3 → 2 | 3% |
| First attack 4 → 3 | 7% |
| Mantle bonus per 3 → 4 held | 2% |
| Appraise takes 3 → 2 | 2% |

Each lever alone barely moves the needle because the boss kill loop is self-reinforcing:
Confiscate steals the deck → the Mantle raises its Force → harder hits mill more → smaller
deck makes the next Confiscate proportionally worse. Breaking the spiral required softening
**several links at once** — which is also why the fight *felt* hopeless rather than hard.

## The bake (all four together)

THE COLLECTOR: **26 HP** (was 30), opening Confiscate **2** (was 3), attack loop **3/4**
(was 4/4), **+1 Force per 4 held** (was per 3). Appraise still takes up to 3 — the identity
stays; the compounding rate drops.

| Hero | Floor before | Floor after (1000 runs) |
| --- | ---: | ---: |
| The Gambler | 0% | 14% |
| The Stoker | 2% | 16% |
| The Actuary | 0% | 22% |
| The Debtor | 1% | 15% |
| The Sexton | 0% | 17% |

The Actuary (the information/control hero) on top and the Gambler (the variance hero) at
the bottom is the intended personality spread. Escrow-at-end ~11 and deck-at-end ~1 mean
runs still finish on fumes — dramatic, and on-theme.

## Two decisions the data settled

**Victory mend (the "not sure about that" experiment).** `VictoryMend = 1` — winning a
fight returns one escrowed card. Alone it moved nothing (0–2% → 0–2%): the problem was
never chronic attrition, it was the boss wall. After the boss bake it adds a small, real
nudge (the Debtor doubled, 5% → 10%, others ±2%). It ships **enabled at 1** anyway, for a
reason the histogram can't show: it makes winning a fight visibly *pay* — the exact
complaint "what is the reward for winning a fight?" — at negligible balance cost. It's one
number in Fate Rules; set it to 0 to turn it off.

**The Debtor's Grit interest.** The Debtor lagged the band (5–10%) because its 2 starting
Debt feed enemy laws all run and 3g per flip doesn't help mid-fight. Its passive now pays
**3g + 1 Grit** per surfaced Debt (a new `GainGritEffect` atom, reusable by events/charms).
Floor moved 10% → 14–15%, inside the band, and the hero now converts its curse into the
agency resource — which is what the fantasy promised.

## Re-running and extending

- In Unity: `Tools → Fate Deck → Run Balance Simulation (Quick)` (50 runs/hero) or
  `(Full)` (300). Writes `Documentation/BalanceReport.md`.
- After adding content: re-run Full and look at three things — the floor band per hero
  (target roughly 10–25%), where the death histogram bunches (a cliff at one step means a
  spiral, not a difficulty curve), and the killer attribution list (one name dominating
  means one number, not the game, is doing the killing).
- The bot is a measuring stick: resist improving it while tuning content, or the ruler
  changes length mid-measurement.
