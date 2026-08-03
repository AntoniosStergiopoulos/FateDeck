using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using FateDeck.Runtime.Core;
using FateDeck.Runtime.Effects.Gameplay;
using FateDeck.Runtime.Effects.Laws;
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
            CreateCharms(fields, items);
            CreateRelics(fields, forces, items);
            return items;
        }

        private static void CreateCharms(Fields fields, Items items)
        {
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

            items.Charms.Add(Charm(fields, "Chalk Stub", "Scry 2 and reorder.", false,
                card => EffectsOf(card, fields.Effects).Add(new ScryEffect { Count = 2, AllowReorder = true })));
            items.Charms.Add(Charm(fields, "Wax Plug", "The next reshuffle adds 1 less Doom.", false,
                card => EffectsOf(card, fields.Effects).Add(
                    new ModifyReshuffleTaxEffect { Delta = -1, NextReshuffleOnly = true })));
            items.Charms.Add(Charm(fields, "Iron Filings", "+3 Force to your next action.", false,
                card => EffectsOf(card, fields.Effects).Add(new NextActionBonusEffect { Delta = 3 })));
            items.Charms.Add(Charm(fields, "Glass Vial", "Your next 2 flips: draw 2, choose which applies.", false,
                card => EffectsOf(card, fields.Effects).Add(new DoubleDrawNextEffect { Charges = 2 })));
            items.Charms.Add(Charm(fields, "Pocket Watch", "+1 Pocket slot for the rest of the run.", false,
                card => EffectsOf(card, fields.Effects).Add(new AddPocketSlotEffect { Delta = 1 })));
            items.Charms.Add(Charm(fields, "Ledger Page", "Gain 8 gold.", false,
                card => EffectsOf(card, fields.Effects).Add(new GainGoldEffect { Amount = 8 })));
            items.Charms.Add(Charm(fields, "Snuffed Wick", "Remove all Burn and Weak on you.", false,
                card =>
                {
                    EffectsOf(card, fields.Effects).Add(new ClearPlayerStatusEffect { Status = StatusKind.Burn });
                    EffectsOf(card, fields.Effects).Add(new ClearPlayerStatusEffect { Status = StatusKind.Weak });
                }));
            items.Charms.Add(Charm(fields, "Dove Feather", "Heal 2: choose wound cards to return.", false,
                card => EffectsOf(card, fields.Effects).Add(new HealWoundsEffect { Count = 2 })));
            items.Charms.Add(Charm(fields, "Powder Charge", "Deal 5 damage. No flip. Costs your Main Action.", true,
                card => EffectsOf(card, fields.Effects).Add(new DealDamageEffect { Amount = 5 })));
            items.Charms.Add(Charm(fields, "Serpent Fang", "Your target suffers Weak 2.", false,
                card => EffectsOf(card, fields.Effects).Add(new ApplyStatusEffect
                {
                    Status = StatusKind.Weak,
                    Stacks = 2,
                    Target = StatusTarget.SelectedEnemy
                })));
            items.Charms.Add(Charm(fields, "Skeleton Key", "Gain a Key (opens one locked chest).", false,
                card => EffectsOf(card, fields.Effects).Add(new GainKeyEffect { Count = 1 })));
            items.Charms.Add(Charm(fields, "Echo Bell", "+2 Force to your next action; Scry 1.", false,
                card =>
                {
                    EffectsOf(card, fields.Effects).Add(new NextActionBonusEffect { Delta = 2 });
                    EffectsOf(card, fields.Effects).Add(new ScryEffect { Count = 1, AllowReorder = false });
                }));
        }

        private static void CreateRelics(Fields fields, Forces forces, Items items)
        {
            items.Relics.Add(Relic(fields, "Anvil Creed", "Your Iron gives +3 Force instead of +2.", card =>
            {
                OnFlip(card, fields, forces.Iron, ActionOwnerFilter.Player,
                    new ModifyActionForceEffect { Delta = 1 });
            }));
            items.Relics.Add(Relic(fields, "Tollbooth", "Fortune flips grant +2 additional gold.", card =>
            {
                OnFlip(card, fields, forces.Fortune, ActionOwnerFilter.Any, new GainGoldEffect { Amount = 2 });
                OnFlip(card, fields, forces.FortunePlus, ActionOwnerFilter.Any, new GainGoldEffect { Amount = 2 });
            }));
            items.Relics.Add(Relic(fields, "Third Sleeve", "+1 Pocket slot.", card =>
            {
                EffectsOf(card, fields.Effects).Add(new AddPocketSlotEffect { Delta = 1 });
            }));
            items.Relics.Add(Relic(fields, "Magnet Ring", "After your action flips Iron, Scry 1.", card =>
            {
                OnFlip(card, fields, forces.Iron, ActionOwnerFilter.Player,
                    new ScryEffect { Count = 1, AllowReorder = false });
            }));
            items.Relics.Add(Relic(fields, "Scab Ritual", "At room end, return 1 wound card to the draw pile.",
                card =>
                {
                    var trigger = new OnRoomEndTrigger();
                    trigger.Effects.Add(new HealWoundsEffect { Count = 1 });
                    TriggersOf(card, fields.Triggers).Add(trigger);
                }));
            items.Relics.Add(Relic(fields, "Rustheart",
                "Decay on enemy actions grants +2 Force to your next action.", card =>
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

            items.Relics.Add(Relic(fields, "Storm Chart", "Your Tempest flips gain +1 Force.", card =>
            {
                OnFlip(card, fields, forces.Tempest, ActionOwnerFilter.Player,
                    new ModifyActionForceEffect { Delta = 1 });
                OnFlip(card, fields, forces.TempestPlus, ActionOwnerFilter.Player,
                    new ModifyActionForceEffect { Delta = 1 });
            }));
            items.Relics.Add(Relic(fields, "Venom Locket", "Your Serpent flips apply 1 extra Weak.", card =>
            {
                OnFlip(card, fields, forces.Serpent, ActionOwnerFilter.Player,
                    new WeakenActionVictimEffect { Stacks = 1 });
                OnFlip(card, fields, forces.SerpentPlus, ActionOwnerFilter.Player,
                    new WeakenActionVictimEffect { Stacks = 1 });
            }));
            items.Relics.Add(Relic(fields, "Doom Ledger", "Whenever Doom surfaces, the House pays you 2g.", card =>
            {
                OnFlip(card, fields, forces.Doom, ActionOwnerFilter.Any, new GainGoldEffect { Amount = 2 });
            }));
            items.Relics.Add(Relic(fields, "Wound Clasp", "Every card milled into the Wound Row pays 1g.", card =>
            {
                var trigger = new OnMillTrigger();
                trigger.Effects.Add(new GainGoldEffect { Amount = 1 });
                TriggersOf(card, fields.Triggers).Add(trigger);
            }));
            items.Relics.Add(Relic(fields, "Grease Trap", "Every reshuffle grants you 3 Block.", card =>
            {
                var trigger = new OnReshuffleTrigger();
                trigger.Effects.Add(new GainBlockEffect { Amount = 3 });
                TriggersOf(card, fields.Triggers).Add(trigger);
            }));
            items.Relics.Add(Relic(fields, "Pocket Mirror",
                "After you play a card from the Pocket, +2 Force to your next action.", card =>
            {
                var trigger = new OnPocketPlayTrigger();
                trigger.Effects.Add(new NextActionBonusEffect { Delta = 2 });
                TriggersOf(card, fields.Triggers).Add(trigger);
            }));
            items.Relics.Add(Relic(fields, "Round Bell", "From round 2 on, each of your turns starts with 1 Block.",
                card =>
                {
                    var trigger = new OnPlayerTurnStartTrigger { SkipFirstRound = true };
                    trigger.Effects.Add(new GainBlockEffect { Amount = 1 });
                    TriggersOf(card, fields.Triggers).Add(trigger);
                }));
            items.Relics.Add(Relic(fields, "Bone Dice",
                "The first flip of each combat: draw 2, choose which applies.", card =>
            {
                var trigger = new OnCombatStartTrigger();
                trigger.Effects.Add(new DoubleDrawNextEffect { Charges = 1 });
                TriggersOf(card, fields.Triggers).Add(trigger);
            }));
            items.Relics.Add(Relic(fields, "Butcher's Hook", "Whenever an enemy dies, heal 1 wound.", card =>
            {
                var trigger = new OnEnemyDeathTrigger { SelfOnly = false };
                trigger.Effects.Add(new HealWoundsEffect { Count = 1 });
                TriggersOf(card, fields.Triggers).Add(trigger);
            }));
            items.Relics.Add(Relic(fields, "Salt Ring", "Start every combat with 3 Block.", card =>
            {
                var trigger = new OnCombatStartTrigger();
                trigger.Effects.Add(new GainBlockEffect { Amount = 3 });
                TriggersOf(card, fields.Triggers).Add(trigger);
            }));
            items.Relics.Add(Relic(fields, "Collector's Cufflink", "Fortune flips also Scry 1.", card =>
            {
                OnFlip(card, fields, forces.Fortune, ActionOwnerFilter.Any,
                    new ScryEffect { Count = 1, AllowReorder = false });
                OnFlip(card, fields, forces.FortunePlus, ActionOwnerFilter.Any,
                    new ScryEffect { Count = 1, AllowReorder = false });
            }));
            items.Relics.Add(Relic(fields, "Tinderbox", "Your Flame flips also deal 1 damage to the target.",
                card =>
                {
                    OnFlip(card, fields, forces.Flame, ActionOwnerFilter.Player, new DealDamageEffect { Amount = 1 });
                    OnFlip(card, fields, forces.FlamePlus, ActionOwnerFilter.Player,
                        new DealDamageEffect { Amount = 1 });
                }));
        }

        private static void OnFlip(CardDefinition card, Fields fields,
            AStergio.OmniCard.Runtime.Cards.MetaData.MetadataEntry force, ActionOwnerFilter owner,
            params AStergio.OmniCard.Runtime.Cards.Effects.Base.CardEffect[] effects)
        {
            var trigger = new OnFateFlipTrigger { ForceFilter = force, Owner = owner };
            trigger.Effects.AddRange(effects);
            TriggersOf(card, fields.Triggers).Add(trigger);
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
