using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using FateDeck.Runtime.Core;
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
            public List<FightRoomDefinition> Elites = new List<FightRoomDefinition>();
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
            CardDefinition usurer = DustUsurer(fields);
            CardDefinition grub = ChandlerGrub(fields);
            CardDefinition leech = InkwellLeech(fields);
            CardDefinition doll = PorcelainDoll(fields);
            CardDefinition wisp = LedgerWisp(fields);
            CardDefinition sergeant = MatchSergeant(fields);
            CardDefinition moth = MothBroker(fields);
            CardDefinition beetle = GildedBeetle(fields);
            CardDefinition golem = PaperGolem(fields);
            CardDefinition smotherer = CandleSmotherer(fields);
            CardDefinition twin = BellowsTwin(fields);
            CardDefinition adder = CoilAdder(fields);
            CardDefinition wight = ChandelierWight(fields);
            CardDefinition tick = VaultTick(fields);
            CardDefinition grudge = GrudgeCandle(fields);
            CardDefinition mason = MarrowMason(fields);
            CardDefinition toll = TollCollector(fields);
            CardDefinition underwriter = TheUnderwriter(fields);
            CardDefinition notary = TheNotary(fields);
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
            AddFight(rooms, "Room - Dust Usurer", Deck("Encounter - Dust Usurer", (usurer, 1)),
                "A minor creditor. Its skim cannot be blocked.");
            AddFight(rooms, "Room - Chandler Grubs", Deck("Encounter - Chandler Grubs", (grub, 2)),
                "Two grubs wind up in alternation. Guard the slams.", minStep: 2);
            AddFight(rooms, "Room - Inkwell Leech", Deck("Encounter - Inkwell Leech", (leech, 1)),
                "It drinks as fast as you cut. Burst it down.", minStep: 2);
            AddFight(rooms, "Room - Porcelain Dolls", Deck("Encounter - Porcelain Dolls", (doll, 2)),
                "Two dolls. Each one shatters into your deck when it dies.", minStep: 3);
            AddFight(rooms, "Room - Ledger Wisps", Deck("Encounter - Ledger Wisps", (wisp, 2)),
                "Flying pages that audit your top cards and burn your clock.", minStep: 2);
            AddFight(rooms, "Room - Match Sergeant", Deck("Encounter - Match Sergeant", (sergeant, 1)),
                "It sets you alight and hides behind regulation Block.", minStep: 3);
            AddFight(rooms, "Room - Moth Broker", Deck("Encounter - Moth Broker", (moth, 1), (imp, 1)),
                "A broker and its imp. The dust makes you swing soft.", minStep: 3);
            AddFight(rooms, "Room - Gilded Beetle", Deck("Encounter - Gilded Beetle", (beetle, 1)),
                "Slow, armored, and worth a small fortune. A knowing bet.", minStep: 4);
            AddFight(rooms, "Room - Paper Golem", Deck("Encounter - Paper Golem", (golem, 1)),
                "Ten thousand unpaid invoices, folded into a fist.", minStep: 5);
            AddFight(rooms, "Room - Candle Smotherer", Deck("Encounter - Candle Smotherer", (smotherer, 1)),
                "Two damp fingers the size of oars. Block means little here.", minStep: 5);
            AddFight(rooms, "Room - Doll and Wisp", Deck("Encounter - Doll and Wisp", (doll, 1), (wisp, 1)),
                "A doll that shatters and a page that audits.", minStep: 3);
            AddFight(rooms, "Room - Bellows Twins", Deck("Encounter - Bellows Twins", (twin, 2)),
                "Two of them, off-beat: one puffs while the other blasts.");
            AddFight(rooms, "Room - Coil Adder", Deck("Encounter - Coil Adder", (adder, 1)),
                "Something soft-scaled softens its meals first.");
            AddFight(rooms, "Room - Chandelier Wight", Deck("Encounter - Chandelier Wight", (wight, 1)),
                "A hundred candles with a grudge. Bring something for the Burn.", minStep: 4);
            AddFight(rooms, "Room - Vault Ticks", Deck("Encounter - Vault Ticks", (tick, 2)),
                "They want your purse, not your deck. Kill them to claw it all back.");
            AddFight(rooms, "Room - Grudge Candle", Deck("Encounter - Grudge Candle", (grudge, 1)),
                "It burns hotter every cycle. Race it.", minStep: 3);
            AddFight(rooms, "Room - Marrow Mason", Deck("Encounter - Marrow Mason", (mason, 1)),
                "A wall that hits back. Rust and big single hits crack it.", minStep: 5);
            AddFight(rooms, "Room - Adder and Tick", Deck("Encounter - Adder and Tick", (adder, 1), (tick, 1)),
                "Venom for your arm, fingers for your purse.", minStep: 4);
            rooms.Elites.Add(GetOrCreate<FightRoomDefinition>("Room - The Toll Collector", room =>
            {
                room.Encounter = Deck("Encounter - Toll Collector", (toll, 1));
                room.IsElite = true;
                room.Blurb = "The Toll Collector waits with an open ledger. Its tithe cannot be blocked. "
                    + "Decline the door and its relic is gone forever.";
            }, "Rooms"));
            rooms.Elites.Add(GetOrCreate<FightRoomDefinition>("Room - The Underwriter", room =>
            {
                room.Encounter = Deck("Encounter - The Underwriter", (underwriter, 1));
                room.IsElite = true;
                room.Blurb = "The Underwriter re-prices your reshuffle clock itself. "
                    + "Decline the door and its relic is gone forever.";
            }, "Rooms"));
            rooms.Elites.Add(GetOrCreate<FightRoomDefinition>("Room - The Notary", room =>
            {
                room.Encounter = Deck("Encounter - The Notary", (notary, 1));
                room.IsElite = true;
                room.Blurb = "The Notary stamps you smaller with every seal. "
                    + "Decline the door and its relic is gone forever.";
            }, "Rooms"));

            rooms.Boss = GetOrCreate<BossRoomDefinition>("Room - THE COLLECTOR", room =>
            {
                room.Encounter = Deck("Encounter - The Collector", (collector, 1));
                room.Blurb = "The vault at the bottom of the mailroom. It appraises only your draw pile - "
                    + "spread your forces, or keep your favorites in discard, pocket and wounds.";
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
                room.Blurb = "Exile one card from your fate, free. Debt clings - it costs gold to burn.";
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
            AStergio.OmniCard.Runtime.Cards.Game.Decks.DeckDefinition encounter, string blurb,
            int minStep = 0)
        {
            rooms.Pool.Add(GetOrCreate<FightRoomDefinition>(name, room =>
            {
                room.Encounter = encounter;
                room.Blurb = blurb;
                room.MinStep = minStep;
            }, "Rooms"));
        }

        private static void AddEventRoom(Rooms rooms, string name, EventDefinition definition, string blurb)
        {
            rooms.Pool.Add(GetOrCreate<EventRoomDefinition>($"Room - {name}", room =>
            {
                room.Event = definition;
                room.Blurb = blurb;
            }, "Rooms"));
        }

        private static void CreateEvents(Fields fields, Forces forces, List<CardDefinition> fateCards, Rooms rooms)
        {
            CardDefinition CardOf(string name) => FindFateCard(fateCards, name);

            EventDefinition beggar = GetOrCreate<EventDefinition>("The Beggar of Odds", ev =>
            {
                ev.Intro = "A beggar sits cross-legged on a pile of losing tickets. \"Ten gold,\" he says, "
                    + "\"and I eat one of your Debts. Market rate. No tricks.\"";
                ev.Options.Add(new EventOption
                {
                    Label = "Pay him",
                    GoldCost = 10,
                    ResultText = "He swallows the Debt whole. It does not come back.",
                    Effects = { new ExileForceFromDrawEffect { Force = forces.Doom, Count = 1 } }
                });
                ev.Options.Add(new EventOption
                {
                    Label = "Refuse",
                    ResultText = "\"Suit yourself. Debt keeps its own books.\""
                });
            }, "Events");
            AddEventRoom(rooms, "The Beggar of Odds", beggar, "Someone is muttering about market rates.");

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
                    ClosesEvent = false,
                    ResultText = "The water swallows the coin and says nothing."
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
                wish.RitualOutcomes.Add(Outcome(forces.Tempest, "Thunder under water. 14g surfaces, crackling.",
                    false, new GainGoldEffect { Amount = 14 }));
                wish.RitualOutcomes.Add(Outcome(forces.TempestPlus, "A storm in a bucket. 16g surfaces.",
                    false, new GainGoldEffect { Amount = 16 }));
                wish.RitualOutcomes.Add(Outcome(forces.Serpent, "Something scaled nudges 8g to the surface.",
                    false, new GainGoldEffect { Amount = 8 }));
                wish.RitualOutcomes.Add(Outcome(forces.SerpentPlus, "Something large and scaled donates 10g.",
                    false, new GainGoldEffect { Amount = 10 }));
                wish.RitualOutcomes.Add(Outcome(forces.Glass, "The shard sings as it sinks: 20g of applause.",
                    false, new GainGoldEffect { Amount = 20 }));
                wish.RitualOutcomes.Add(Outcome(forces.Gloom, "The dark water knits one of your wounds shut.",
                    false, new HealWoundsEffect { Count = 1 }));
                wish.RitualOutcomes.Add(Outcome(forces.Key, "A key floats up, politely.",
                    false, new GainKeyEffect { Count = 1 }));
                wish.RitualOutcomes.Add(Outcome(forces.Mirror, "The well reflects your last wish. 10g.",
                    false, new GainGoldEffect { Amount = 10 }));
                wish.RitualOutcomes.Add(Outcome(forces.Anchor, "The coin sinks straight down. The well approves: 6g.",
                    false, new GainGoldEffect { Amount = 6 }));
                wish.RitualOutcomes.Add(Outcome(forces.Rust, "Rust blooms across the water. 5g, slightly orange.",
                    false, new GainGoldEffect { Amount = 5 }));
                wish.RitualOutcomes.Add(Outcome(forces.Wisp, "A light darts below and returns with 7g.",
                    false, new GainGoldEffect { Amount = 7 }));
                ev.Options.Add(wish);
                ev.Options.Add(new EventOption { Label = "Keep your coins", ResultText = "The water does not judge." });
            }, "Events");
            AddEventRoom(rooms, "The Wishing Well", well, "You hear water where no water should be.");

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
            AddEventRoom(rooms, "The Card Sharp", sharp, "A shuffling sound, too fast to be honest.");

            EventDefinition pawn = GetOrCreate<EventDefinition>("The Pawnbroker", ev =>
            {
                ev.Intro = "Three brass balls hang over a hole in the wall. \"Cards ARE money,\" rasps the hole. "
                    + "\"I give fair rates for luck you aren't using.\"";
                ev.Options.Add(new EventOption
                {
                    Label = "Pawn a card (exile 1 from your discard, +8g)",
                    ResultText = "The card vanishes into the hole. Coins roll out.",
                    Effects =
                    {
                        new GainGoldEffect { Amount = 8 },
                        new ZoneChoiceEffect { Kind = ZoneChoiceKind.ExileFromDiscard, Count = 1 }
                    }
                });
                ev.Options.Add(new EventOption
                {
                    Label = "Keep your luck",
                    ResultText = "\"Sentiment. It's why the House always wins.\""
                });
            }, "Events");
            AddEventRoom(rooms, "The Pawnbroker", pawn, "Three brass balls glint over a dark slot.");

            EventDefinition glassman = GetOrCreate<EventDefinition>("The Glass Salesman", ev =>
            {
                ev.Intro = "A coat of a thousand shards. \"Glass, friend. +4 Force, once - then it shatters "
                    + "beautifully. Nothing this bright is meant to last.\"";
                ev.Options.Add(new EventOption
                {
                    Label = "Buy the pair (12g): 2 Glass",
                    GoldCost = 12,
                    ResultText = "Two bright shards slide into your fate.",
                    Effects = { new AddFateCardEffect { Card = CardOf("Glass"), Zone = FateDeckZone.DrawPile, Count = 2 } }
                });
                ev.Options.Add(new EventOption
                {
                    Label = "Free sample (he slips in 1 Debt)",
                    ResultText = "One shard, one splinter of something colder.",
                    Effects =
                    {
                        new AddFateCardEffect { Card = CardOf("Glass"), Zone = FateDeckZone.DrawPile, Count = 1 },
                        new AddFateCardEffect { Card = CardOf("Debt"), Zone = FateDeckZone.DrawPile, Count = 1 }
                    }
                });
                ev.Options.Add(new EventOption { Label = "Decline", ResultText = "The coat tinkles away." });
            }, "Events");
            AddEventRoom(rooms, "The Glass Salesman", glassman, "Something crystalline is whistling a jingle.");

            EventDefinition stormDoor = GetOrCreate<EventDefinition>("The Storm Door", ev =>
            {
                ev.Intro = "A door stands open onto a hallway of weather. Lightning arcs politely around the frame. "
                    + "The storm wants into your deck.";
                ev.Options.Add(new EventOption
                {
                    Label = "Walk through (free): 1 Tempest and 1 Debt",
                    ResultText = "Thunder files itself between your cards. So does the fine print.",
                    Effects =
                    {
                        new AddFateCardEffect { Card = CardOf("Tempest"), Zone = FateDeckZone.DrawPile, Count = 1 },
                        new AddFateCardEffect { Card = CardOf("Debt"), Zone = FateDeckZone.DrawPile, Count = 1 }
                    }
                });
                ev.Options.Add(new EventOption
                {
                    Label = "Bribe the wind (10g): 1 Tempest, clean",
                    GoldCost = 10,
                    ResultText = "The storm pockets your coin and behaves.",
                    Effects = { new AddFateCardEffect { Card = CardOf("Tempest"), Zone = FateDeckZone.DrawPile, Count = 1 } }
                });
                ev.Options.Add(new EventOption { Label = "Close the door", ResultText = "The weather sighs." });
            }, "Events");
            AddEventRoom(rooms, "The Storm Door", stormDoor, "You smell rain indoors.");

            EventDefinition snakePit = GetOrCreate<EventDefinition>("The Snake Pit", ev =>
            {
                ev.Intro = "A dry fountain full of gently seething rope. One strand lifts its head "
                    + "and looks at your deck with professional interest.";
                ev.Options.Add(new EventOption
                {
                    Label = "Reach in (free): 2 Serpent, but it bites (mill 1)",
                    ResultText = "Venom joins your fate. So does a toothmark.",
                    Effects =
                    {
                        new AddFateCardEffect { Card = CardOf("Serpent"), Zone = FateDeckZone.DrawPile, Count = 2 },
                        new MillPlayerEffect { Count = 1 }
                    }
                });
                ev.Options.Add(new EventOption
                {
                    Label = "Charm it (8g): 1 Serpent, no bite",
                    GoldCost = 8,
                    ResultText = "It slides into your deck like it always lived there.",
                    Effects = { new AddFateCardEffect { Card = CardOf("Serpent"), Zone = FateDeckZone.DrawPile, Count = 1 } }
                });
                ev.Options.Add(new EventOption { Label = "Back away slowly", ResultText = "The rope settles." });
            }, "Events");
            AddEventRoom(rooms, "The Snake Pit", snakePit, "A dry fountain seethes softly.");

            EventDefinition widow = GetOrCreate<EventDefinition>("The Locksmith's Widow", ev =>
            {
                ev.Intro = "She polishes keys that no longer have doors. \"He made every lock down here,\" "
                    + "she says. \"I sell what opens them.\"";
                ev.Options.Add(new EventOption
                {
                    Label = "Buy the spares (6g): 2 Keys",
                    GoldCost = 6,
                    ResultText = "Two brass teeth, still warm from the polishing cloth.",
                    Effects = { new GainKeyEffect { Count = 2 } }
                });
                var reading = new EventOption
                {
                    Label = "Ask her to read the wards (free flip)",
                    FlipsFate = true,
                    ResultText = "She turns your card over with a locksmith's care."
                };
                reading.RitualOutcomes.Add(Outcome(forces.Iron, "\"Sturdy. Like him.\" She gifts you a Key.",
                    false, new GainKeyEffect { Count = 1 }));
                reading.RitualOutcomes.Add(Outcome(forces.Fortune, "\"Lucky hands.\" Two Keys, free.",
                    false, new GainKeyEffect { Count = 2 }));
                reading.RitualOutcomes.Add(Outcome(forces.Key, "\"One of his!\" She weeps, and arms you: 2 Keys.",
                    false, new GainKeyEffect { Count = 2 }));
                reading.RitualOutcomes.Add(Outcome(forces.Doom, "\"Oh. Oh no. Go. GO.\"", true,
                    new MillPlayerEffect { Count = 1 }));
                reading.RitualOutcomes.Add(Outcome(forces.Void, "\"The wards are silent.\" She closes the case.", true));
                ev.Options.Add(reading);
                ev.Options.Add(new EventOption { Label = "Leave her be", ResultText = "The polishing goes on." });
            }, "Events");
            AddEventRoom(rooms, "The Locksmith's Widow", widow, "Somewhere, keys click like knitting.");

            EventDefinition auction = GetOrCreate<EventDefinition>("The Debt Auction", ev =>
            {
                ev.Intro = "A lectern, a gavel, a crowd of empty coats. \"Lot 44: two certificates of DOOM. "
                    + "The House will PAY the taker fifteen gold. Do I hear a fool?\"";
                ev.Options.Add(new EventOption
                {
                    Label = "Take the lot (+15g, +2 Debt)",
                    ResultText = "The coats applaud silently. The gold is real. So is the Debt.",
                    Effects =
                    {
                        new GainGoldEffect { Amount = 15 },
                        new AddFateCardEffect { Card = CardOf("Debt"), Zone = FateDeckZone.DrawPile, Count = 2 }
                    }
                });
                ev.Options.Add(new EventOption
                {
                    Label = "Keep your name off the books",
                    ResultText = "The gavel falls on someone else's fate."
                });
            }, "Events");
            AddEventRoom(rooms, "The Debt Auction", auction, "A gavel echoes in an empty hall.");

            EventDefinition peddler = GetOrCreate<EventDefinition>("The Mirror Peddler", ev =>
            {
                ev.Intro = "A cart of mirrors that reflect cards instead of faces. \"Mirrors repeat whatever "
                    + "fate came before them,\" the peddler grins. \"Pair one with your best law.\"";
                ev.Options.Add(new EventOption
                {
                    Label = "The set (10g): 1 Mirror and 1 Wisp",
                    GoldCost = 10,
                    ResultText = "Silver and lantern-light slide into your deck.",
                    Effects =
                    {
                        new AddFateCardEffect { Card = CardOf("Mirror"), Zone = FateDeckZone.DrawPile, Count = 1 },
                        new AddFateCardEffect { Card = CardOf("Wisp"), Zone = FateDeckZone.DrawPile, Count = 1 }
                    }
                });
                ev.Options.Add(new EventOption
                {
                    Label = "Trade a memory (mill 1): 1 Mirror",
                    ResultText = "The mirror takes its price off the top of you.",
                    Effects =
                    {
                        new MillPlayerEffect { Count = 1 },
                        new AddFateCardEffect { Card = CardOf("Mirror"), Zone = FateDeckZone.DrawPile, Count = 1 }
                    }
                });
                ev.Options.Add(new EventOption { Label = "Avert your eyes", ResultText = "The cart rolls on." });
            }, "Events");
            AddEventRoom(rooms, "The Mirror Peddler", peddler, "You catch your deck's reflection in passing glass.");

            EventDefinition bonesetter = GetOrCreate<EventDefinition>("The Bonesetter", ev =>
            {
                ev.Intro = "A folding chair, a bag of splints, hands like old rope. \"Wounds, is it? "
                    + "Sit. The House breaks; I set.\"";
                ev.Options.Add(new EventOption
                {
                    Label = "Full setting (5g): heal 3 wounds",
                    GoldCost = 5,
                    ResultText = "Three cards crack back into place.",
                    Effects = { new HealWoundsEffect { Count = 3 } }
                });
                ev.Options.Add(new EventOption
                {
                    Label = "Quick look (free): heal 1 wound",
                    ResultText = "\"That one was just dislocated.\"",
                    Effects = { new HealWoundsEffect { Count = 1 } }
                });
                ev.Options.Add(new EventOption { Label = "Walk it off", ResultText = "\"They all say that.\"" });
            }, "Events");
            AddEventRoom(rooms, "The Bonesetter", bonesetter, "A folding chair waits under a lone bulb.");

            EventDefinition vigil = GetOrCreate<EventDefinition>("The Candle Vigil", ev =>
            {
                ev.Intro = "A hundred candles around a single unlit wick. Light it, and the flame reads "
                    + "whatever fate you feed it.";
                var light = new EventOption
                {
                    Label = "Light the wick (free flip)",
                    FlipsFate = true,
                    ResultText = "The flame gutters, unreadable."
                };
                light.RitualOutcomes.Add(Outcome(forces.Flame, "The flame ROARS approval: +5g and a Flame joins you.",
                    false, new GainGoldEffect { Amount = 5 },
                    new AddFateCardEffect { Card = CardOf("Flame"), Zone = FateDeckZone.DrawPile, Count = 1 }));
                light.RitualOutcomes.Add(Outcome(forces.FlamePlus, "The flame HOWLS: +8g and a Flame joins you.",
                    false, new GainGoldEffect { Amount = 8 },
                    new AddFateCardEffect { Card = CardOf("Flame"), Zone = FateDeckZone.DrawPile, Count = 1 }));
                light.RitualOutcomes.Add(Outcome(forces.Iron, "The wax sets iron-hard. An Iron joins your deck.",
                    false, new AddFateCardEffect { Card = CardOf("Iron"), Zone = FateDeckZone.DrawPile, Count = 1 }));
                light.RitualOutcomes.Add(Outcome(forces.Fortune, "Molten gold beads on the wick: +10g.",
                    false, new GainGoldEffect { Amount = 10 }));
                light.RitualOutcomes.Add(Outcome(forces.Echo, "The flame doubles, pays double: +6g.",
                    false, new GainGoldEffect { Amount = 6 }));
                light.RitualOutcomes.Add(Outcome(forces.Void, "The wick refuses. The vigil is over.", true));
                light.RitualOutcomes.Add(Outcome(forces.Doom, "Every candle dies at once.", true,
                    new MillPlayerEffect { Count = 2 }));
                ev.Options.Add(light);
                ev.Options.Add(new EventOption { Label = "Let it sleep", ResultText = "The candles keep their own vigil." });
            }, "Events");
            AddEventRoom(rooms, "The Candle Vigil", vigil, "A hundred small flames breathe in unison.");

            EventDefinition scale = GetOrCreate<EventDefinition>("The Scale of Debts", ev =>
            {
                ev.Intro = "A brass scale the size of a door. One pan holds a feather. The other waits "
                    + "for your worst card.";
                ev.Options.Add(new EventOption
                {
                    Label = "Confess (mill 1): exile 1 Debt from your draw pile",
                    ResultText = "The scale tips. Something red and heavy is carried away.",
                    Effects =
                    {
                        new MillPlayerEffect { Count = 1 },
                        new ExileForceFromDrawEffect { Force = forces.Doom, Count = 1 }
                    }
                });
                ev.Options.Add(new EventOption
                {
                    Label = "Deny everything",
                    ResultText = "The feather doesn't move. It knows."
                });
            }, "Events");
            AddEventRoom(rooms, "The Scale of Debts", scale, "Brass groans under the weight of nothing.");

            EventDefinition monk = GetOrCreate<EventDefinition>("The Whetstone Monk", ev =>
            {
                ev.Intro = "A monk kneels over a whetstone the size of a millwheel, sharpening nothing. "
                    + "\"Bring me a law,\" he says, \"and I will give it an edge.\"";
                ev.Options.Add(new EventOption
                {
                    Label = "Offer a card (free): upgrade it to its + tier",
                    ResultText = "Sparks the color of your fate. The card comes back keener.",
                    Effects = { new ZoneChoiceEffect { Kind = ZoneChoiceKind.UpgradeFromDraw, Count = 1 } }
                });
                ev.Options.Add(new EventOption
                {
                    Label = "Bow and pass",
                    ResultText = "The wheel keeps turning, sharpening patience."
                });
            }, "Events");
            AddEventRoom(rooms, "The Whetstone Monk", monk, "A grinding wheel sings one clean note.");

            EventDefinition chained = GetOrCreate<EventDefinition>("The Chained Chest", ev =>
            {
                ev.Intro = "A chest wound in chains, each link stamped PROPERTY OF THE HOUSE. "
                    + "The lock is honest. The contents are not the House's to keep.";
                ev.Options.Add(new EventOption
                {
                    Label = "Turn a Key: take everything",
                    KeyCost = 1,
                    ResultText = "The chains applaud as they fall. 18g and a spare key inside.",
                    Effects =
                    {
                        new GainGoldEffect { Amount = 18 },
                        new GainKeyEffect { Count = 1 }
                    }
                });
                var pry = new EventOption
                {
                    Label = "Pry at it (free flip)",
                    FlipsFate = true,
                    ResultText = "The chains hold. The House smiles somewhere."
                };
                pry.RitualOutcomes.Add(Outcome(forces.Iron, "Iron against iron - the lock gives! 12g.",
                    false, new GainGoldEffect { Amount = 12 }));
                pry.RitualOutcomes.Add(Outcome(forces.IronPlus, "The chains snap like bread. 15g.",
                    false, new GainGoldEffect { Amount = 15 }));
                pry.RitualOutcomes.Add(Outcome(forces.Flame, "The chains glow, soften, drip. 14g, warm.",
                    false, new GainGoldEffect { Amount = 14 }));
                pry.RitualOutcomes.Add(Outcome(forces.Key, "The Key force IS a key. 16g, smugly.",
                    false, new GainGoldEffect { Amount = 16 }));
                pry.RitualOutcomes.Add(Outcome(forces.Doom, "The chains bite back.", true,
                    new MillPlayerEffect { Count = 2 }));
                pry.RitualOutcomes.Add(Outcome(forces.Void, "The lock swallows its own keyhole.", true));
                ev.Options.Add(pry);
                ev.Options.Add(new EventOption { Label = "Leave it chained", ResultText = "Some property stays repossessed." });
            }, "Events");
            AddEventRoom(rooms, "The Chained Chest", chained, "Chains rattle around something generous.");

            EventDefinition dicePriest = GetOrCreate<EventDefinition>("The Dice Priest", ev =>
            {
                ev.Intro = "A priest of the Odds shakes a cup of dice carved from reshuffled decks. "
                    + "\"One free blessing,\" she says, \"denomination decided by your own fate.\"";
                var blessing = new EventOption
                {
                    Label = "Receive the blessing (free flip)",
                    FlipsFate = true,
                    ResultText = "The dice come up blank. She shrugs."
                };
                blessing.RitualOutcomes.Add(Outcome(forces.Fortune, "\"The Lady smiles!\" Two Draw-2 charges.",
                    false, new DoubleDrawNextEffect { Charges = 2 }));
                blessing.RitualOutcomes.Add(Outcome(forces.FortunePlus, "\"The Lady BEAMS!\" Two Draw-2 charges and 5g.",
                    false, new DoubleDrawNextEffect { Charges = 2 }, new GainGoldEffect { Amount = 5 }));
                blessing.RitualOutcomes.Add(Outcome(forces.Iron, "\"Strength, then.\" +3 Force to your next action.",
                    false, new NextActionBonusEffect { Delta = 3 }));
                blessing.RitualOutcomes.Add(Outcome(forces.IronPlus, "\"STRENGTH.\" +4 Force to your next action.",
                    false, new NextActionBonusEffect { Delta = 4 }));
                blessing.RitualOutcomes.Add(Outcome(forces.Echo, "\"Twice-blessed.\" One Draw-2 charge and Scry 2.",
                    false, new DoubleDrawNextEffect { Charges = 1 }, new ScryEffect { Count = 2, AllowReorder = true }));
                blessing.RitualOutcomes.Add(Outcome(forces.Wisp, "\"Sight, then.\" Scry 3, reorder.",
                    false, new ScryEffect { Count = 3, AllowReorder = true }));
                blessing.RitualOutcomes.Add(Outcome(forces.Gloom, "\"Mending, then.\" Heal 2 wounds.",
                    false, new HealWoundsEffect { Count = 2 }));
                blessing.RitualOutcomes.Add(Outcome(forces.Doom, "The dice come up skulls. All of them.", true,
                    new MillPlayerEffect { Count = 1 }));
                blessing.RitualOutcomes.Add(Outcome(forces.Void, "The cup is empty. It was always empty.", true));
                ev.Options.Add(blessing);
                ev.Options.Add(new EventOption { Label = "Keep your own odds", ResultText = "She rattles on." });
            }, "Events");
            AddEventRoom(rooms, "The Dice Priest", dicePriest, "Dice rattle in a cup somewhere close.");

            EventDefinition moltingPit = GetOrCreate<EventDefinition>("The Molting Pit", ev =>
            {
                ev.Intro = "A pit of shed skins - old decks, old luck, old selves. Things left here "
                    + "stay left. The pit hums, patient.";
                ev.Options.Add(new EventOption
                {
                    Label = "Shed (free): exile 1 card from your discard",
                    ResultText = "The old law slides off you. The pit sighs contentedly.",
                    Effects = { new ZoneChoiceEffect { Kind = ZoneChoiceKind.ExileFromDiscard, Count = 1 } }
                });
                ev.Options.Add(new EventOption
                {
                    Label = "Deep molt (8g): exile 1 and heal 1 wound",
                    GoldCost = 8,
                    ResultText = "Something old leaves; something torn knits.",
                    Effects =
                    {
                        new ZoneChoiceEffect { Kind = ZoneChoiceKind.ExileFromDiscard, Count = 1 },
                        new HealWoundsEffect { Count = 1 }
                    }
                });
                ev.Options.Add(new EventOption { Label = "Stay in your skin", ResultText = "The pit hums on." });
            }, "Events");
            AddEventRoom(rooms, "The Molting Pit", moltingPit, "Something in the dark is shedding.");

            EventDefinition ragman = GetOrCreate<EventDefinition>("The Rag-and-Bone Man", ev =>
            {
                ev.Intro = "A cart of things the House repossessed. \"Any old luck! Any old luck! "
                    + "Trade us a card, get a key for the honest doors.\"";
                ev.Options.Add(new EventOption
                {
                    Label = "Trade (exile 1 from discard): gain a Key",
                    ResultText = "Your card joins the cart. A key finds your palm.",
                    Effects =
                    {
                        new GainKeyEffect { Count = 1 },
                        new ZoneChoiceEffect { Kind = ZoneChoiceKind.ExileFromDiscard, Count = 1 }
                    }
                });
                ev.Options.Add(new EventOption { Label = "Nothing today", ResultText = "The cart squeaks on." });
            }, "Events");
            AddEventRoom(rooms, "The Rag-and-Bone Man", ragman, "A wheel squeaks: any old luck, any old luck.");
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
            CardDefinition doom = FindFateCard(fateCards, "Debt");
            return new EventOption
            {
                Label = $"Take 2 {forceName} (he slips in 1 Debt)",
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
