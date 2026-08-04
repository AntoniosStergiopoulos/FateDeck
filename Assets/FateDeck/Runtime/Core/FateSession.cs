using System;
using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Events;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.Game.Zones;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using AStergio.OmniCard.Runtime.Cards.Triggers;
using FateDeck.Runtime.Combat;
using UnityEngine;

namespace FateDeck.Runtime.Core
{
    /// <summary>
    /// The Fate Deck game context: owns the deck service, gold, statuses, relics, charms and the
    /// interactive flip pipeline. Implements OmniCard's <c>IGameContext</c> so every package atom
    /// (effects, conditions, triggers, trigger bindings) runs against it unchanged.
    /// </summary>
    public sealed class FateSession : IFateSession, IDisposable
    {
        private readonly Dictionary<CardInstance, CardTriggerBinding> _bindings =
            new Dictionary<CardInstance, CardTriggerBinding>();

        private readonly List<CardInstance> _flippedThisAction = new List<CardInstance>();
        private readonly Action<string> _log;
        private double _nextPlayerActionBonus;
        private int _echoQueued;

        public FateSession(FateContentCatalog catalog, int seed, Action<string> log = null)
        {
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            Rules = catalog.Rules;
            _log = log ?? Debug.Log;
            Seed = seed == 0 ? Environment.TickCount : seed;
            Rng = new System.Random(Seed);
            Events = new EventBus();
            Deck = new FateDeckService(Catalog, Rules, this, Rng);
            RelicZone = new CardZone(catalog.Relics);
            CharmZone = new CardZone(catalog.Charms);
            Gold = Rules.StartingGold;
            PocketSlots = Rules.PocketSlots;
        }

        public FateContentCatalog Catalog { get; }

        public FateRulesDefinition Rules { get; }

        public int Seed { get; }

        public System.Random Rng { get; }

        public IEventBus Events { get; }

        public FateDeckService Deck { get; }

        public CardZone RelicZone { get; }

        public CardZone CharmZone { get; }

        public CombatEngine Combat { get; private set; }

        public CardInstance Hero { get; private set; }

        public int Gold { get; private set; }

        public int PocketSlots { get; private set; }

        public double PlayerBlock { get; private set; }

        /// <summary>Burn the player retaliates with while guarding under the Flame law.</summary>
        public int PlayerRetaliateBurn { get; set; }

        public int PlayerBurn { get; private set; }

        public int PlayerWeak { get; private set; }

        public int DoubleDrawCharges { get; private set; }

        /// <summary>Keys open locked chests without gambling on Flame.</summary>
        public int Keys { get; private set; }

        /// <summary>The House's insults harden you: Debt flips bank Grit, spent on Grit actions.</summary>
        public int Grit { get; private set; }

        public bool IsPlayerDead { get; private set; }

        public FateAction CurrentAction { get; private set; }

        public FateResolutionPhase Phase { get; private set; } = FateResolutionPhase.Idle;

        /// <summary>The card revealed by the current flip, while banking or choosing is pending.</summary>
        public CardInstance RevealedCard { get; private set; }

        /// <summary>The second revealed card during a draw-2-choose-1 flip.</summary>
        public CardInstance AlternateCard { get; private set; }

        public int DoomFlipsThisRun { get; private set; }

        public int TotalFlipsThisRun { get; private set; }

        public FateAction LastFatalAction { get; private set; }

        /// <summary>The force whose law most recently resolved - what a Mirror flip repeats.</summary>
        public MetadataEntry LastFlippedForce { get; private set; }

        /// <summary>The pending +Force bonus the next declared player action will consume.</summary>
        public double NextPlayerActionBonus => _nextPlayerActionBonus;

        internal void RestoreRunState(int gold, int keys, int doomFlips, int totalFlips)
        {
            Gold = Math.Max(0, gold);
            Keys = Math.Max(0, keys);
            DoomFlipsThisRun = Math.Max(0, doomFlips);
            TotalFlipsThisRun = Math.Max(0, totalFlips);
        }

