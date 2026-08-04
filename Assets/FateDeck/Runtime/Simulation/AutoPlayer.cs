using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Combat;
using FateDeck.Runtime.Core;
using FateDeck.Runtime.Run;

namespace FateDeck.Runtime.Simulation
{
    /// <summary>The outcome of one simulated run, for aggregate balance statistics.</summary>
    public sealed class RunResult
    {
        public bool Victory;
        public bool Stalled;
        public int DeathStep;
        public int Flips;
        public int DebtFlips;
        public int Reshuffles;
        public int FinalGold;
        public int EscrowAtEnd;
        public int DeckAtEnd;
        public string FatalAction;
    }

    /// <summary>
    /// A deliberately-simple baseline player: reads the same odds the Odds Panel shows,
    /// strikes for lethal, guards big incoming, pockets bad reveals, plays interrupts
    /// against heavy attacks, heals when hurt and shops greedily. It ignores charms and
    /// scry ordering entirely, so a human should comfortably beat its win rate.
    /// </summary>
    public sealed class AutoPlayer
    {
        private const int MaxIterations = 8000;

        private readonly FateContentCatalog _catalog;
        private int _eventTakes;

        public AutoPlayer(FateContentCatalog catalog)
        {
            _catalog = catalog;
        }

        public RunResult Play(CardDefinition hero, int seed)
        {
            bool previousSuppression = FateRunSave.Suppressed;
            FateRunSave.Suppressed = true;
            var run = new RunController(_catalog, _ => { });
            try
            {
                run.StartNewRun(hero, seed);
                int guard = 0;
                RunScreen lastScreen = run.Screen;
                while (guard++ < MaxIterations)
                {
                    if (run.Screen != lastScreen)
                    {
                        lastScreen = run.Screen;
                        _eventTakes = 0;
                    }

                    FateSession session = run.Session;
                    if (run.Screen == RunScreen.Dead || run.Screen == RunScreen.Victory)
                    {
                        break;
                    }

                    if (session.Phase != FateResolutionPhase.Idle)
                    {
                        StepPhase(session);
                        continue;
                    }

                    switch (run.Screen)
                    {
                        case RunScreen.Doors: ChooseDoor(run); break;
                        case RunScreen.Combat: StepCombat(run); break;
                        case RunScreen.Chest: StepChest(run); break;
                        case RunScreen.Shrine: StepShrine(run); break;
                        case RunScreen.Event: StepEvent(run); break;
                        case RunScreen.Rest: StepRest(run); break;
                        case RunScreen.Shop: StepShop(run); break;
                        case RunScreen.Rewards: StepRewards(run); break;
                        default: run.CompleteRoom(); break;
                    }
                }

                return Summarize(run, guard >= MaxIterations);
            }
            finally
            {
                run.Session?.Dispose();
                FateRunSave.Suppressed = previousSuppression;
            }
        }

        private RunResult Summarize(RunController run, bool stalled)
        {
            FateSession session = run.Session;
            return new RunResult
            {
                Victory = run.Screen == RunScreen.Victory,
                Stalled = stalled,
                DeathStep = run.Screen == RunScreen.Dead ? run.Step : 0,
                Flips = session.TotalFlipsThisRun,
                DebtFlips = session.DoomFlipsThisRun,
                Reshuffles = session.Deck.ReshuffleCount,
                FinalGold = session.Gold,
                EscrowAtEnd = session.Deck.Wound.Count,
                DeckAtEnd = session.Deck.Draw.Count + session.Deck.Discard.Count,
                FatalAction = session.LastFatalAction?.Name
            };
        }

        // ---------------------------------------------------------------- phases

        private void StepPhase(FateSession session)
        {
            switch (session.Phase)
            {
                case FateResolutionPhase.AwaitPreFlip:
                    StepPreFlip(session);
                    break;

                case FateResolutionPhase.AwaitBank:
                    StepBank(session);
                    break;

                case FateResolutionPhase.AwaitDoubleDrawChoice:
                    StepDoubleDraw(session);
                    break;

                default:
                    session.ContinueFlip();
                    break;
            }
        }

