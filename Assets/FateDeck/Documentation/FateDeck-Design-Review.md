# FATE DECK — Design Review & Direction

*A thinking pass, no code. Grounded in the game as it currently plays: what confuses, what's unfair, what's actually good, and where to take it.*

---

## 1. What this game is (the theme, stated plainly)

The game already has a theme hiding in its bones — it just hasn't been said out loud anywhere, so the names drift. Here it is:

**You died owing Fate. The House collateralized your soul into a deck of cards, and you are climbing out of a debtors' purgatory, floor by floor, buying yourself back.**

The Dealer is your case officer. Every enemy is a creditor, clerk, or repossession instrument of the House — which is why Biome 1 is a **Mailroom** full of dead letters, wax cherubs, paper golems, ledger wisps, notaries and underwriters. THE COLLECTOR doesn't want you dead; it wants your *assets*. Biome 2 (The Mire) and Biome 3 (The Vault) are deeper departments.

Everything in the game should speak this language:

- Your deck is **your Worth** — health, luck and identity in one stack. "15 cards. That is what you are worth today" is already the best line in the game; it should be the thesis.
- Damage isn't damage, it's **collection**. Wounds aren't wounds, they're cards **held in escrow** — recoverable, because the House is scrupulous about paperwork.
- The reshuffle tax isn't a tax, it's **interest**. You are cycling borrowed luck, and the House charges for it. (This single re-frame answers "what does reshuffling do" better than any tooltip — see §4.)
- Doom is the House's **lien** on you — consider renaming it **Debt** outright. "The House adds 1 Debt" is instantly understood by anyone; "1 Doom" is generic fantasy. This is the biggest single naming decision on the table.
- Exiling is **settling** a debt / burning a contract. Victory over the boss is **buying back your file**.

The mechanical vocabulary mostly survives contact with this theme (Pocket, Scry, Block are fine as table/casino terms). What needs a voice pass is the *descriptions*: forces currently read like elemental spells ("Lightning: your actions +1 Force…"). Re-voiced, Iron becomes "the House's standard weight — every action presses +2 harder", Fortune is "a skimmed ledger — the acting party pockets 3g", Glass is "counterfeit brilliance — spend it once, watch it shatter." Same rules, one voice.

