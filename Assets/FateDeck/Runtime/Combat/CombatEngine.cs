using System;
using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Game.Zones;
using AStergio.OmniCard.Runtime.Cards.Instances;
using FateDeck.Runtime.Core;
using FateDeck.Runtime.Run;

namespace FateDeck.Runtime.Combat
{
    public enum CombatPhase
    {
        PlayerTurn,
        EnemyPhase,
        Ended
    }

    /// <summary>
    /// Drives one fight: the player's single Main Action, the scripted enemy phase (one
    /// interactive fate action at a time), statuses, bounties, and win/loss. The view pumps
    /// <see cref="TryAdvance"/> whenever the fate pipeline returns to idle.
    /// </summary>
    public sealed class CombatEngine
    {
        private readonly FateSession _session;
        private readonly HashSet<object> _onceClaims = new HashSet<object>();
        private List<CardInstance> _roundSnapshot = new List<CardInstance>();
        private int _actingIndex;
        private int _actionsDoneForCurrent;

        public CombatEngine(FateSession session, FightRoomDefinition room)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            Room = room;
            Enemies = new CardZone(session.Catalog.Enemies);
            Slain = new CardZone(session.Catalog.Slain);
            Mantle = new CardZone(session.Catalog.Mantle);
        }

        public FightRoomDefinition Room { get; }

        public CardZone Enemies { get; }

        public CardZone Slain { get; }

        public CardZone Mantle { get; }

        public CombatPhase Phase { get; private set; } = CombatPhase.PlayerTurn;

        public int Round { get; private set; }

        public bool MainActionTaken { get; private set; }

        public bool Victory { get; private set; }

        public bool Fled { get; private set; }

        public int DeckSizeAtStart { get; private set; }

        public CardInstance SelectedEnemy { get; set; }

        public bool CanTakeMainAction =>
            Phase == CombatPhase.PlayerTurn
            && !MainActionTaken
            && _session.Phase == FateResolutionPhase.Idle
            && !_session.IsPlayerDead;

        public bool CanFlee => Room != null && !Room.IsElite && !Room.IsBoss && CanTakeMainAction;

        public void Begin()
        {
            SpawnEncounter();
            DeckSizeAtStart = _session.Deck.Draw.Count + _session.Deck.Discard.Count;
            Round = 1;
            _session.Events.Publish(new CombatStartedEvent());
            if (Room != null && Room.RiggedTopForce != null)
            {
                _session.Deck.MoveForceToTop(Room.RiggedTopForce);
            }

            StartPlayerTurn();
        }

        private void SpawnEncounter()
        {
            if (Room?.Encounter == null)
            {
                return;
            }

            foreach (AStergio.OmniCard.Runtime.Cards.Game.Decks.DeckEntry entry in Room.Encounter.Cards)
            {
                if (entry?.Card == null)
                {
                    continue;
                }

                for (int i = 0; i < entry.Count; i++)
                {
                    var enemy = new CardInstance(entry.Card);
                    Enemies.Add(enemy);
                    _session.BindEnemy(enemy);
                }
            }

            SelectedEnemy = Enemies.Count > 0 ? Enemies.Cards[0] : null;
        }

        // ---------------------------------------------------------------- player side

        public bool PlayerStrike(CardInstance target)
        {
            if (!CanTakeMainAction)
            {
                return false;
            }

            target = target != null && Enemies.Contains(target) ? target : SelectedOrFirstEnemy();
            if (target == null)
            {
                return false;
            }

            SelectedEnemy = target;
            MainActionTaken = true;
            var action = new FateAction(FateActionKind.Strike, "Strike", _session.Rules.StrikeBaseForce, true)
            {
                TargetEnemy = target
            };
            _session.BeginAction(action);
            return true;
        }

        public bool PlayerGuard()
        {
            if (!CanTakeMainAction)
            {
                return false;
            }

            MainActionTaken = true;
            _session.BeginAction(new FateAction(FateActionKind.Guard, "Guard", _session.Rules.GuardBaseForce, true));
            return true;
        }

        public bool PlayerFlee()
        {
            if (!CanFlee)
            {
                return false;
            }

            MainActionTaken = true;
            Fled = true;
            _session.MillPlayer(_session.Rules.FleeMill);
            End(victory: false);
            return true;
        }

        public void ConsumeMainAction()
        {
            MainActionTaken = true;
        }

        /// <summary>Once-per-combat gate used by conditions; true the first time a key claims it.</summary>
        public bool ClaimOnce(object key)
        {
            return key != null && _onceClaims.Add(key);
        }

