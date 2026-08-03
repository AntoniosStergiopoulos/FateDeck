using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.Fields.Kinds;
using AStergio.OmniCard.Runtime.Cards.Fields.Values;
using AStergio.OmniCard.Runtime.Cards.Game.Decks;
using AStergio.OmniCard.Runtime.Cards.Game.Zones;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Combat;
using FateDeck.Runtime.Core;
using FateDeck.Runtime.Effects.Gameplay;
using FateDeck.Runtime.Effects.Laws;
using UnityEngine;

namespace FateDeck.Tests
{
    /// <summary>Builds an in-memory catalog (no AssetDatabase) mirroring the generator's shape.</summary>
    internal sealed class TestContent
    {
        public FateContentCatalog Catalog;
        public CardDefinition IronCard;
        public CardDefinition FortuneCard;
        public CardDefinition DoomCard;
        public CardDefinition GlassCard;
        public CardDefinition MirrorCard;
        public CardDefinition TempestCard;
        public CardDefinition Enemy;
        public CardDefinition Collector;
        public CardDefinition Hero;
        public FateDeck.Runtime.Run.FightRoomDefinition FightRoom;
        public FateDeck.Runtime.Run.FightRoomDefinition PairRoom;
        public FateDeck.Runtime.Run.FightRoomDefinition CollectorRoom;
        public readonly List<Object> Owned = new List<Object>();