**Recommendation:** adopt the theme statement above verbatim into the README and the in-game help; do a full description/bark voice pass; decide on the Doom→Debt rename (my vote: yes) and re-label the reshuffle tax as **Interest** (strong yes — it's clearer *and* more thematic, a rare free win).

---

## 2. Why the game currently feels unfair — the four real causes

I think the "unfair" feeling is real, and it isn't one problem. It's four stacked problems, and only one of them is numbers.

**Cause 1 — Perspective ambiguity: you often don't know what just happened to whom.** The laws are written role-neutrally ("the action's owner banks 3 gold", "the action's target suffers 2 Burn"). When *you* strike, the target is the enemy; when the *enemy* attacks, the target is *you* — same sentence, opposite meaning. Your burn question is exactly this: no, a burned enemy's attack does not burn you back — what happened is the enemy's attack flipped **Flame from your own deck**, and Flame's law says the *target* burns. The target of an enemy attack is you. The rule worked perfectly; the sentence betrayed you. A player who can't attribute damage will always experience it as unfairness, whatever the numbers say. This is fixable purely with words (§4).

**Cause 2 — Your deck empowers your enemies symmetrically, and the strongest buy is a defensive trap.** The core genius of the game is that enemies flip *your* deck: your luck is a shared pool, and sculpting it cuts both ways. But right now Iron gives **+2 in every context** — including enemy attacks. So the most natural purchase in the game ("buy more damage") silently makes every enemy hit harder, and the player who stacks Iron feels the game cheating them without understanding why. The *idea* is right; the *symmetry* is too literal. The fix is to cost enemy-context laws at roughly 60–70% of player-context laws (see the number sheet in §7): Iron +2 on your actions but only +1 on enemy actions ("the blow is heavier, but it's your iron — it resists being turned against you"). This one change re-tilts the whole game toward "sculpting is good" without touching the elegant core.

**Cause 3 — Action economy: two enemies attack twice, you act once.** Against pairs you mathematically cannot Guard both attacks and cannot race both healths, so early pair rooms are quietly lethal, and the player correctly senses "no chance to defend." The interrupt tools (Pocket, charms, Weak) help but are earned, not given. This needs a *structural* answer, not just softer numbers — see the **Outnumbered rule** in §5.

**Cause 4 — The rewards and the clock are invisible.** Fights risk your literal life total for a bounty that is never shown on the door, and the reshuffle clock ticks silently until Doom appears "out of nowhere." Invisible cost + invisible reward = perceived unfairness even when the math is fine. Doors should advertise bounty and drop odds; the reshuffle should be telegraphed loudly before it happens and attributed when it does; every mill should say *why* it happened (§4).

The encouraging part: none of these four requires abandoning anything. The deck-as-everything core, the public odds, the one-visible-deck honesty — all of it holds. The game isn't broken; it's under-explained and mis-costed in three places.

---

## 3. Your direct questions, answered as the game should answer them

These answers should become in-game text — if a design needs me to explain it in a chat, the game failed to say it.

**"Is the enemy me when the enemy draws this?"** Every flip, always, comes from **your** deck — there is only one deck at the table, and it's you. When an enemy attacks, it *spends your luck*: the flipped card's **Enemy-Action law** applies, and in that law "the action" is the enemy's attack, "the owner" is the enemy, "the target" is you. The fix is to never make you do that translation: all law text will be rewritten per-context in second person ("YOU suffer 2 Burn", "the enemy pockets 3g", "the blow weakens — you take 2 less").

**"What does reshuffling do?"** When your draw pile empties mid-flip or mid-mill, your discard pile shuffles back in to become your new draw pile — *and the House adds one Doom card as interest*. That's the whole rule. Its strategic weight: every cycle of your deck costs you one permanently-worse card, so the run has a metronome — fast, thin decks cycle more and accrue interest faster; fat decks are slow but tax-resistant; wounds thin your live deck and *accelerate* the clock, which is the hidden death spiral. The game should show the metronome: a small "Interest due in N cards" indicator on the draw pile, a warning beat when N ≤ 2, and a loud, attributed log line when it happens.

**"Does an attack from an enemy I burned also burn me?"** No. Enemy Burn only damages the *enemy* at round end. What you saw was Cause 1: its attack flipped Flame from your deck, and Flame burns the attack's target — you. Once law text is second-person and every status line is attributed ("Your Burn eats 2 cards", "Rat's Attack + IRON: you mill 5"), this ghost disappears.

**"What are the rewards for winning a fight?"** Currently: the enemy's bounty in gold, plus any gold it pocketed during the fight, plus a 25% charm drop; elites add a relic choice; the boss ends the biome. Two problems: none of this is visible up front, and for multi-enemy rooms it under-pays the risk. Proposal in §5: doors advertise their purse ("FIGHT: Rat — purse 5g · 25% charm"), multi-enemy rooms pay a **squad bonus**, and every victory **returns one wound card to your deck** ("the House pays out and un-escrows a page of you") — a small guaranteed heal that makes fighting sustainable *and* is the single biggest fairness lever available, because it converts fights from pure attrition into a rhythm of loss-and-partial-recovery.

---

## 4. The Clarity Plan (make the game explain itself)

**4.1 Perspective-correct law text, generated from the rules themselves.** Each law atom already *knows* who it affects in each context (the code that resolves it branches on exactly that). The plan is to have every atom produce its own per-context sentence — "on your Strike: the target suffers 2 Burn" / "on an enemy attack: **you** suffer 2 Burn" — and use those generated sentences everywhere text appears: odds rows, tooltips, the glossary, the pile inspector, and the flip banner. One source of truth, so the words can never drift from the behavior. The odds panel becomes fully context-aware: the INCOMING panel's Flame row will literally say "you suffer 2 Burn."

**4.2 Attribution on every loss.** Every mill names its cause: "Rat's Attack 5 (Block absorbed 2) — you mill 3", "Your Burn eats 2 (Burn falls to 1)", "The Doom trap bites — mill 2", "Interest: the House adds 1 Doom." Losing to a visible ledger feels like a lesson; losing to an anonymous drain feels like theft.

**4.3 The Dealer teaches, once.** The first time each mechanic fires in a run — first mill, first reshuffle, first Burn on you, first pocketable reveal, first enemy pre-flip window, first Doom — the Dealer delivers one explanatory bark ("\"That's escrow, not the grave. Win, and I un-tear a page.\""). Diegetic tutorial, zero UI, already have the bark channel.

**4.4 The clock, visible.** Draw pile shows "Interest due in N" beneath the count; at N ≤ 2 a quiet warning line appears; the reshuffle banner says what it did and what it cost. Rename the event "Interest" (or "the Rake") everywhere.

**4.5 Doors advertise stakes.** Every fight door lists purse + drop chance; chest doors list base gold; shrine/event doors keep their blurbs. The player should *choose* risks, never discover them.

**4.6 Bug & inspection fixes (logged for the implementation pass).** (a) The deck count showing 0 at run start is a refresh-ordering bug — the tableau is drawn from the session-started hook before the starting deck is built, and nothing re-draws it until the first deck event; the fix is to refresh the tableau on every screen rebuild. (b) Discard, wound and exile piles all become clickable with the same inspector overlay the draw pile has (showing composition; discard additionally shows exact order since it's public knowledge). (c) The squash problem gets a standing rule: *every dynamically-growing list lives in a capped, scrollable container* — wound row and relics already do; charms, pocket row, prompt strip, enemy chips and the top status chips get the same treatment, and the tableau bar gets a fixed height it can never exceed.

---

## 5. The Fairness Plan (structural, not just numeric)

**5.1 Soften enemy-context laws (~60–70% of player value).** The flagship change from Cause 2. Your deck should still cut both ways — that's the game — but sculpting must be net-positive. Concretely (full sheet in §7): Iron +2/+1 (yours/enemy's), Iron+ +3/+2, and a review of every force's enemy line with the question "does buying this card feel like self-harm?"

**5.2 The Outnumbered rule.** When two or more enemies are alive, **your Guard also strikes your current target for 2** ("you guard with the sharp edge out"). One sentence, no new UI, and it fixes the pair-fight math: Guard stops being a wasted turn against groups, becoming chip-damage-plus-mitigation, while single-enemy fights keep the clean Strike/Guard tension. Pair it with a light audit of pair encounters (pairs of individually-weaker enemies, never two full-strength patterns) and a **squad purse** (+2g per extra enemy) so groups are a deliberate high-risk/high-pay door.

**5.3 Victory un-escrows a page.** Winning any fight automatically returns 1 wound card to your deck, loudly attributed. This is the sustain valve the run is missing — currently healing lives only in shrines/rests/items, so every fight is pure attrition and the death spiral (wounds → thinner deck → faster interest → more Doom) has no natural brake. One card per victory is small enough to keep attrition real, big enough to make "fight more" a viable strategy identity.

**5.4 Depth gating for rooms.** The door dealer currently draws every fight from one flat pool, so Paper Golem (attack 5) can appear on step 2 against a 15-card deck. Rooms get a depth band (early/mid/late); early steps deal from the shallow end. This is invisible fairness — the kind players feel but never see.

**5.5 The Void refund.** Void currently negates your whole action — with Doom also whiffing you, the game has two "your turn was for nothing" outcomes, and total turn loss is the worst feeling in turn-based games. New Void (player actions only): the action fizzles *but your Main Action is refunded* (once per combat, so it can't loop). "The Void takes nothing. Not even your time." Doom keeps its crown as the one true disaster — that's its identity — but now it's the *only* one.

**5.6 Doom pays Grit (the proposal I most want to test).** Every Doom that surfaces grants 1 **Grit**; at 3 Grit you may spend it on one of: Scry 2 & reorder, +2 Force on your next action, or return 1 wound card. Bad luck becomes a slow-charging battery — the House's insults harden you. This converts the game's worst moments into future agency, smooths variance without deleting it, and gives Doom-heavy decks (the Debtor!) a real identity. It's the one genuinely new system in this review; everything else is adjustment. If it tests well it becomes a pillar; if it clutters, drop it without regret.

---

## 6. The Control Plan (randomness you can lean on)

First, naming what already exists — the game has more control than it communicates, and part of the fix is teaching the existing kit as a kit: **Sleeve** (bank a flip, resolve at base), **Pocket play** (replace any flip, yours or theirs), **Scry/reorder** (see and shape the top), **Stack** (Second Sleeve: rig your own next flip), **Draw-2** (flip two, choose), **composition sculpting** (every purchase/exile IS a probability edit, live in the odds panel), targeting, statuses, and Flee. The help overlay should present these as "YOUR CONTROL KIT" — one screen, nine verbs.

What's genuinely missing is a *universal, always-available* micro-decision, because when your pocket is full (or the flip isn't pocketable) a player action resolves with zero input. Candidate: **Press** — once per combat, after seeing your revealed card, pay 1 card off the top (mill 1) to discard the reveal and flip again. A costed mulligan: rare enough to stay tense, universal enough that no turn is ever pure spectation. I'd ship Press *or* Grit first, not both at once — two new currencies of agency in one patch would blur what's doing the work.

And one deliberate *anti*-control stance, stated openly: enemy attacks flipping your deck stays. It's the game's signature. The fairness work above (soften enemy laws, telegraph windows, second-person text) is what makes that signature feel like a dark deal you understand rather than a cheat — the goal is "I knew the odds and gambled," never "the game decided."

---

## 7. The Number Sheet (current → proposed)

| Thing | Now | Proposed | Why |
|---|---|---|---|
| Iron / Iron+ on enemy actions | +2 / +3 | **+1 / +2** | Cause 2: buying damage shouldn't arm the enemy 1:1 |
| Fortune on enemy actions | enemy pockets 3g | pockets **2g** | Softer symmetric sting; still recoverable on kill |
| Echo on enemy actions | extra flip | extra flip (unchanged) but **never chains Doom twice** | Keep chaos, remove the one unsurvivable chain |
| Doom on enemy actions | +2 | +2 (keep) | Already tuned last pass; it's *supposed* to hurt |
| Void on player actions | negate turn | negate + **refund Main Action** (1×/combat) | Removes double "wasted turn" outcome |
| Victory reward | bounty + 25% charm | + **heal 1 wound**, squad bonus **+2g/extra enemy**, purse shown on door | The sustain valve + visible stakes |
| Pair encounters | full-strength duos | duos built from weaker patterns; depth-gated | Action-economy honesty |
| Guard vs 2+ enemies | base 2 Block | base 2 Block **+ strike 2** (Outnumbered) | Fixes the "can't defend" math |
| Room pool | flat | **depth bands** (steps 1–3 / 4–6 / 7+) | No step-2 Paper Golem |
| Reshuffle tax | 1 Doom, silent | 1 Doom, **telegraphed 2 cards early, attributed, renamed Interest** | Same cost, zero ambush |
| Starting decks | 15 cards, 1–2 Doom | unchanged | They're fine once the above lands |
| Grit (new, experimental) | — | 1 per Doom flip; spend 3: scry 2 / +2 next / heal 1 | Bad luck → agency |

What stays random **on purpose**: which card flips (the whole game), door deals, chest outcomes, event rituals, shop stock, charm drops. The design goal is not less randomness — it's that every random event is *priced* (odds panel), *telegraphed* (windows, interest meter, intents), *attributed* (the ledger log), and *answerable* (the control kit). Randomness you can see coming and lean against is tension; the rest is noise, and the plans above are all noise-removal.

---

## 8. Roadmap (when we go back to code)

**Pass 1 — Truth & sight (clarity):** per-context generated law text everywhere; mill/loss attribution; Dealer-teaches-once barks; Interest meter + telegraph; door purses; pile inspectors for discard/wound/exile; the deck-count-0 refresh bug; the scroll-cap rule for every growing list.

**Pass 2 — The fairness set:** enemy-law softening; Outnumbered; victory heal + squad purse; depth bands; Void refund; pair-encounter audit. (All small, all testable with the existing headless suite — each gets a test.)

**Pass 3 — Theme voice:** the Doom→Debt / Interest / escrow decision, then a single writing pass over every description, bark, blurb and the help overlay in the House's voice.

**Pass 4 — The experiment:** Grit *or* Press, behind a rules toggle, evaluated honestly.

**Pass 5 — Balance by data, not vibes:** the engine is already deterministic and headless — we can build a simulation harness that auto-plays hundreds of seeded runs with simple policies (always-strike, guard-when-lethal, sculpt-greedy) and reports death-step distributions, doom counts, and gold curves per hero. Then every future number change comes with a before/after chart instead of a feeling. This is the payoff of the architecture, and I'd genuinely prioritize it right after Pass 2.

---

## 9. Open decisions (yours)

1. **Rename Doom → Debt** (and tax → Interest, Wound Row → Escrow)? Full rename, Interest-only, or keep current names?
2. **Grit or Press** as the new agency system — or neither for now?
3. **Victory heal** — comfortable with 1 wound per win, or prefer it as a relic/hero passive instead of a core rule?
4. Anything above you'd veto before I start Pass 1?