        // ---------------------------------------------------------------- pump

        /// <summary>
        /// Advances combat by one beat when the fate pipeline is idle: into the enemy phase after
        /// the Main Action, one enemy action per call, then end-of-round back to the player.
        /// Returns false when there is nothing to advance (waiting on player input).
        /// </summary>
        public bool TryAdvance()
        {
            if (Phase == CombatPhase.Ended || _session.IsPlayerDead
                || _session.Phase != FateResolutionPhase.Idle)
            {
                return false;
            }

            if (Phase == CombatPhase.PlayerTurn)
            {
                if (!MainActionTaken)
                {
                    return false;
                }

                StartEnemyPhase();
                return true;
            }

            ContinueEnemyPhase();
            return true;
        }

        private void StartEnemyPhase()
        {
            Phase = CombatPhase.EnemyPhase;
            _roundSnapshot = new List<CardInstance>(Enemies.Cards);
            _actingIndex = 0;
            _actionsDoneForCurrent = 0;
            ContinueEnemyPhase();
        }

        private void ContinueEnemyPhase()
        {
            if (Phase != CombatPhase.EnemyPhase)
            {
                return;
            }

            while (_actingIndex < _roundSnapshot.Count)
            {
                CardInstance enemy = _roundSnapshot[_actingIndex];
                if (!Enemies.Contains(enemy))
                {
                    _actingIndex++;
                    _actionsDoneForCurrent = 0;
                    continue;
                }

                if (_actionsDoneForCurrent == 0)
                {
                    enemy.Fields.SetNumber(_session.Catalog.BlockField, 0);
                }

                int actionsPerRound = Math.Max(1, (int)enemy.Fields.GetNumber(_session.Catalog.ActionsPerRoundField));
                if (_actionsDoneForCurrent >= actionsPerRound)
                {
                    _actingIndex++;
                    _actionsDoneForCurrent = 0;
                    continue;
                }

                _actionsDoneForCurrent++;
                DeclareEnemyAction(enemy);
                return;
            }

            EndRound();
        }

        /// <summary>The force this enemy's step will actually declare: base + Howl bonus + Mantle bonus.</summary>
        public double EffectiveForceOf(CardInstance enemy, EnemyActionSpec spec)
        {
            if (enemy == null || spec == null)
            {
                return 0;
            }

            double force = spec.Force + enemy.Fields.GetNumber(_session.Catalog.ForceBonusField);
            double mantlePer = enemy.Fields.GetNumber(_session.Catalog.MantleBonusPerField);
            if (mantlePer > 0 && spec.Kind == EnemyActionKind.Attack)
            {
                force += Math.Floor(Mantle.Count / mantlePer);
            }

            return force;
        }

        private void DeclareEnemyAction(CardInstance enemy)
        {
            EnemyActionSpec spec = IntentOf(enemy);
            AdvancePattern(enemy);
            if (spec == null)
            {
                return;
            }

            double force = EffectiveForceOf(enemy, spec);
            FateActionKind kind = spec.Kind == EnemyActionKind.Attack
                ? FateActionKind.EnemyAttack
                : spec.Kind == EnemyActionKind.Brace
                    ? FateActionKind.EnemyBrace
                    : FateActionKind.EnemySpecial;

            var action = new FateAction(kind, spec.Name, force, spec.FlipsFate)
            {
                SourceEnemy = enemy,
                SpecialEffects = spec.Effects
            };
            _session.BeginAction(action);
        }

        public void ResolveSpecial(FateAction action)
        {
            if (action?.SpecialEffects == null || action.SourceEnemy == null)
            {
                return;
            }

            _session.ResolveEffectList(action.SourceEnemy, action.SpecialEffects);
        }

        private void EndRound()
        {
            TickBurns();
            if (Phase == CombatPhase.Ended || _session.IsPlayerDead)
            {
                return;
            }

            Round++;
            StartPlayerTurn();
        }

        private void StartPlayerTurn()
        {
            Phase = CombatPhase.PlayerTurn;
            MainActionTaken = false;
            _session.ResetPlayerGuard();
            _session.Events.Publish(new PlayerTurnStartedEvent(Round));
        }

        private void TickBurns()
        {
            int playerBurn = _session.GetStatus(null, StatusKind.Burn);
            if (playerBurn > 0)
            {
                _session.MillPlayer(playerBurn);
                _session.SetStatus(null, StatusKind.Burn, playerBurn - 1);
            }

            foreach (CardInstance enemy in EnemiesSnapshot())
            {
                int burn = _session.GetStatus(enemy, StatusKind.Burn);
                if (burn > 0)
                {
                    DamageEnemyDirect(enemy, burn);
                    if (Enemies.Contains(enemy))
                    {
                        _session.SetStatus(enemy, StatusKind.Burn, burn - 1);
                    }
                }
            }
        }

