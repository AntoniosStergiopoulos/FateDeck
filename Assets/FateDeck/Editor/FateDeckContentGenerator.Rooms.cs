using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using FateDeck.Runtime.Effects.Gameplay;
using FateDeck.Runtime.Run;

namespace FateDeck.Editor
{
    public static partial class FateDeckContentGenerator
    {
        private sealed class Rooms
        {
            public List<RoomDefinition> Pool = new List<RoomDefinition>();
            public FightRoomDefinition Opening;
            public FightRoomDefinition Elite;
            public BossRoomDefinition Boss;
            public ShrineRoomDefinition Forge;
        }

        private static Rooms CreateRooms(Fields fields, Forces forces, List<CardDefinition> fateCards, Items items)
        {
            var rooms = new Rooms();

            CardDefinition imp = TinderImp(fields);
            CardDefinition scrapling = Scrapling(fields);
            CardDefinition rat = CardsharpRat(fields);
            CardDefinition hound = TinderHound(fields);
            CardDefinition cherub = WaxCherub(fields);
            CardDefinition toll = TollCollector(fields);
            CardDefinition collector = TheCollector(fields);

            rooms.Opening = GetOrCreate<FightRoomDefinition>("Room - Opening Imp", room =>
            {
                room.Encounter = Deck("Encounter - Tinder Imp", (imp, 1));
                room.RiggedTopForce = forces.Iron;
                room.Blurb = "A single Tinder Imp guards the mailroom door. The Dealer has stacked "
                    + "your first card - Iron waits on top.";
            }, "Rooms");

            AddFight(rooms, "Room - Scrapling", Deck("Encounter - Scrapling", (scrapling, 1)),
                "A Scrapling drags its shield of dead letters.");
            AddFight(rooms, "Room - Cardsharp Rat", Deck("Encounter - Cardsharp Rat", (rat, 1)),
                "Something small is shuffling cards in the dark. Your cards.");
            AddFight(rooms, "Room - Tinder Hound", Deck("Encounter - Tinder Hound", (hound, 1)),
                "A hound of matchsticks. It gets angrier the longer it lives.");
            AddFight(rooms, "Room - Wax Cherubs", Deck("Encounter - Wax Cherubs", (cherub, 2)),
                "Two wax cherubs. Each one burns your reshuffle clock.");
            AddFight(rooms, "Room - Imp and Scrapling", Deck("Encounter - Imp and Scrapling", (imp, 1), (scrapling, 1)),
                "A creditor's escort: one imp, one scrapling.");

            rooms.Elite = GetOrCreate<FightRoomDefinition>("Room - The Toll Collector", room =>
            {
                room.Encounter = Deck("Encounter - Toll Collector", (toll, 1));
                room.IsElite = true;
                room.Blurb = "The Toll Collector waits with an open ledger. Its tithe cannot be blocked. "
                    + "Decline the door and its relic is gone forever.";
            }, "Rooms");

            rooms.Boss = GetOrCreate<BossRoomDefinition>("Room - THE COLLECTOR", room =>
            {
                room.Encounter = Deck("Encounter - The Collector", (collector, 1));
                room.Blurb = "The vault at the bottom of the mailroom. It reads only your draw pile - "
                    + "what sits in your discard, pocket and wounds is invisible to it.";
            }, "Rooms");

            rooms.Pool.Add(GetOrCreate<ChestRoomDefinition>("Room - Chest", room =>
            {
                room.Blurb = "A chest, unlatched. Every chest is a knowing bet against your own composition.";
            }, "Rooms"));
            rooms.Pool.Add(GetOrCreate<ChestRoomDefinition>("Room - Locked Chest", room =>
            {
                room.Locked = true;
                room.Blurb = "A locked chest, heavier. A Key opens it politely; Flame opens it rudely.";
            }, "Rooms"));

            rooms.Pool.Add(GetOrCreate<ShrineRoomDefinition>("Room - Shrine of Ash", room =>
            {
                room.Kind = ShrineKind.Ash;
                room.Blurb = "Exile one card from your fate, free. Doom clings - it costs gold to burn.";
            }, "Rooms"));
            rooms.Pool.Add(GetOrCreate<ShrineRoomDefinition>("Room - Shrine of Stitches", room =>
            {
                room.Kind = ShrineKind.Stitches;
                room.Blurb = "Four wounds, stitched back into the deck of you.";
            }, "Rooms"));

            rooms.Forge = GetOrCreate<ShrineRoomDefinition>("Room - Shrine of the Forge", room =>
            {
                room.Kind = ShrineKind.Forge;
                room.Blurb = "The Forge hums. It offers Flame - a law that burns whoever the action targets.";
            }, "Rooms");

            rooms.Pool.Add(GetOrCreate<ShopRoomDefinition>("Room - Mini-Shop", room =>
            {
                room.MiniShop = true;
                room.Blurb = "A folding table of contraband odds.";
            }, "Rooms"));

            CreateEvents(fields, forces, fateCards, rooms);
            return rooms;
        }

        private static void AddFight(Rooms rooms, string name,
            AStergio.OmniCard.Runtime.Cards.Game.Decks.DeckDefinition encounter, string blurb)
        {
            rooms.Pool.Add(GetOrCreate<FightRoomDefinition>(name, room =>
            {
                room.Encounter = encounter;
                room.Blurb = blurb;
            }, "Rooms"));
        }

