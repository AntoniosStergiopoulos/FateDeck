using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using FateDeck.Runtime.Core;
using FateDeck.Runtime.Effects.Gameplay;
using FateDeck.Runtime.Triggers;

namespace FateDeck.Editor
{
    public static partial class FateDeckContentGenerator
    {
        /// <summary>
        /// The playable heroes. Every hero is 15 cards of a different shape of luck plus one
        /// passive built from the same trigger atoms relics use.
        /// </summary>
        private static List<CardDefinition> CreateHeroes(Fields fields, Forces forces,
            List<CardDefinition> fateCards)
        {
            CardDefinition CardOf(string name) => FindFateCard(fateCards, name);

            var heroes = new List<CardDefinition>();

            heroes.Add(Hero(fields, "The Gambler",
                "Opening Hand - the first flip of each combat: draw 2, choose 1.",
                pocketSlots: 2,
                Deck("Gambler Starting Deck",
                    (CardOf("Iron"), 9), (CardOf("Fortune"), 3), (CardOf("Flame"), 1),
                    (CardOf("Echo"), 1), (CardOf("Debt"), 1)),
                hero =>
                {
                    var opening = new OnCombatStartTrigger();
                    opening.Effects.Add(new DoubleDrawNextEffect { Charges = 1 });
                    TriggersOf(hero, fields.Triggers).Add(opening);
                }));

            heroes.Add(Hero(fields, "The Stoker",
                "Stoked Coals - whenever your action flips Flame, gain 1 Block.",
                pocketSlots: 2,
                Deck("Stoker Starting Deck",
                    (CardOf("Iron"), 7), (CardOf("Flame"), 4), (CardOf("Fortune"), 2),
                    (CardOf("Echo"), 1), (CardOf("Debt"), 1)),
                hero =>
                {
                    var stoked = new OnFateFlipTrigger
                    {
                        Owner = ActionOwnerFilter.Player,
                        ForceFilter = forces.Flame
                    };
                    stoked.Effects.Add(new GainBlockEffect { Amount = 1 });
                    TriggersOf(hero, fields.Triggers).Add(stoked);

                    var stokedPlus = new OnFateFlipTrigger
                    {
                        Owner = ActionOwnerFilter.Player,
                        ForceFilter = forces.FlamePlus
                    };
                    stokedPlus.Effects.Add(new GainBlockEffect { Amount = 1 });
                    TriggersOf(hero, fields.Triggers).Add(stokedPlus);
                }));

            heroes.Add(Hero(fields, "The Actuary",
                "Full Audit - every combat starts with Scry 3 (reorder). Pocket holds 3.",
                pocketSlots: 3,
                Deck("Actuary Starting Deck",
                    (CardOf("Iron"), 6), (CardOf("Fortune"), 3), (CardOf("Decay"), 3),
                    (CardOf("Wisp"), 2), (CardOf("Debt"), 1)),
                hero =>
                {
                    var audit = new OnCombatStartTrigger();
                    audit.Effects.Add(new ScryEffect { Count = 3, AllowReorder = true });
                    TriggersOf(hero, fields.Triggers).Add(audit);
                }));

            heroes.Add(Hero(fields, "The Debtor",
                "Compound Interest - whenever Debt surfaces, the House pays you 3g. Starts 2 Debt deep.",
                pocketSlots: 2,
                Deck("Debtor Starting Deck",
                    (CardOf("Iron"), 9), (CardOf("Fortune"), 3), (CardOf("Echo"), 1),
                    (CardOf("Debt"), 2)),
                hero =>
                {
                    var interest = new OnFateFlipTrigger { ForceFilter = forces.Doom };
                    interest.Effects.Add(new GainGoldEffect { Amount = 3 });
                    TriggersOf(hero, fields.Triggers).Add(interest);
                }));

            heroes.Add(Hero(fields, "The Sexton",
                "Gravework - at the end of every room, 1 wound card returns to your deck.",
                pocketSlots: 2,
                Deck("Sexton Starting Deck",
                    (CardOf("Iron"), 8), (CardOf("Gloom"), 2), (CardOf("Fortune"), 2),
                    (CardOf("Anchor"), 2), (CardOf("Debt"), 1)),
                hero =>
                {
                    var gravework = new OnRoomEndTrigger();
                    gravework.Effects.Add(new HealWoundsEffect { Count = 1 });
                    TriggersOf(hero, fields.Triggers).Add(gravework);
                }));

            return heroes;
        }

        private static CardDefinition Hero(Fields fields, string name, string passive, int pocketSlots,
            AStergio.OmniCard.Runtime.Cards.Game.Decks.DeckDefinition deck,
            System.Action<CardDefinition> configure)
        {
            return Card(fields.HeroSchema, name, hero =>
            {
                SetText(hero, fields.Description, passive);
                SetNumber(hero, fields.PocketSlots, pocketSlots);
                SetObject(hero, fields.StartingDeck, deck);
                configure?.Invoke(hero);
            }, "Heroes");
        }
    }
}