        // ---------------------------------------------------------------- damage & death

        /// <summary>Damage after the enemy's Block; kills collect bounty plus everything pocketed.</summary>
        public void DamageEnemy(CardInstance enemy, double amount)
        {
            if (enemy == null || !Enemies.Contains(enemy) || amount <= 0)
            {
                return;
            }

            double block = enemy.Fields.GetNumber(_session.Catalog.BlockField);
            double absorbed = Math.Min(block, amount);
            if (absorbed > 0)
            {
                enemy.Fields.SetNumber(_session.Catalog.BlockField, block - absorbed);
            }

            DamageEnemyDirect(enemy, amount - absorbed, absorbed);
        }

        /// <summary>Damage that ignores Block (Burn ticks).</summary>
        public void DamageEnemyDirect(CardInstance enemy, double amount)
        {
            DamageEnemyDirect(enemy, amount, absorbed: 0);
        }

        private void DamageEnemyDirect(CardInstance enemy, double amount, double absorbed)
        {
            if (enemy == null || !Enemies.Contains(enemy) || amount <= 0)
            {
                if (enemy != null && absorbed > 0 && Enemies.Contains(enemy))
                {
                    _session.Events.Publish(new EnemyDamagedEvent(enemy, 0, absorbed,
                        enemy.Fields.GetNumber(_session.Catalog.HpField)));
                }

                return;
            }

            double hp = enemy.Fields.GetNumber(_session.Catalog.HpField) - amount;
            enemy.Fields.SetNumber(_session.Catalog.HpField, Math.Max(0, hp));
            _session.Events.Publish(new EnemyDamagedEvent(enemy, amount, absorbed, Math.Max(0, hp)));
            if (hp <= 0)
            {
                Kill(enemy);
            }
        }

        private void Kill(CardInstance enemy)
        {
            int bounty = (int)(enemy.Fields.GetNumber(_session.Catalog.BountyField)
                + enemy.Fields.GetNumber(_session.Catalog.PocketedGoldField));
            Enemies.Remove(enemy);
            Slain.Add(enemy);
            _session.Events.Publish(new EnemyDiedEvent(enemy, bounty));
            _session.Unbind(enemy);
            if (bounty > 0)
            {
                _session.AddGold(bounty);
            }

            if (SelectedEnemy == enemy)
            {
                SelectedEnemy = Enemies.Count > 0 ? Enemies.Cards[0] : null;
            }

            if (Enemies.Count == 0 && Phase != CombatPhase.Ended)
            {
                End(victory: true);
            }
        }

        private void End(bool victory)
        {
            Phase = CombatPhase.Ended;
            Victory = victory;
            _session.Events.Publish(new CombatEndedEvent(victory));
        }

        // ---------------------------------------------------------------- queries

        public List<CardInstance> EnemiesSnapshot()
        {
            return new List<CardInstance>(Enemies.Cards);
        }

        public CardInstance SelectedOrFirstEnemy()
        {
            if (SelectedEnemy != null && Enemies.Contains(SelectedEnemy))
            {
                return SelectedEnemy;
            }

            return Enemies.Count > 0 ? Enemies.Cards[0] : null;
        }

        /// <summary>The enemy's next pattern step - always visible one turn ahead.</summary>
        public EnemyActionSpec IntentOf(CardInstance enemy)
        {
            List<EnemyActionSpec> steps = PatternOf(enemy);
            if (steps == null || steps.Count == 0)
            {
                return null;
            }

            int index = (int)enemy.Fields.GetNumber(_session.Catalog.PatternIndexField) % steps.Count;
            return steps[index];
        }

        private List<EnemyActionSpec> PatternOf(CardInstance enemy)
        {
            var value = enemy.Definition.GetValue(_session.Catalog.PatternField) as EnemyPatternFieldValue;
            return value?.Steps;
        }

        private void AdvancePattern(CardInstance enemy)
        {
            List<EnemyActionSpec> steps = PatternOf(enemy);
            if (steps == null || steps.Count == 0)
            {
                return;
            }

            int index = (int)enemy.Fields.GetNumber(_session.Catalog.PatternIndexField);
            enemy.Fields.SetNumber(_session.Catalog.PatternIndexField, (index + 1) % steps.Count);
        }
    }
}