        /// <summary>Restores charge-like state a save captured (pocket upgrades, draw-2, bonuses).</summary>
        internal void RestoreCharges(int pocketSlots, int doubleDrawCharges, double nextActionBonus)
        {
            if (pocketSlots > 0)
            {
                PocketSlots = pocketSlots;
            }

            DoubleDrawCharges = Math.Max(0, doubleDrawCharges);
            _nextPlayerActionBonus = Math.Max(0, nextActionBonus);
        }

        public void Log(string message)
        {
            _log?.Invoke(message);
        }

        public void Bark(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                Events.Publish(new DealerBarkEvent(line));
            }
        }

        public void Dispose()
        {
            foreach (CardTriggerBinding binding in _bindings.Values)
            {
                binding?.Dispose();
            }

            _bindings.Clear();
        }

        // ---------------------------------------------------------------- hero & items

        public void SetHero(CardDefinition heroDefinition, bool buildStartingDeck = true)
        {
            Hero = new CardInstance(heroDefinition);
            double slots = Hero.Fields.GetNumber(Catalog.PocketSlotsField);
            if (slots > 0)
            {
                PocketSlots = (int)slots;
            }

            Bind(Hero);
            if (buildStartingDeck)
            {
                var startingDeck = heroDefinition.GetObject(Catalog.StartingDeckField)
                    as AStergio.OmniCard.Runtime.Cards.Game.Decks.DeckDefinition;
                Deck.BuildStartingDeck(startingDeck);
            }
        }

        public void AcquireRelic(CardDefinition relic)
        {
            if (relic == null)
            {
                return;
            }

            var instance = new CardInstance(relic);
            RelicZone.Add(instance);
            ResolveEffectList(instance, instance.Definition.GetEffects(Catalog.EffectsField));
            Bind(instance);
        }

        public bool AcquireCharm(CardDefinition charm)
        {
            if (charm == null || CharmZone.Count >= Rules.MaxCharms)
            {
                return false;
            }

            CharmZone.Add(new CardInstance(charm));
            return true;
        }

        public bool UseCharm(CardInstance charm)
        {
            if (charm == null || !CharmZone.Contains(charm) || Phase != FateResolutionPhase.Idle)
            {
                return false;
            }

            bool isMainAction = charm.Definition.GetBoolean(Catalog.MainActionField);
            if (isMainAction && (Combat == null || !Combat.CanTakeMainAction))
            {
                return false;
            }

            CharmZone.Remove(charm);
            ResolveEffectList(charm, charm.Definition.GetEffects(Catalog.EffectsField));
            if (isMainAction)
            {
                Combat?.ConsumeMainAction();
            }

            return true;
        }

        private void Bind(CardInstance owner)
        {
            if (owner != null && !_bindings.ContainsKey(owner))
            {
                _bindings.Add(owner, new CardTriggerBinding(owner, this));
            }
        }

        internal void Unbind(CardInstance owner)
        {
            if (owner != null && _bindings.TryGetValue(owner, out CardTriggerBinding binding))
            {
                binding.Dispose();
                _bindings.Remove(owner);
            }
        }

        internal void BindEnemy(CardInstance enemy)
        {
            Bind(enemy);
        }

        // ---------------------------------------------------------------- resources & statuses

        public void AddGold(int delta)
        {
            int old = Gold;
            Gold = Math.Max(0, Gold + delta);
            if (Gold != old)
            {
                Events.Publish(new GoldChangedEvent(old, Gold));
            }
        }

        public void AddPocketSlots(int delta)
        {
            PocketSlots = Math.Max(1, PocketSlots + delta);
        }

        public void AddKeys(int delta)
        {
            int old = Keys;
            Keys = Math.Max(0, Keys + delta);
            if (Keys != old)
            {
                Events.Publish(new KeysChangedEvent(old, Keys));
            }
        }

