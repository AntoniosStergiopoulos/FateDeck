using System;
using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Combat;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Run
{
    public enum RunScreen
    {
        Doors,
        Combat,
        Chest,
        Shrine,
        Event,
        Rest,
        Shop,
        Rewards,
        Dead,
        Victory
    }

    /// <summary>
    /// One run of the track: deals doors, enters rooms, hands combat to the engine, resolves
    /// chests/shrines/events/rest/shops, and collects rewards. Owns the session lifecycle.
    /// </summary>
    public sealed class RunController
    {
        private readonly FateContentCatalog _catalog;
        private EventOption _pendingRitual;
        private CombatEndedEvent _pendingCombatEnd;

        public RunController(FateContentCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public event Action Changed;

        /// <summary>Invoked the moment a new session exists, before content flows - views hook the bus here.</summary>
        public Action<FateSession> SessionStarted;

        public FateSession Session { get; private set; }

        public int Biome { get; private set; } = 1;

        public int Step { get; private set; }

        public RunScreen Screen { get; private set; } = RunScreen.Doors;

        public List<RoomDefinition> Doors { get; } = new List<RoomDefinition>();

        public RoomDefinition CurrentRoom { get; private set; }

        public bool EliteOffered { get; set; }

        public bool ForgeOffered { get; set; }

        public ShopService Shop { get; private set; }

        public EventDefinition ActiveEvent { get; private set; }

        public string LastEventResult { get; private set; }

        public bool ChestOpened { get; private set; }

        public List<CardDefinition> RelicChoices { get; } = new List<CardDefinition>();

        public CardDefinition CharmReward { get; private set; }

        public bool RestUsed { get; private set; }

        public int ForgeGiftsRemaining { get; private set; }

        public int ShrineExilesRemaining { get; private set; }

        public int ShrineHealsRemaining { get; private set; }

        // ---------------------------------------------------------------- lifecycle

        public void StartNewRun(CardDefinition hero, int seed)
        {
            Session?.Dispose();
            Session = new FateSession(_catalog, seed);
            Session.Events.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            Session.Events.Subscribe<ActionResolvedEvent>(OnActionResolved);
            Session.Events.Subscribe<CombatEndedEvent>(OnCombatEnded);
            SessionStarted?.Invoke(Session);
            Session.SetHero(hero != null ? hero : FirstHero());
            Step = 0;
            Biome = 1;
            EliteOffered = false;
            ForgeOffered = false;
            int worth = Session.Deck.Draw.Count;
            Session.Bark($"\"{worth} cards. That is what you are worth today. Play them well.\"");
            NextStep();
        }

        private CardDefinition FirstHero()
        {
            return _catalog.Heroes.Count > 0 ? _catalog.Heroes[0] : null;
        }

        /// <summary>Resumes a saved run at the doors of the saved step. False when no valid save exists.</summary>
        public bool TryContinueRun()
        {
            if (!FateRunSave.TryLoad(out FateRunSave.SaveData data))
            {
                return false;
            }

            Session?.Dispose();
            Session = new FateSession(_catalog, data.ResumeSeed);
            Session.Events.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            Session.Events.Subscribe<ActionResolvedEvent>(OnActionResolved);
            Session.Events.Subscribe<CombatEndedEvent>(OnCombatEnded);
            SessionStarted?.Invoke(Session);
            Session.SetHero(ResolveCard(data.HeroId) ?? FirstHero(), buildStartingDeck: false);

            foreach (string relicId in data.RelicIds)
            {
                Session.AcquireRelic(ResolveCard(relicId));
            }

            foreach (string charmId in data.CharmIds)
            {
                Session.AcquireCharm(ResolveCard(charmId));
            }

            FateRunSave.RestoreZones(Session, data, ResolveCard);
            Session.Deck.TaxModifier = data.TaxModifier;
            Session.Deck.ExtraTaxNextReshuffle = data.ExtraTaxNextReshuffle;
            Session.Deck.RestoreReshuffleCount(data.ReshuffleCount);
            Session.RestoreRunState(data.Gold, data.Keys, data.DoomFlips, data.TotalFlips);
            Session.RestoreCharges(data.PocketSlots, data.DoubleDrawCharges, data.NextActionBonus);

            Biome = data.Biome;
            EliteOffered = data.EliteOffered;
            ForgeOffered = data.ForgeOffered;
            Step = data.Step - 1;
            Session.Bark("\"Back for the rest of yourself? The table kept your seat warm.\"");
            NextStep();
            return true;
        }

        private CardDefinition ResolveCard(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            CardDefinition found = FindById(_catalog.FateCards, id);
            if (found == null)
            {
                found = FindById(_catalog.Heroes, id);
            }

            if (found == null)
            {
                found = FindById(_catalog.CharmPool, id);
            }

            if (found == null)
            {
                found = FindById(_catalog.RelicPool, id);
            }

            return found;
        }

        private static CardDefinition FindById(List<CardDefinition> pool, string id)
        {
            foreach (CardDefinition card in pool)
            {
                if (card != null && card.Id.Value == id)
                {
                    return card;
                }
            }

            return null;
        }

        private void OnPlayerDied(PlayerDiedEvent _)
        {
            Screen = RunScreen.Dead;
            FateRunSave.Delete();
            RaiseChanged();
        }

        private void RaiseChanged()
        {
            Changed?.Invoke();
        }

        // ---------------------------------------------------------------- track

        private void NextStep()
        {
            Step++;
            CurrentRoom = null;
            ChestOpened = false;
            LastEventResult = null;

            if (Step >= _catalog.Rules.TrackSteps)
            {
                EnterRoom(_catalog.Biome1Boss);
                return;
            }

            if (Step == _catalog.Rules.TrackSteps - 1)
            {
                RestUsed = false;
                Screen = RunScreen.Rest;
                Session.Bark("\"Rest up. The house always lets you sleep before it collects.\"");
                RaiseChanged();
                return;
            }

            DealDoors();
        }

        private void DealDoors()
        {
            Doors.Clear();
            bool eliteOffered = EliteOffered;
            RoomDefinition forced = null;

            if (Step == 1 && _catalog.Biome1Opening != null)
            {
                Doors.Add(_catalog.Biome1Opening);
                Screen = RunScreen.Doors;
                RaiseChanged();
                return;
            }

            if (!ForgeOffered && Step == 4 && _catalog.ForgeShrine != null)
            {
                forced = _catalog.ForgeShrine;
                ForgeOffered = true;
            }

            Doors.AddRange(DoorDealer.Deal(_catalog.Biome1Rooms, _catalog.ElitePool(), Step,
                _catalog.Rules.DoorsPerStep, Session.Rng, ref eliteOffered, forced));
            EliteOffered = eliteOffered;
            Screen = RunScreen.Doors;
            FateRunSave.Save(this);
            RaiseChanged();
        }

        public void ChooseDoor(int index)
        {
            if (Screen != RunScreen.Doors || index < 0 || index >= Doors.Count)
            {
                return;
            }

            EnterRoom(Doors[index]);
        }

        private void EnterRoom(RoomDefinition room)
        {
            CurrentRoom = room;
            switch (room)
            {
                case FightRoomDefinition fight:
                    Screen = RunScreen.Combat;
                    Session.StartCombat(fight);
                    break;

                case ChestRoomDefinition _:
                    Screen = RunScreen.Chest;
                    break;

                case ShrineRoomDefinition shrine:
                    Screen = RunScreen.Shrine;
                    ForgeGiftsRemaining = shrine.Kind == ShrineKind.Forge ? 2 : 0;
                    ShrineExilesRemaining = shrine.Kind == ShrineKind.Ash ? 1 : 0;
                    ShrineHealsRemaining = shrine.Kind == ShrineKind.Stitches ? 4 : 0;
                    break;

                case EventRoomDefinition eventRoom:
                    Screen = RunScreen.Event;
                    ActiveEvent = eventRoom.Event;
                    break;

                case RestRoomDefinition _:
                    Screen = RunScreen.Rest;
                    RestUsed = false;
                    break;

                case ShopRoomDefinition shopRoom:
                    Screen = RunScreen.Shop;
                    Shop = new ShopService(Session, shopRoom.MiniShop);
                    break;
            }

            RaiseChanged();
        }

        /// <summary>Leaves the current room and advances the track (the view's Continue button).</summary>
        public void CompleteRoom()
        {
            Session.Events.Publish(new RoomEndedEvent());
            ActiveEvent = null;
            Shop = null;
            RelicChoices.Clear();
            CharmReward = null;

            if (Session.IsPlayerDead)
            {
                Screen = RunScreen.Dead;
                RaiseChanged();
                return;
            }

            if (Screen == RunScreen.Victory)
            {
                RaiseChanged();
                return;
            }

            NextStep();
        }

        /// <summary>Step 8 flow: after resting, the guaranteed shop opens.</summary>
        public void ContinueRestToShop()
        {
            if (Step != _catalog.Rules.TrackSteps - 1 || Screen != RunScreen.Rest)
            {
                return;
            }

            Screen = RunScreen.Shop;
            Shop = new ShopService(Session, miniShop: false);
            RaiseChanged();
        }

        // ---------------------------------------------------------------- combat outcome

        private void OnCombatEnded(CombatEndedEvent ended)
        {
            if (Session.CurrentAction != null)
            {
                _pendingCombatEnd = ended;
                return;
            }

            HandleCombatEnded(ended);
        }

        private void HandleCombatEnded(CombatEndedEvent ended)
        {
            var room = CurrentRoom as FightRoomDefinition;
            CombatEngine combat = Session.Combat;
            if (room == null || combat == null)
            {
                return;
            }

            if (Session.IsPlayerDead)
            {
                Session.EndCombat();
                return;
            }

            if (!ended.Victory)
            {
                Session.EndCombat();
                if (!Session.IsPlayerDead && combat.Fled)
                {
                    Session.Bark("\"Sensible. The door remembers you, though.\"");
                    CompleteRoom();
                }

                return;
            }

            if (room.IsBoss)
            {
                Session.EndCombat();
                Screen = RunScreen.Victory;
                FateRunSave.Delete();
                Session.Bark("\"The Collector, out-collected. The Mire smells your luck already.\"");
                RaiseChanged();
                return;
            }

            bool relicReward = room.IsElite;
            if (relicReward)
            {
                RollRelicChoices();
            }

            if (room.CharmDropChance > 0 && Session.Rng.NextDouble() < room.CharmDropChance)
            {
                CharmReward = RollCharm();
            }

            Session.EndCombat();
            if (RelicChoices.Count > 0 || CharmReward != null)
            {
                Screen = RunScreen.Rewards;
                RaiseChanged();
            }
            else
            {
                CompleteRoom();
            }
        }

        private void RollRelicChoices()
        {
            RelicChoices.Clear();
            var pool = new List<CardDefinition>(_catalog.RelicPool);
            foreach (CardInstance owned in Session.RelicZone.Cards)
            {
                pool.Remove(owned.Definition);
            }

            for (int i = 0; i < 3 && pool.Count > 0; i++)
            {
                int index = Session.Rng.Next(pool.Count);
                RelicChoices.Add(pool[index]);
                pool.RemoveAt(index);
            }
        }

        private CardDefinition RollCharm()
        {
            if (_catalog.CharmPool.Count == 0)
            {
                return null;
            }

            return _catalog.CharmPool[Session.Rng.Next(_catalog.CharmPool.Count)];
        }

        public void PickRelic(int index)
        {
            if (Screen != RunScreen.Rewards || index < 0 || index >= RelicChoices.Count)
            {
                return;
            }

            Session.AcquireRelic(RelicChoices[index]);
            RelicChoices.Clear();
            RaiseChanged();
        }

        public void TakeCharmReward()
        {
            if (CharmReward != null && Session.AcquireCharm(CharmReward))
            {
                CharmReward = null;
                RaiseChanged();
            }
        }

        // ---------------------------------------------------------------- chest

        public void OpenChest(bool useKey)
        {
            if (Screen != RunScreen.Chest || ChestOpened || !(CurrentRoom is ChestRoomDefinition chest))
            {
                return;
            }

            bool locked = chest.Locked;
            bool keyUsed = false;
            if (locked && useKey && Session.Keys > 0)
            {
                Session.AddKeys(-1);
                keyUsed = true;
            }

            double baseGold = locked ? Session.Rules.LockedChestBaseGold : Session.Rules.ChestBaseGold;
            ChestOpened = true;
            Session.BeginAction(new FateAction(FateActionKind.Loot, "Chest", baseGold, true)
            {
                LockedChest = locked,
                KeyUsed = keyUsed
            });
            RaiseChanged();
        }

        // ---------------------------------------------------------------- shrine services

        public bool ShrineExile(AStergio.OmniCard.Runtime.Cards.Game.Zones.CardZone zone, CardInstance card)
        {
            if (ShrineExilesRemaining <= 0)
            {
                return false;
            }

            MetadataEntry force = _catalog.ForceOf(card);
            if (force == _catalog.Doom)
            {
                if (Session.Gold < Session.Rules.DoomExileShrinePrice)
                {
                    return false;
                }

                Session.AddGold(-Session.Rules.DoomExileShrinePrice);
            }

            if (!Session.Deck.ExileCard(zone, card))
            {
                return false;
            }

            ShrineExilesRemaining--;
            RaiseChanged();
            return true;
        }

        public void ForgeGift()
        {
            if (ForgeGiftsRemaining <= 0)
            {
                return;
            }

            CardDefinition flame = _catalog.FateCardFor(_catalog.Flame);
            Session.Deck.AddCard(flame, Session.Deck.Draw, randomPosition: true);
            ForgeGiftsRemaining--;
            if (ForgeGiftsRemaining == 0)
            {
                Session.Bark("\"Flame. It burns whoever the action was pointing at. Aim accordingly.\"");
            }

            RaiseChanged();
        }

        public bool ShrineHeal(CardInstance wound)
        {
            if (ShrineHealsRemaining <= 0 || !Session.Deck.HealWound(wound))
            {
                return false;
            }

            ShrineHealsRemaining--;
            RaiseChanged();
            return true;
        }

        // ---------------------------------------------------------------- rest services

        public bool RestMend()
        {
            if (RestUsed)
            {
                return false;
            }

            RestUsed = true;
            if (!Session.RequestWoundChoice(Session.Rules.RestHeal))
            {
                Session.Deck.HealWounds(Session.Rules.RestHeal);
            }

            RaiseChanged();
            return true;
        }

        public bool RestSharpen(CardInstance card)
        {
            if (RestUsed || !Session.UpgradeFateCard(card))
            {
                return false;
            }

            RestUsed = true;
            RaiseChanged();
            return true;
        }

        public bool RestCleanse(AStergio.OmniCard.Runtime.Cards.Game.Zones.CardZone zone, CardInstance card)
        {
            if (RestUsed)
            {
                return false;
            }

            MetadataEntry force = _catalog.ForceOf(card);
            if (force == _catalog.Doom)
            {
                if (Session.Gold < Session.Rules.DoomCleansePrice)
                {
                    return false;
                }

                Session.AddGold(-Session.Rules.DoomCleansePrice);
            }

            if (!Session.Deck.ExileCard(zone, card))
            {
                return false;
            }

            RestUsed = true;
            RaiseChanged();
            return true;
        }

        // ---------------------------------------------------------------- events

        public void TakeEventOption(int index)
        {
            if (Screen != RunScreen.Event || ActiveEvent == null
                || index < 0 || index >= ActiveEvent.Options.Count)
            {
                return;
            }

            EventOption option = ActiveEvent.Options[index];
            if (Session.Gold < option.GoldCost)
            {
                return;
            }

            Session.AddGold(-option.GoldCost);
            if (Session.Hero != null)
            {
                Session.ResolveEffectList(Session.Hero, option.Effects);
            }

            LastEventResult = option.ResultText;

            if (option.FlipsFate)
            {
                _pendingRitual = option;
                Session.BeginAction(new FateAction(FateActionKind.Ritual, ActiveEvent.name, 0, true));
                RaiseChanged();
                return;
            }

            if (option.ClosesEvent && !option.Repeatable)
            {
                ActiveEvent = null;
            }

            RaiseChanged();
        }

        private void OnActionResolved(ActionResolvedEvent resolved)
        {
            if (_pendingCombatEnd != null)
            {
                CombatEndedEvent ended = _pendingCombatEnd;
                _pendingCombatEnd = null;
                HandleCombatEnded(ended);
            }

            if (Session.IsPlayerDead)
            {
                _pendingRitual = null;
                RaiseChanged();
                return;
            }

            if (resolved.Action.Kind != FateActionKind.Ritual || _pendingRitual == null)
            {
                RaiseChanged();
                return;
            }

            EventOption option = _pendingRitual;
            _pendingRitual = null;
            MetadataEntry flipped = resolved.Action.AppliedForces.Count > 0
                ? resolved.Action.AppliedForces[0]
                : null;

            RitualOutcome outcome = FindOutcome(option, flipped);
            if (outcome != null)
            {
                LastEventResult = outcome.ResultText;
                if (Session.Hero != null)
                {
                    Session.ResolveEffectList(Session.Hero, outcome.Effects);
                }

                if (outcome.ClosesEvent)
                {
                    ActiveEvent = null;
                }
            }
            else if (!option.Repeatable && option.ClosesEvent)
            {
                ActiveEvent = null;
            }

            RaiseChanged();
        }

        private static RitualOutcome FindOutcome(EventOption option, MetadataEntry flipped)
        {
            foreach (RitualOutcome outcome in option.RitualOutcomes)
            {
                if (outcome.Force == flipped)
                {
                    return outcome;
                }
            }

            return null;
        }
    }
}