        public static TestContent Create()
        {
            var content = new TestContent();
            FateContentCatalog catalog = content.New<FateContentCatalog>("Catalog");
            content.Catalog = catalog;

            var rules = content.New<FateRulesDefinition>("Rules");
            catalog.Rules = rules;

            CardFieldDefinition Law(string name) => content.Field(name, new EffectListFieldKind());
            catalog.LawOffenseField = Law("Law Offense");
            catalog.LawDefenseField = Law("Law Defense");
            catalog.LawEnemyField = Law("Law Enemy");
            catalog.LawLootField = Law("Law Loot");
            catalog.LawRitualField = Law("Law Ritual");
            catalog.CannotPocketField = content.Field("Cannot Pocket", new BooleanFieldKind());
            catalog.CannotStackField = content.Field("Cannot Stack", new BooleanFieldKind());
            catalog.ExileWhenMilledField = content.Field("Exile When Milled", new BooleanFieldKind());
            catalog.ExileAfterFlipField = content.Field("Exile After Flip", new BooleanFieldKind());
            catalog.ForceColorField = content.Field("Force Color", new ColorFieldKind());
            catalog.ForceGlyphField = content.Field("Force Glyph", new TextFieldKind());
            catalog.DescriptionField = content.Field("Description", new TextFieldKind());

            var forceKind = content.New<MetadataKind>("Force Kind");
            forceKind.EntryFields.AddRange(new[]
            {
                catalog.LawOffenseField, catalog.LawDefenseField, catalog.LawEnemyField,
                catalog.LawLootField, catalog.LawRitualField, catalog.CannotPocketField,
                catalog.CannotStackField, catalog.ExileWhenMilledField, catalog.ExileAfterFlipField,
                catalog.ForceColorField, catalog.ForceGlyphField, catalog.DescriptionField
            });
            catalog.ForceKind = forceKind;

            catalog.Iron = content.Force(forceKind, catalog, "Iron", new ModifyActionForceEffect { Delta = 2 });
            catalog.Fortune = content.Force(forceKind, catalog, "Fortune", new BankGoldEffect { Amount = 3 });
            catalog.Doom = content.DoomForce(forceKind, catalog);
            catalog.Echo = content.Force(forceKind, catalog, "Echo", new EchoFlipEffect());
            catalog.Void = content.Force(forceKind, catalog, "Void", new NegateActionEffect());
            catalog.Glass = content.Force(forceKind, catalog, "Glass", new ModifyActionForceEffect { Delta = 4 });
            ((BooleanFieldValue)catalog.Glass.GetValue(catalog.ExileAfterFlipField)).Value = true;
            catalog.Mirror = content.Force(forceKind, catalog, "Mirror", new CopyLastForceEffect());
            catalog.Tempest = content.Force(forceKind, catalog, "Tempest", new CleaveDamageEffect { Amount = 2 });

            catalog.ForceField = content.Field("Force", new ReferenceFieldKind { Kind = forceKind });
            var fateSchema = content.New<CardSchema>("Fate Schema");
            fateSchema.Fields.AddRange(new[] { catalog.ForceField, catalog.DescriptionField });
            catalog.FateCardSchema = fateSchema;

            content.IronCard = content.FateCard(fateSchema, catalog, "Iron", catalog.Iron);
            content.FortuneCard = content.FateCard(fateSchema, catalog, "Fortune", catalog.Fortune);
            content.DoomCard = content.FateCard(fateSchema, catalog, "Doom", catalog.Doom);
            content.GlassCard = content.FateCard(fateSchema, catalog, "Glass", catalog.Glass);
            content.MirrorCard = content.FateCard(fateSchema, catalog, "Mirror", catalog.Mirror);
            content.TempestCard = content.FateCard(fateSchema, catalog, "Tempest", catalog.Tempest);
            catalog.FateCards.Add(content.IronCard);
            catalog.FateCards.Add(content.FortuneCard);
            catalog.FateCards.Add(content.DoomCard);
            catalog.FateCards.Add(content.GlassCard);
            catalog.FateCards.Add(content.MirrorCard);
            catalog.FateCards.Add(content.TempestCard);

            catalog.HpField = content.Field("HP", new NumberFieldKind());
            catalog.MaxHpField = content.Field("Max HP", new NumberFieldKind());
            catalog.BountyField = content.Field("Bounty", new NumberFieldKind());
            catalog.PatternField = content.Field("Pattern", new EnemyPatternFieldKind());
            catalog.PatternIndexField = content.Field("Pattern Index", new NumberFieldKind());
            catalog.ForceBonusField = content.Field("Force Bonus", new NumberFieldKind());
            catalog.BlockField = content.Field("Enemy Block", new NumberFieldKind());
            catalog.BurnField = content.Field("Burn", new NumberFieldKind());
            catalog.WeakField = content.Field("Weak", new NumberFieldKind());
            catalog.PocketedGoldField = content.Field("Pocketed Gold", new NumberFieldKind());
            catalog.MantleBonusPerField = content.Field("Mantle Bonus Per", new NumberFieldKind());
            catalog.ActionsPerRoundField = content.Field("Actions Per Round", new NumberFieldKind { DefaultValue = 1 });
            catalog.GimmickField = content.Field("Gimmick", new TextFieldKind());
            catalog.EffectsField = content.Field("Effects", new EffectListFieldKind());
            catalog.TriggersField = content.Field("Triggers", new TriggerListFieldKind());
            catalog.MainActionField = content.Field("Is Main Action", new BooleanFieldKind());
            catalog.PocketSlotsField = content.Field("Pocket Slots", new NumberFieldKind { DefaultValue = 2 });
            catalog.StartingDeckField = content.Field("Starting Deck", new ObjectFieldKind());

            var enemySchema = content.New<CardSchema>("Enemy Schema");
            enemySchema.Fields.AddRange(new[]
            {
                catalog.HpField, catalog.MaxHpField, catalog.BountyField, catalog.PatternField,
                catalog.PatternIndexField, catalog.ForceBonusField, catalog.BlockField, catalog.BurnField,
                catalog.WeakField, catalog.PocketedGoldField, catalog.MantleBonusPerField,
                catalog.ActionsPerRoundField, catalog.TriggersField
            });
            catalog.EnemySchema = enemySchema;

            var heroSchema = content.New<CardSchema>("Hero Schema");
            heroSchema.Fields.AddRange(new[]
            {
                catalog.PocketSlotsField, catalog.StartingDeckField, catalog.TriggersField
            });
            catalog.HeroSchema = heroSchema;

            catalog.DrawPile = content.Zone("Draw");
            catalog.DiscardPile = content.Zone("Discard");
            catalog.WoundRow = content.Zone("Wound");
            catalog.Pocket = content.Zone("Pocket");
            catalog.ExilePile = content.Zone("Exile");
            catalog.Enemies = content.Zone("Enemies");
            catalog.Slain = content.Zone("Slain");
            catalog.Relics = content.Zone("Relics");
            catalog.Charms = content.Zone("Charms");
            catalog.Mantle = content.Zone("Mantle");

            content.Enemy = content.New<CardDefinition>("Grunt");
            content.Enemy.Schema = enemySchema;
            content.Enemy.SyncValuesWithSchema();
            ((NumberFieldValue)content.Enemy.GetValue(catalog.HpField)).Value = 8;
            ((NumberFieldValue)content.Enemy.GetValue(catalog.MaxHpField)).Value = 8;
            ((NumberFieldValue)content.Enemy.GetValue(catalog.BountyField)).Value = 5;
            ((EnemyPatternFieldValue)content.Enemy.GetValue(catalog.PatternField)).Steps.Add(
                new EnemyActionSpec { Name = "Attack", Kind = EnemyActionKind.Attack, Force = 3, FlipsFate = true });

            var starterDeck = content.New<DeckDefinition>("Starter");
            starterDeck.Cards.Add(new DeckEntry { Card = content.IronCard, Count = 5 });
            starterDeck.Cards.Add(new DeckEntry { Card = content.FortuneCard, Count = 3 });
            starterDeck.Cards.Add(new DeckEntry { Card = content.DoomCard, Count = 1 });

            content.Hero = content.New<CardDefinition>("Hero");
            content.Hero.Schema = heroSchema;
            content.Hero.SyncValuesWithSchema();
            ((NumberFieldValue)content.Hero.GetValue(catalog.PocketSlotsField)).Value = 2;
            ((ObjectFieldValue)content.Hero.GetValue(catalog.StartingDeckField)).Value = starterDeck;
            catalog.Heroes.Add(content.Hero);

            content.FightRoom = content.New<FateDeck.Runtime.Run.FightRoomDefinition>("Fight");
            var encounter = content.New<DeckDefinition>("Encounter");
            encounter.Cards.Add(new DeckEntry { Card = content.Enemy, Count = 1 });
            content.FightRoom.Encounter = encounter;

            content.PairRoom = content.New<FateDeck.Runtime.Run.FightRoomDefinition>("Pair Fight");
            var pairEncounter = content.New<DeckDefinition>("Pair Encounter");
            pairEncounter.Cards.Add(new DeckEntry { Card = content.Enemy, Count = 2 });
            content.PairRoom.Encounter = pairEncounter;

            content.Collector = content.New<CardDefinition>("Collector");
            content.Collector.Schema = enemySchema;
            content.Collector.SyncValuesWithSchema();
            ((NumberFieldValue)content.Collector.GetValue(catalog.HpField)).Value = 25;
            ((NumberFieldValue)content.Collector.GetValue(catalog.MaxHpField)).Value = 25;
            ((NumberFieldValue)content.Collector.GetValue(catalog.BountyField)).Value = 10;
            ((NumberFieldValue)content.Collector.GetValue(catalog.MantleBonusPerField)).Value = 3;
            ((EnemyPatternFieldValue)content.Collector.GetValue(catalog.PatternField)).Steps.Add(
                new EnemyActionSpec { Name = "Attack", Kind = EnemyActionKind.Attack, Force = 4, FlipsFate = true });

            content.CollectorRoom = content.New<FateDeck.Runtime.Run.FightRoomDefinition>("Collector Fight");
            var bossEncounter = content.New<DeckDefinition>("Collector Encounter");
            bossEncounter.Cards.Add(new DeckEntry { Card = content.Collector, Count = 1 });
            content.CollectorRoom.Encounter = bossEncounter;

            return content;
        }