        public void AddGrit(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            int old = Grit;
            Grit = Math.Min(Rules.GritMax, Grit + amount);
            if (Grit != old)
            {
                Events.Publish(new GritChangedEvent(old, Grit));
            }
        }

        /// <summary>
        /// Spends banked Grit on one Grit action. Only between actions (Idle), and only when
        /// the full cost is banked. Returns false when the spend is not possible right now.
        /// </summary>
        public bool SpendGrit(GritSpend spend)
        {
            int cost = Rules.GritSpendCost;
            if (Grit < cost || Phase != FateResolutionPhase.Idle || IsPlayerDead)
            {
                return false;
            }

            switch (spend)
            {
                case GritSpend.Foresight:
                    Scry(2, allowReorder: true);
                    break;

                case GritSpend.Momentum:
                    AddNextPlayerActionBonus(2);
                    break;

                default:
                    if (Deck.Wound.Count == 0)
                    {
                        return false;
                    }

                    if (!RequestWoundChoice(1))
                    {
                        Deck.HealWounds(1);
                    }

                    break;
            }

            int old = Grit;
            Grit -= cost;
            Events.Publish(new GritChangedEvent(old, Grit));
            return true;
        }

        internal void RestoreGrit(int value)
        {
            Grit = Math.Max(0, Math.Min(Rules.GritMax, value));
        }

        /// <summary>
        /// Upgrades a basic fate card in place: the instance is replaced by its + version
        /// in whatever fate zone holds it. Upgrades never change a card's force family.
        /// </summary>
        public bool UpgradeFateCard(CardInstance card)
        {
            MetadataEntry force = Catalog.ForceOf(card);
            MetadataEntry plus = Catalog.PlusVersionOf(force);
            CardDefinition plusCard = plus != null ? Catalog.FateCardFor(plus) : null;
            if (plusCard == null)
            {
                return false;
            }

            foreach (CardZone zone in new[] { Deck.Draw, Deck.Discard, Deck.Wound, Deck.Pocket })
            {
                if (zone.Replace(card, new CardInstance(plusCard)))
                {
                    return true;
                }
            }

            return false;
        }

        public void AddPlayerBlock(double delta)
        {
            PlayerBlock = Math.Max(0, PlayerBlock + delta);
        }

        public void ResetPlayerGuard()
        {
            PlayerBlock = 0;
            PlayerRetaliateBurn = 0;
        }

        public int GetStatus(CardInstance enemy, StatusKind status)
        {
            if (enemy == null)
            {
                return status == StatusKind.Burn ? PlayerBurn : PlayerWeak;
            }

            CardFieldDefinition field = StatusField(status);
            return (int)enemy.Fields.GetNumber(field);
        }

        public void AddStatus(CardInstance enemy, StatusKind status, int stacks)
        {
            SetStatus(enemy, status, GetStatus(enemy, status) + stacks);
        }

        public void SetStatus(CardInstance enemy, StatusKind status, int stacks)
        {
            stacks = Math.Max(0, stacks);
            if (enemy == null)
            {
                if (status == StatusKind.Burn)
                {
                    PlayerBurn = stacks;
                }
                else
                {
                    PlayerWeak = stacks;
                }
            }
            else
            {
                enemy.Fields.SetNumber(StatusField(status), stacks);
            }

            Events.Publish(new StatusChangedEvent(enemy, status, stacks));
        }

        private CardFieldDefinition StatusField(StatusKind status)
        {
            return status == StatusKind.Burn ? Catalog.BurnField : Catalog.WeakField;
        }

        public void AddNextPlayerActionBonus(double delta)
        {
            _nextPlayerActionBonus += delta;
        }

        public void AddDoubleDrawCharges(int count)
        {
            DoubleDrawCharges += count;
        }

        /// <summary>
        /// Installed by the table view. Returns true when the view will resolve the pick
        /// interactively; false (or no handler) falls back to an automatic resolution.
        /// </summary>
        public Func<ZoneChoiceRequest, bool> ChoiceHandler { get; set; }