        private void StepPreFlip(FateSession session)
        {
            FateAction action = session.CurrentAction;
            if (action?.SourceEnemy != null && session.Deck.Pocket.Count > 0)
            {
                double flipAverage = ExpectedForce(session, LawContext.EnemyAction, action.Force);
                CardInstance best = null;
                double bestResult = flipAverage;
                foreach (CardInstance pocketed in session.Deck.Pocket.Cards)
                {
                    double result = ResultIfPlayed(session, pocketed, LawContext.EnemyAction, action.Force);
                    if (result < bestResult)
                    {
                        bestResult = result;
                        best = pocketed;
                    }
                }

                if (best != null && flipAverage - bestResult >= 2)
                {
                    session.PlayPocket(best);
                    return;
                }
            }

            session.ContinueFlip();
        }

        private void StepBank(FateSession session)
        {
            FateAction action = session.CurrentAction;
            CardInstance revealed = session.RevealedCard;
            if (action == null || revealed == null)
            {
                session.DeclineBank();
                return;
            }

            double honored = ResultIfPlayed(session, revealed, action.Context, action.Force);
            double baseline = action.BaseForce + action.DeclaredBonus;
            if (honored < baseline && session.Deck.CanPocket(revealed, session.PocketSlots))
            {
                session.BankRevealed();
            }
            else
            {
                session.DeclineBank();
            }
        }

        private void StepDoubleDraw(FateSession session)
        {
            FateAction action = session.CurrentAction;
            double first = ResultIfPlayed(session, session.RevealedCard, action.Context, action.Force);
            double second = ResultIfPlayed(session, session.AlternateCard, action.Context, action.Force);
            bool enemyOwned = action.SourceEnemy != null;
            bool takeAlternate = enemyOwned ? second < first : second > first;
            session.ChooseRevealed(takeAlternate);
        }

        private double ResultIfPlayed(FateSession session, CardInstance card, LawContext context, double force)
        {
            MetadataEntry entry = _catalog.ForceOf(card);
            if (entry == null)
            {
                return force;
            }

            OddsRow row = OddsCalculator.BuildRow(entry, 1, 1, _catalog.LawFieldFor(context), force, context);
            return row.ResultForce;
        }

        private double ExpectedForce(FateSession session, LawContext context, double baseForce)
        {
            List<OddsRow> rows = OddsCalculator.Table(_catalog, session.Deck, context, baseForce);
            double expected = 0;
            foreach (OddsRow row in rows)
            {
                expected += row.Probability * row.ResultForce;
            }

            return rows.Count > 0 ? expected : baseForce;
        }

        // ---------------------------------------------------------------- combat

        private void StepCombat(RunController run)
        {
            FateSession session = run.Session;
            CombatEngine combat = session.Combat;
            if (combat == null)
            {
                run.CompleteRoom();
                return;
            }

            if (combat.Phase != CombatPhase.PlayerTurn || combat.MainActionTaken)
            {
                combat.TryAdvance();
                return;
            }

            if (session.Grit >= session.Rules.GritSpendCost)
            {
                session.SpendGrit(session.Deck.Wound.Count >= 3 ? GritSpend.Mend : GritSpend.Momentum);
            }

            int lifeLeft = session.Deck.Draw.Count + session.Deck.Discard.Count;
            double totalEnemyHp = 0;
            foreach (CardInstance enemy in combat.EnemiesSnapshot())
            {
                totalEnemyHp += enemy.Fields.GetNumber(_catalog.HpField);
            }

            if (combat.CanFlee && lifeLeft <= 3 && totalEnemyHp >= 6)
            {
                combat.PlayerFlee();
                return;
            }

            CardInstance target = PickTarget(session, combat);
            combat.SelectedEnemy = target;
            double strikeExpected = ExpectedForce(session, LawContext.PlayerOffense,
                session.Rules.StrikeBaseForce + session.NextPlayerActionBonus);
            double targetHp = target != null ? target.Fields.GetNumber(_catalog.HpField)
                + target.Fields.GetNumber(_catalog.BlockField) : 0;
            double incoming = ExpectedIncoming(session, combat);

            if (target != null && strikeExpected >= targetHp)
            {
                combat.PlayerStrike(target);
            }
            else if (incoming - session.PlayerBlock >= 4)
            {
                combat.PlayerGuard();
            }
            else
            {
                combat.PlayerStrike(target);
            }
        }