        private static void CreateEvents(Fields fields, Forces forces, List<CardDefinition> fateCards, Rooms rooms)
        {
            EventDefinition beggar = GetOrCreate<EventDefinition>("The Beggar of Odds", ev =>
            {
                ev.Intro = "A beggar sits cross-legged on a pile of losing tickets. \"Ten gold,\" he says, "
                    + "\"and I eat one of your Dooms. Market rate. No tricks.\"";
                ev.Options.Add(new EventOption
                {
                    Label = "Pay him",
                    GoldCost = 10,
                    ResultText = "He swallows the Doom whole. It does not come back.",
                    Effects = { new ExileForceFromDrawEffect { Force = forces.Doom, Count = 1 } }
                });
                ev.Options.Add(new EventOption
                {
                    Label = "Refuse",
                    ResultText = "\"Suit yourself. Doom keeps its own books.\""
                });
            }, "Events");

            EventDefinition well = GetOrCreate<EventDefinition>("The Wishing Well", ev =>
            {
                ev.Intro = "A well of still, black water. Coins glitter at the bottom - other people's wishes. "
                    + "Five gold buys a flip of your own fate.";
                var wish = new EventOption
                {
                    Label = "Throw in 5g and flip",
                    GoldCost = 5,
                    FlipsFate = true,
                    Repeatable = true,
                    ClosesEvent = false
                };
                wish.RitualOutcomes.Add(Outcome(forces.Iron, "The well rings like an anvil. 15g surfaces.",
                    false, new GainGoldEffect { Amount = 15 }));
                wish.RitualOutcomes.Add(Outcome(forces.IronPlus, "The well rings twice. 18g surfaces.",
                    false, new GainGoldEffect { Amount = 18 }));
                wish.RitualOutcomes.Add(Outcome(forces.Flame, "Steam hisses off the water. 12g, slightly warm.",
                    false, new GainGoldEffect { Amount = 12 }));
                wish.RitualOutcomes.Add(Outcome(forces.FlamePlus, "Steam hisses. 12g, quite warm.",
                    false, new GainGoldEffect { Amount = 12 }));
                wish.RitualOutcomes.Add(Outcome(forces.Fortune, "The water turns gold for a heartbeat. 25g!",
                    false, new GainGoldEffect { Amount = 25 }));
                wish.RitualOutcomes.Add(Outcome(forces.FortunePlus, "The water turns solid gold. 27g!",
                    false, new GainGoldEffect { Amount = 27 }));
                wish.RitualOutcomes.Add(Outcome(forces.Decay, "The coin rots before it lands. Nothing.", false));
                wish.RitualOutcomes.Add(Outcome(forces.DecayPlus, "The coin rots mid-air. Nothing.", false));
                wish.RitualOutcomes.Add(Outcome(forces.Echo, "The well echoes - both wishes pay. 10g.",
                    false, new GainGoldEffect { Amount = 10 }));
                wish.RitualOutcomes.Add(Outcome(forces.Void, "The water goes glass-still. The well is closed.", true));
                wish.RitualOutcomes.Add(Outcome(forces.Doom, "The well bites.", true,
                    new MillPlayerEffect { Count = 2 }));
                ev.Options.Add(wish);
                ev.Options.Add(new EventOption { Label = "Keep your coins", ResultText = "The water does not judge." });
            }, "Events");

            EventDefinition sharp = GetOrCreate<EventDefinition>("The Card Sharp", ev =>
            {
                ev.Intro = "A gaunt figure fans a deck that looks suspiciously like yours. \"Two cards, any suit, "
                    + "free of charge. I'll just slip in a little something of mine.\"";
                ev.Options.Add(SharpOption(fields, fateCards, forces, "Iron"));
                ev.Options.Add(SharpOption(fields, fateCards, forces, "Flame"));
                ev.Options.Add(SharpOption(fields, fateCards, forces, "Fortune"));
                ev.Options.Add(new EventOption
                {
                    Label = "Walk away",
                    ResultText = "\"Pity. The house always slips one in eventually.\""
                });
            }, "Events");

            rooms.Pool.Add(GetOrCreate<EventRoomDefinition>("Room - The Beggar of Odds", room =>
            {
                room.Event = beggar;
                room.Blurb = "Someone is muttering about market rates.";
            }, "Rooms"));
            rooms.Pool.Add(GetOrCreate<EventRoomDefinition>("Room - The Wishing Well", room =>
            {
                room.Event = well;
                room.Blurb = "You hear water where no water should be.";
            }, "Rooms"));
            rooms.Pool.Add(GetOrCreate<EventRoomDefinition>("Room - The Card Sharp", room =>
            {
                room.Event = sharp;
                room.Blurb = "A shuffling sound, too fast to be honest.";
            }, "Rooms"));
        }

        private static RitualOutcome Outcome(AStergio.OmniCard.Runtime.Cards.MetaData.MetadataEntry force,
            string text, bool closes, params AStergio.OmniCard.Runtime.Cards.Effects.Base.CardEffect[] effects)
        {
            var outcome = new RitualOutcome { Force = force, ResultText = text, ClosesEvent = closes };
            outcome.Effects.AddRange(effects);
            return outcome;
        }

        private static EventOption SharpOption(Fields fields, List<CardDefinition> fateCards, Forces forces,
            string forceName)
        {
            CardDefinition gift = FindFateCard(fateCards, forceName);
            CardDefinition doom = FindFateCard(fateCards, "Doom");
            return new EventOption
            {
                Label = $"Take 2 {forceName} (he slips in 1 Doom)",
                ResultText = $"Two {forceName} join your fate - and something colder slides in with them.",
                Effects =
                {
                    new AddFateCardEffect { Card = gift, Zone = FateDeckZone.DrawPile, Count = 2 },
                    new AddFateCardEffect { Card = doom, Zone = FateDeckZone.DrawPile, Count = 1 }
                }
            };
        }
    }
}