        public bool RequestWoundChoice(int count)
        {
            return ChoiceHandler?.Invoke(new ZoneChoiceRequest(ZoneChoiceKind.HealWounds, count)) == true;
        }

        public bool RequestZoneChoice(ZoneChoiceKind kind, int count)
        {
            return ChoiceHandler?.Invoke(new ZoneChoiceRequest(kind, count)) == true;
        }

        public void Scry(int count, bool allowReorder)
        {
            List<CardInstance> top = Deck.PeekTop(count);
            if (top.Count > 0)
            {
                Events.Publish(new ScryEvent(top, allowReorder));
            }
        }

        public void MillPlayer(int count, string reason = null)
        {
            if (count <= 0 || IsPlayerDead)
            {
                return;
            }

            int milled = Deck.Mill(count, reason);
            if (milled < count && Deck.IsOutOfCards)
            {
                MarkPlayerDead();
            }
        }

        private void MarkPlayerDead()
        {
            if (IsPlayerDead)
            {
                return;
            }

            IsPlayerDead = true;
            LastFatalAction = CurrentAction;
            Events.Publish(new PlayerDiedEvent(CurrentAction));
        }

        // ---------------------------------------------------------------- combat lifecycle

        public CombatEngine StartCombat(Run.FightRoomDefinition room)
        {
            Combat = new CombatEngine(this, room);
            Combat.Begin();
            return Combat;
        }

        public void EndCombat()
        {
            Combat = null;
        }

        // ---------------------------------------------------------------- the fate pipeline

        /// <summary>Declares an action. Interactive phases follow; the view pumps them.</summary>
        public void BeginAction(FateAction action)
        {
            if (action == null || CurrentAction != null || IsPlayerDead)
            {
                return;
            }

            CurrentAction = action;
            _flippedThisAction.Clear();
            _echoQueued = 0;

            if (action.IsPlayerAction && _nextPlayerActionBonus != 0)
            {
                action.DeclaredBonus = _nextPlayerActionBonus;
                action.Force += _nextPlayerActionBonus;
                _nextPlayerActionBonus = 0;
            }

            if (!action.FlipsFate)
            {
                Commit();
                return;
            }

            SetPhase(FateResolutionPhase.AwaitPreFlip);
        }

        /// <summary>Plays a pocketed card to replace the imminent flip entirely. No card leaves the deck.</summary>
        public bool PlayPocket(CardInstance pocketCard)
        {
            if (Phase != FateResolutionPhase.AwaitPreFlip || CurrentAction == null)
            {
                return false;
            }

            if (!Deck.TakeFromPocket(pocketCard))
            {
                return false;
            }

            FateAction action = CurrentAction;
            action.ReplacedByPocket = true;
            Events.Publish(new PocketPlayedEvent(pocketCard, action));
            ApplyLaw(pocketCard, fromPocket: true);
            DiscardFlipped(pocketCard);
            FinishFlipsOrContinue();
            return true;
        }

        /// <summary>Proceeds from the pre-flip window to the actual flip.</summary>
        public void ContinueFlip()
        {
            if (Phase != FateResolutionPhase.AwaitPreFlip || CurrentAction == null)
            {
                return;
            }

            bool doubleDraw = CurrentAction.IsPlayerAction && DoubleDrawCharges > 0;
            CardInstance first = Deck.TakeTop();
            if (first == null)
            {
                MarkPlayerDead();
                FateAction dead = CurrentAction;
                foreach (CardInstance flipped in _flippedThisAction)
                {
                    DiscardFlipped(flipped);
                }

                _flippedThisAction.Clear();
                CurrentAction = null;
                SetPhase(FateResolutionPhase.Idle);
                Events.Publish(new ActionResolvedEvent(dead));
                return;
            }

            RevealedCard = first;
            if (doubleDraw)
            {
                CardInstance second = Deck.TakeTop();
                if (second != null)
                {
                    DoubleDrawCharges--;
                    AlternateCard = second;
                    SetPhase(FateResolutionPhase.AwaitDoubleDrawChoice);
                    return;
                }
            }

            AfterReveal();
        }