        public void Destroy()
        {
            foreach (Object owned in Owned)
            {
                Object.DestroyImmediate(owned);
            }

            Owned.Clear();
        }

        private T New<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            Owned.Add(asset);
            return asset;
        }

        private CardFieldDefinition Field(string name, CardFieldKind kind)
        {
            CardFieldDefinition field = New<CardFieldDefinition>(name);
            field.Kind = kind;
            return field;
        }

        private ZoneDefinition Zone(string name)
        {
            return New<ZoneDefinition>(name);
        }

        private MetadataEntry Force(MetadataKind kind, FateContentCatalog catalog, string name,
            AStergio.OmniCard.Runtime.Cards.Effects.Base.CardEffect law)
        {
            MetadataEntry entry = New<MetadataEntry>(name);
            entry.Kind = kind;
            entry.SyncValuesWithKind();
            AddLaw(entry, catalog.LawOffenseField, Clone(law));
            AddLaw(entry, catalog.LawDefenseField, Clone(law));
            AddLaw(entry, catalog.LawEnemyField, Clone(law));
            AddLaw(entry, catalog.LawLootField, Clone(law));
            AddLaw(entry, catalog.LawRitualField, Clone(law));
            return entry;
        }

        private MetadataEntry DoomForce(MetadataKind kind, FateContentCatalog catalog)
        {
            MetadataEntry entry = New<MetadataEntry>("Doom");
            entry.Kind = kind;
            entry.SyncValuesWithKind();
            ((BooleanFieldValue)entry.GetValue(catalog.CannotPocketField)).Value = true;
            ((BooleanFieldValue)entry.GetValue(catalog.CannotStackField)).Value = true;
            ((BooleanFieldValue)entry.GetValue(catalog.ExileWhenMilledField)).Value = true;
            AddLaw(entry, catalog.LawOffenseField, new SetActionForceEffect { Value = 0 });
            AddLaw(entry, catalog.LawOffenseField, new MillPlayerEffect { Count = 1 });
            AddLaw(entry, catalog.LawEnemyField, new ModifyActionForceEffect { Delta = 3 });
            return entry;
        }

        private static void AddLaw(MetadataEntry entry, CardFieldDefinition field,
            AStergio.OmniCard.Runtime.Cards.Effects.Base.CardEffect effect)
        {
            ((EffectListFieldValue)entry.GetValue(field)).Effects.Add(effect);
        }

        private static AStergio.OmniCard.Runtime.Cards.Effects.Base.CardEffect Clone(
            AStergio.OmniCard.Runtime.Cards.Effects.Base.CardEffect law)
        {
            switch (law)
            {
                case ModifyActionForceEffect modify: return new ModifyActionForceEffect { Delta = modify.Delta };
                case BankGoldEffect bank: return new BankGoldEffect { Amount = bank.Amount };
                case EchoFlipEffect _: return new EchoFlipEffect();
                case NegateActionEffect _: return new NegateActionEffect();
                default: return law;
            }
        }

        private CardDefinition FateCard(CardSchema schema, FateContentCatalog catalog, string name, MetadataEntry force)
        {
            CardDefinition card = New<CardDefinition>(name);
            card.Schema = schema;
            card.SyncValuesWithSchema();
            ((ReferenceFieldValue)card.GetValue(catalog.ForceField)).Value = force;
            return card;
        }
    }
}
