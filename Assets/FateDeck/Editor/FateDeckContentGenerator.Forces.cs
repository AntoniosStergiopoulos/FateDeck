using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Effects.Gameplay;
using FateDeck.Runtime.Effects.Laws;
using UnityEngine;

namespace FateDeck.Editor
{
    public static partial class FateDeckContentGenerator
    {
        private sealed class Forces
        {
            public MetadataEntry Iron;
            public MetadataEntry IronPlus;
            public MetadataEntry Flame;
            public MetadataEntry FlamePlus;
            public MetadataEntry Decay;
            public MetadataEntry DecayPlus;
            public MetadataEntry Fortune;
            public MetadataEntry FortunePlus;
            public MetadataEntry Echo;
            public MetadataEntry Void;
            public MetadataEntry Doom;

            public IEnumerable<MetadataEntry> All()
            {
                yield return Iron;
                yield return IronPlus;
                yield return Flame;
                yield return FlamePlus;
                yield return Decay;
                yield return DecayPlus;
                yield return Fortune;
                yield return FortunePlus;
                yield return Echo;
                yield return Void;
                yield return Doom;
            }
        }

        private static Forces CreateForces(Fields fields)
        {
            var forces = new Forces
            {
                Iron = ForceEntry(fields, "Iron", "I", new Color(0.62f, 0.66f, 0.72f),
                    "The action's Force +2.",
                    context => new CardEffect[] { new ModifyActionForceEffect { Delta = 2 } }),
                IronPlus = ForceEntry(fields, "Iron+", "I+", new Color(0.75f, 0.79f, 0.85f),
                    "The action's Force +3.",
                    context => new CardEffect[] { new ModifyActionForceEffect { Delta = 3 } }),
                Decay = ForceEntry(fields, "Decay", "D", new Color(0.26f, 0.70f, 0.55f),
                    "The action's Force -2 (min 0).",
                    context => new CardEffect[] { new ModifyActionForceEffect { Delta = -2 } }),
                DecayPlus = ForceEntry(fields, "Decay+", "D+", new Color(0.36f, 0.82f, 0.65f),
                    "The action's Force -3 (min 0).",
                    context => new CardEffect[] { new ModifyActionForceEffect { Delta = -3 } }),
                Fortune = ForceEntry(fields, "Fortune", "$", new Color(0.79f, 0.64f, 0.15f),
                    "The action's owner banks 3 gold; Force +0.",
                    context => new CardEffect[] { new BankGoldEffect { Amount = 3 } }),
                FortunePlus = ForceEntry(fields, "Fortune+", "$+", new Color(0.91f, 0.76f, 0.25f),
                    "The action's owner banks 5 gold; Force +0.",
                    context => new CardEffect[] { new BankGoldEffect { Amount = 5 } }),
                Echo = ForceEntry(fields, "Echo", "E", new Color(0.43f, 0.35f, 0.56f),
                    "Flip one additional fate card; apply both laws (max 3 per action).",
                    context => new CardEffect[] { new EchoFlipEffect() }),
                Void = ForceEntry(fields, "Void", "O", new Color(0.82f, 0.82f, 0.86f),
                    "The action resolves at Force 0 with no effects; no other laws trigger.",
                    context => new CardEffect[] { new NegateActionEffect() })
            };

            forces.Flame = FlameEntry(fields, "Flame", "F", new Color(0.89f, 0.35f, 0.13f), 2,
                "The action's target suffers 2 Burn.");
            forces.FlamePlus = FlameEntry(fields, "Flame+", "F+", new Color(0.98f, 0.48f, 0.22f), 3,
                "The action's target suffers 3 Burn.");
            forces.Doom = DoomEntry(fields);
            return forces;
        }