        /// <summary>Picks which of the two revealed cards applies; the other goes to discard.</summary>
        public void ChooseRevealed(bool takeAlternate)
        {
            if (Phase != FateResolutionPhase.AwaitDoubleDrawChoice)
            {
                return;
            }

            CardInstance rejected = takeAlternate ? RevealedCard : AlternateCard;
            if (takeAlternate)
            {
                RevealedCard = AlternateCard;
            }

            AlternateCard = null;
            DiscardFlipped(rejected);
            AfterReveal();
        }

        private void AfterReveal()
        {
            bool canBank = CurrentAction.IsPlayerAction
                && CurrentAction.FlipCount == 0
                && _echoQueued == 0
                && Deck.CanPocket(RevealedCard, PocketSlots);
            if (canBank)
            {
                SetPhase(FateResolutionPhase.AwaitBank);
                return;
            }

            ApplyRevealed();
        }

        /// <summary>Banks the revealed card; the action resolves at base value, unmodified.</summary>
        public void BankRevealed()
        {
            if (Phase != FateResolutionPhase.AwaitBank || RevealedCard == null)
            {
                return;
            }

            CardInstance banked = RevealedCard;
            RevealedCard = null;
            Deck.BankToPocket(banked);
            CurrentAction.Force = CurrentAction.BaseForce + CurrentAction.DeclaredBonus;
            Commit();
        }

        public void DeclineBank()
        {
            if (Phase == FateResolutionPhase.AwaitBank)
            {
                ApplyRevealed();
            }
        }

        private void ApplyRevealed()
        {
            CardInstance card = RevealedCard;
            RevealedCard = null;
            _flippedThisAction.Add(card);
            ApplyLaw(card, fromPocket: false);
            FinishFlipsOrContinue();
        }

        private void ApplyLaw(CardInstance fateCard, bool fromPocket)
        {
            FateAction action = CurrentAction;
            MetadataEntry force = Catalog.ForceOf(fateCard);
            action.FlipCount++;
            TotalFlipsThisRun++;
            if (force == Catalog.Doom)
            {
                DoomFlipsThisRun++;
                AddGrit(Rules.GritPerDebtFlip);
            }

            if (force != null)
            {
                action.AppliedForces.Add(force);
            }

            Events.Publish(new FateFlipEvent(fateCard, force, action, fromPocket));

            if (force == null || action.Negated)
            {
                LastFlippedForce = force ?? LastFlippedForce;
                return;
            }

            CardFieldDefinition lawField = Catalog.LawFieldFor(action.Context);
            IReadOnlyList<CardEffect> law = force.GetEffects(lawField);
            ResolveEffectList(fateCard, law);
            LastFlippedForce = force;
        }

        private void FinishFlipsOrContinue()
        {
            FateAction action = CurrentAction;
            if (_echoQueued > 0
                && !action.Negated
                && action.FlipCount < Rules.EchoMaxFlipsPerAction
                && !IsPlayerDead)
            {
                _echoQueued--;
                SetPhase(FateResolutionPhase.AwaitPreFlip);
                return;
            }

            _echoQueued = 0;
            Commit();
        }

        public void QueueEchoFlip()
        {
            _echoQueued++;
        }

        private void Commit()
        {
            FateAction action = CurrentAction;
            ApplyWeak(action);
            foreach (CardInstance flipped in _flippedThisAction)
            {
                DiscardFlipped(flipped);
            }

            _flippedThisAction.Clear();

            if (!IsPlayerDead)
            {
                CommitByKind(action);
            }

            if (action.RequestsMainActionRefund && !IsPlayerDead
                && Combat != null && Combat.TryRefundMainAction())
            {
                action.MainActionRefunded = true;
            }

            CurrentAction = null;
            SetPhase(FateResolutionPhase.Idle);
            Events.Publish(new ActionResolvedEvent(action));
        }

