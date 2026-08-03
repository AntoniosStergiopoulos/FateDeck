using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using FateDeck.Runtime.Core;
using FateDeck.Runtime.Effects.Gameplay;
using FateDeck.Runtime.Triggers;

namespace FateDeck.Editor
{
    public static partial class FateDeckContentGenerator
    {
        private sealed class Items
        {
            public List<CardDefinition> Charms = new List<CardDefinition>();
            public List<CardDefinition> Relics = new List<CardDefinition>();
        }

        private static Items CreateItems(Fields fields, Forces forces)
        {
            var items = new Items();
            items.Charms.Add(Charm(fields, "Loupe", "Scry 3 and reorder the top of your deck.", false,
                card => EffectsOf(card, fields.Effects).Add(new ScryEffect { Count = 3, AllowReorder = true })));
            items.Charms.Add(Charm(fields, "Tongs", "Exile 1 card from your discard pile.", false,
                card => EffectsOf(card, fields.Effects).Add(
                    new ZoneChoiceEffect { Kind = ZoneChoiceKind.ExileFromDiscard, Count = 1 })));
            items.Charms.Add(Charm(fields, "Loaded Coin", "Your next flip: draw 2, choose which applies.", false,
                card => EffectsOf(card, fields.Effects).Add(new DoubleDrawNextEffect { Charges = 1 })));
            items.Charms.Add(Charm(fields, "Blood Salve", "Heal 3: choose wound cards to return.", false,
                card => EffectsOf(card, fields.Effects).Add(new HealWoundsEffect { Count = 3 })));
            items.Charms.Add(Charm(fields, "Firecracker", "Deal 3 damage. No flip. Costs your Main Action.", true,
                card => EffectsOf(card, fields.Effects).Add(new DealDamageEffect { Amount = 3 })));
            items.Charms.Add(Charm(fields, "Smelling Salts", "Remove all Burn on you; gain 2 Block.", false,
                card =>
                {
                    EffectsOf(card, fields.Effects).Add(new ClearPlayerStatusEffect { Status = StatusKind.Burn });
                    EffectsOf(card, fields.Effects).Add(new GainBlockEffect { Amount = 2 });
                }));

            items.Relics.Add(Relic(fields, "Anvil Creed", "Your Iron gives +3 Force instead of +2.", card =>
            {
                var trigger = new OnFateFlipTrigger { ForceFilter = forces.Iron, Owner = ActionOwnerFilter.Player };
                trigger.Effects.Add(new FateDeck.Runtime.Effects.Laws.ModifyActionForceEffect { Delta = 1 });
                TriggersOf(card, fields.Triggers).Add(trigger);
            }));
            items.Relics.Add(Relic(fields, "Tollbooth", "Fortune flips grant +2 additional gold.", card =>
            {
                var trigger = new OnFateFlipTrigger { ForceFilter = forces.Fortune };
                trigger.Effects.Add(new GainGoldEffect { Amount = 2 });
                TriggersOf(card, fields.Triggers).Add(trigger);
            }));
            items.Relics.Add(Relic(fields, "Third Sleeve", "+1 Pocket slot.", card =>
            {
                EffectsOf(card, fields.Effects).Add(new AddPocketSlotEffect { Delta = 1 });
            }));
            items.Relics.Add(Relic(fields, "Magnet Ring", "After your action flips Iron, Scry 1.", card =>
            {
                var trigger = new OnFateFlipTrigger { ForceFilter = forces.Iron, Owner = ActionOwnerFilter.Player };
                trigger.Effects.Add(new ScryEffect { Count = 1, AllowReorder = false });
                TriggersOf(card, fields.Triggers).Add(trigger);
            }));
            items.Relics.Add(Relic(fields, "Scab Ritual", "At room end, return 1 wound card to the draw pile.", card =>
            {
                var trigger = new OnRoomEndTrigger();
                trigger.Effects.Add(new HealWoundsEffect { Count = 1 });
                TriggersOf(card, fields.Triggers).Add(trigger);
            }));
            items.Relics.Add(Relic(fields, "Rustheart", "Decay on enemy actions grants +2 Force to your next action.", card =>
            {
                var trigger = new OnFateFlipTrigger
                {
                    AnyContext = false,
                    Context = LawContext.EnemyAction,
                    ForceFilter = forces.Decay
                };
                trigger.Effects.Add(new NextActionBonusEffect { Delta = 2 });
                TriggersOf(card, fields.Triggers).Add(trigger);
            }));

            return items;
        }

        private static CardDefinition Charm(Fields fields, string name, string description, bool mainAction,
            System.Action<CardDefinition> configure)
        {
            return Card(fields.CharmSchema, name, card =>
            {
                SetText(card, fields.Description, description);
                SetBoolean(card, fields.MainAction, mainAction);
                configure?.Invoke(card);
            }, "Items");
        }

        private static CardDefinition Relic(Fields fields, string name, string description,
            System.Action<CardDefinition> configure)
        {
            return Card(fields.RelicSchema, name, card =>
            {
                SetText(card, fields.Description, description);
                configure?.Invoke(card);
            }, "Items");
        }

        private static CardDefinition CreateHeroes(Fields fields, Forces forces, List<CardDefinition> fateCards)
        {
            CardDefinition iron = FindFateCard(fateCards, "Iron");
            CardDefinition fortune = FindFateCard(fateCards, "Fortune");
            CardDefinition doom = FindFateCard(fateCards, "Doom");
            var starterDeck = Deck("Gambler Starting Deck", (iron, 10), (fortune, 4), (doom, 1));

            return Card(fields.HeroSchema, "The Gambler", card =>
            {
                SetText(card, fields.Description,
                    "Opening Hand - the first flip of each combat: draw 2, choose 1.");
                SetNumber(card, fields.PocketSlots, 2);
                SetObject(card, fields.StartingDeck, starterDeck);
                var opening = new OnCombatStartTrigger();
                opening.Effects.Add(new DoubleDrawNextEffect { Charges = 1 });
                TriggersOf(card, fields.Triggers).Add(opening);
            }, "Heroes");
        }

        private static CardDefinition FindFateCard(List<CardDefinition> fateCards, string name)
        {
            foreach (CardDefinition card in fateCards)
            {
                if (card != null && card.name == name)
                {
                    return card;
                }
            }

            return null;
        }
    }
}