        private CardInstance PickTarget(FateSession session, CombatEngine combat)
        {
            CardInstance best = null;
            double bestHp = double.MaxValue;
            foreach (CardInstance enemy in combat.EnemiesSnapshot())
            {
                double hp = enemy.Fields.GetNumber(_catalog.HpField);
                if (hp < bestHp)
                {
                    bestHp = hp;
                    best = enemy;
                }
            }

            return best;
        }

        private double ExpectedIncoming(FateSession session, CombatEngine combat)
        {
            double total = 0;
            foreach (CardInstance enemy in combat.EnemiesSnapshot())
            {
                EnemyActionSpec intent = combat.IntentOf(enemy);
                if (intent == null || intent.Kind != EnemyActionKind.Attack)
                {
                    continue;
                }

                double force = combat.EffectiveForceOf(enemy, intent);
                total += intent.FlipsFate
                    ? ExpectedForce(session, LawContext.EnemyAction, force)
                    : force;
            }

            return total;
        }

        // ---------------------------------------------------------------- rooms

        private void ChooseDoor(RunController run)
        {
            FateSession session = run.Session;
            int bestIndex = 0;
            double bestScore = double.MinValue;
            for (int i = 0; i < run.Doors.Count; i++)
            {
                double score = ScoreDoor(run, session, run.Doors[i]);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            run.ChooseDoor(bestIndex);
        }

        private double ScoreDoor(RunController run, FateSession session, RoomDefinition room)
        {
            int wounds = session.Deck.Wound.Count;
            int debts = session.Deck.CountForceIn(session.Deck.Draw, _catalog.Doom);
            switch (room)
            {
                case BossRoomDefinition _:
                    return 100;

                case FightRoomDefinition fight when fight.IsElite:
                    return session.Deck.Draw.Count + session.Deck.Discard.Count >= 12 ? 4.5 : 0.5;

                case FightRoomDefinition _:
                    return 3.0;

                case ChestRoomDefinition chest:
                    return chest.Locked && session.Keys == 0 ? 3.0 : 4.0;

                case ShrineRoomDefinition shrine:
                    switch (shrine.Kind)
                    {
                        case ShrineKind.Stitches: return wounds >= 3 ? 5.0 : 1.5;
                        case ShrineKind.Ash: return debts >= 2 ? 4.5 : 2.0;
                        default: return 2.5;
                    }

                case EventRoomDefinition _:
                    return 3.5;

                case ShopRoomDefinition _:
                    return session.Gold >= 25 ? 4.0 : 1.5;

                default:
                    return 1.0;
            }
        }

        private void StepChest(RunController run)
        {
            if (!run.ChestOpened)
            {
                bool locked = run.CurrentRoom is ChestRoomDefinition chest && chest.Locked;
                run.OpenChest(useKey: locked && run.Session.Keys > 0);
                return;
            }

            run.CompleteRoom();
        }

        private void StepShrine(RunController run)
        {
            FateSession session = run.Session;
            if (run.ShrineExilesRemaining > 0)
            {
                CardInstance debt = FirstOfForce(session, _catalog.Doom);
                if (debt != null && session.Gold >= session.Rules.DoomExileShrinePrice)
                {
                    run.ShrineExile(session.Deck.Draw, debt);
                    return;
                }
            }

            if (run.ForgeGiftsRemaining > 0)
            {
                run.ForgeGift();
                return;
            }

            if (run.ShrineHealsRemaining > 0 && session.Deck.Wound.Count > 0)
            {
                run.ShrineHeal(session.Deck.Wound.Cards[0]);
                return;
            }

            run.CompleteRoom();
        }

        private CardInstance FirstOfForce(FateSession session, MetadataEntry force)
        {
            foreach (CardInstance card in session.Deck.Draw.Cards)
            {
                if (_catalog.ForceOf(card) == force)
                {
                    return card;
                }
            }

            return null;
        }

        private void StepEvent(RunController run)
        {
            EventDefinition active = run.ActiveEvent;
            if (active == null || _eventTakes >= 3)
            {
                run.CompleteRoom();
                return;
            }

            FateSession session = run.Session;
            int bestIndex = -1;
            double bestScore = 0.05;
            for (int i = 0; i < active.Options.Count; i++)
            {
                EventOption option = active.Options[i];
                if (session.Gold < option.GoldCost || session.Keys < option.KeyCost)
                {
                    continue;
                }

                double score = EventPolicy.Score(option, _catalog);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0)
            {
                run.CompleteRoom();
                return;
            }

            _eventTakes++;
            run.TakeEventOption(bestIndex);
        }

        private void StepRest(RunController run)
        {
            FateSession session = run.Session;
            if (!run.RestUsed)
            {
                if (session.Deck.Wound.Count >= 3)
                {
                    run.RestMend();
                }
                else
                {
                    CardInstance upgradeable = FirstUpgradeable(session);
                    if (upgradeable != null)
                    {
                        run.RestSharpen(upgradeable);
                    }
                    else
                    {
                        CardInstance debt = FirstOfForce(session, _catalog.Doom);
                        if (debt == null || session.Gold < session.Rules.DoomCleansePrice
                            || !run.RestCleanse(session.Deck.Draw, debt))
                        {
                            run.RestMend();
                        }
                    }
                }

                return;
            }

            if (run.Step == session.Rules.TrackSteps - 1)
            {
                run.ContinueRestToShop();
            }
            else
            {
                run.CompleteRoom();
            }
        }

        private CardInstance FirstUpgradeable(FateSession session)
        {
            foreach (CardInstance card in session.Deck.Draw.Cards)
            {
                if (_catalog.IsBasicForce(_catalog.ForceOf(card)))
                {
                    return card;
                }
            }

            return null;
        }

        private void StepShop(RunController run)
        {
            FateSession session = run.Session;
            ShopService shop = run.Shop;
            if (shop == null)
            {
                run.CompleteRoom();
                return;
            }

            foreach (ShopItem item in shop.Stock)
            {
                if (item.Sold || session.Gold < item.Price)
                {
                    continue;
                }

                switch (item.Kind)
                {
                    case ShopItemKind.Tonic when session.Deck.Wound.Count >= 3:
                    case ShopItemKind.Relic:
                        if (shop.Buy(item))
                        {
                            return;
                        }

                        break;

                    case ShopItemKind.FateCard when session.Gold >= item.Price + 10:
                    {
                        var force = item.Card != null
                            ? item.Card.GetObject(_catalog.ForceField) as MetadataEntry
                            : null;
                        if ((force == _catalog.Iron || force == _catalog.IronPlus
                             || force == _catalog.Tempest || force == _catalog.Anchor)
                            && shop.Buy(item))
                        {
                            return;
                        }

                        break;
                    }
                }
            }

            run.CompleteRoom();
        }

        private void StepRewards(RunController run)
        {
            if (run.RelicChoices.Count > 0)
            {
                run.PickRelic(0);
                return;
            }

            if (run.CharmReward != null)
            {
                run.TakeCharmReward();
                return;
            }

            run.CompleteRoom();
        }
    }
}