        private void ApplyWeak(FateAction action)
        {
            if (action.Negated)
            {
                return;
            }

            if (action.IsPlayerAction && PlayerWeak > 0)
            {
                action.Force = Math.Max(0, action.Force - 2);
                SetStatus(null, StatusKind.Weak, PlayerWeak - 1);
            }
            else if (action.SourceEnemy != null && GetStatus(action.SourceEnemy, StatusKind.Weak) > 0)
            {
                action.Force = Math.Max(0, action.Force - 2);
                AddStatus(action.SourceEnemy, StatusKind.Weak, -1);
            }
        }

        private void CommitByKind(FateAction action)
        {
            double force = Math.Max(0, action.Force);
            switch (action.Kind)
            {
                case FateActionKind.Strike:
                    if (!action.Negated && force > 0 && Combat != null)
                    {
                        Combat.DamageEnemy(action.TargetEnemy, force);
                    }

                    break;

                case FateActionKind.Guard:
                    if (!action.Negated)
                    {
                        AddPlayerBlock(force);
                        if (Combat != null && Combat.Enemies.Count >= 2
                            && Rules.OutnumberedGuardDamage > 0)
                        {
                            Combat.DamageEnemy(Combat.SelectedOrFirstEnemy(),
                                Rules.OutnumberedGuardDamage);
                        }
                    }

                    break;

                case FateActionKind.EnemyAttack:
                    if (!action.Negated)
                    {
                        CommitEnemyAttack(action, force);
                    }

                    break;

                case FateActionKind.EnemyBrace:
                    if (!action.Negated && action.SourceEnemy != null)
                    {
                        action.SourceEnemy.Fields.ModifyNumber(Catalog.BlockField, force);
                    }

                    break;

                case FateActionKind.EnemySpecial:
                    if (!action.Negated && action.SourceEnemy != null && Combat != null)
                    {
                        Combat.ResolveSpecial(action);
                    }

                    break;

                case FateActionKind.Loot:
                    bool lockOpen = !action.LockedChest || action.KeyUsed || action.OpensLock;
                    if (!action.Negated && !action.NoLoot && lockOpen)
                    {
                        AddGold((int)force);
                    }

                    break;
            }
        }

        private void CommitEnemyAttack(FateAction action, double force)
        {
            double absorbed = Math.Min(PlayerBlock, force);
            PlayerBlock -= absorbed;
            double remaining = force - absorbed;
            Events.Publish(new PlayerHitEvent(action.SourceEnemy, force, absorbed, (int)remaining));
            if (remaining > 0)
            {
                string attacker = action.SourceEnemy != null ? action.SourceEnemy.DisplayName : "the enemy";
                MillPlayer((int)remaining, $"{attacker}'s {action.Name}");
            }

            if (PlayerRetaliateBurn > 0 && action.SourceEnemy != null)
            {
                AddStatus(action.SourceEnemy, StatusKind.Burn, PlayerRetaliateBurn);
            }
        }

        /// <summary>
        /// Retires a card that just resolved its law. Glass-style forces shatter - they exile
        /// themselves instead of returning to the discard pile.
        /// </summary>
        private void DiscardFlipped(CardInstance card)
        {
            MetadataEntry force = Catalog.ForceOf(card);
            if (force != null && Catalog.ExileAfterFlipField != null
                && force.GetBoolean(Catalog.ExileAfterFlipField))
            {
                Deck.ExileLoose(card);
            }
            else
            {
                Deck.ToDiscard(card);
            }
        }

        private void SetPhase(FateResolutionPhase phase)
        {
            Phase = phase;
            Events.Publish(new ResolutionPhaseChangedEvent(phase, CurrentAction));
        }

        internal void ResolveEffectList(CardInstance source, IReadOnlyList<CardEffect> effects)
        {
            if (source == null || effects == null)
            {
                return;
            }

            var context = new EffectContext(source, this);
            foreach (CardEffect effect in effects)
            {
                effect?.Resolve(context);
            }
        }
    }
}