        private static MetadataEntry ForceEntry(Fields fields, string name, string glyph, Color color,
            string description, System.Func<FateDeck.Runtime.Core.LawContext, CardEffect[]> lawFor)
        {
            return GetOrCreate<MetadataEntry>(name, entry =>
            {
                entry.Kind = fields.ForceKind;
                entry.SyncValuesWithKind();
                SetText(entry, fields.ForceGlyph, glyph);
                SetColor(entry, fields.ForceColor, color);
                SetText(entry, fields.Description, description);
                EffectsOf(entry, fields.LawOffense).AddRange(lawFor(FateDeck.Runtime.Core.LawContext.PlayerOffense));
                EffectsOf(entry, fields.LawDefense).AddRange(lawFor(FateDeck.Runtime.Core.LawContext.PlayerDefense));
                EffectsOf(entry, fields.LawEnemy).AddRange(lawFor(FateDeck.Runtime.Core.LawContext.EnemyAction));
                EffectsOf(entry, fields.LawLoot).AddRange(lawFor(FateDeck.Runtime.Core.LawContext.Loot));
                EffectsOf(entry, fields.LawRitual).AddRange(lawFor(FateDeck.Runtime.Core.LawContext.Ritual));
            }, "Forces");
        }

        private static MetadataEntry FlameEntry(Fields fields, string name, string glyph, Color color,
            int stacks, string description)
        {
            return GetOrCreate<MetadataEntry>(name, entry =>
            {
                entry.Kind = fields.ForceKind;
                entry.SyncValuesWithKind();
                SetText(entry, fields.ForceGlyph, glyph);
                SetColor(entry, fields.ForceColor, color);
                SetText(entry, fields.Description, description);
                EffectsOf(entry, fields.LawOffense).Add(new BurnActionVictimEffect { Stacks = stacks });
                EffectsOf(entry, fields.LawDefense).Add(new GuardRetaliateBurnEffect { Stacks = stacks });
                EffectsOf(entry, fields.LawEnemy).Add(new BurnActionVictimEffect { Stacks = stacks });
                EffectsOf(entry, fields.LawLoot).Add(new FlameLootEffect { GoldBurned = 3 });
                EffectsOf(entry, fields.LawRitual).Add(new BurnActionVictimEffect { Stacks = stacks });
            }, "Forces");
        }

        private static MetadataEntry DoomEntry(Fields fields)
        {
            return GetOrCreate<MetadataEntry>("Doom", entry =>
            {
                entry.Kind = fields.ForceKind;
                entry.SyncValuesWithKind();
                SetText(entry, fields.ForceGlyph, "X");
                SetColor(entry, fields.ForceColor, new Color(0.62f, 0.16f, 0.19f));
                SetText(entry, fields.Description,
                    "The worst, always. Cannot be pocketed or stacked; milled Doom is exiled forever.");
                SetBoolean(entry, fields.CannotPocket, true);
                SetBoolean(entry, fields.CannotStack, true);
                SetBoolean(entry, fields.ExileWhenMilled, true);
                EffectsOf(entry, fields.LawOffense).Add(new SetActionForceEffect { Value = 0 });
                EffectsOf(entry, fields.LawOffense).Add(new MillPlayerEffect { Count = 1 });
                EffectsOf(entry, fields.LawDefense).Add(new SetActionForceEffect { Value = 0 });
                EffectsOf(entry, fields.LawDefense).Add(new MillPlayerEffect { Count = 1 });
                EffectsOf(entry, fields.LawEnemy).Add(new ModifyActionForceEffect { Delta = 3 });
                EffectsOf(entry, fields.LawLoot).Add(new DoomLootEffect { Mill = 2 });
                EffectsOf(entry, fields.LawRitual).Add(new MillPlayerEffect { Count = 1 });
            }, "Forces");
        }

        private static List<CardDefinition> CreateFateCards(Fields fields, Forces forces)
        {
            var cards = new List<CardDefinition>();
            foreach (MetadataEntry force in forces.All())
            {
                MetadataEntry captured = force;
                cards.Add(Card(fields.FateCardSchema, captured.name, card =>
                {
                    SetReference(card, fields.Force, captured);
                    SetText(card, fields.Description, captured.GetText(fields.Description));
                }));
            }

            return cards;
        }
    }
}
