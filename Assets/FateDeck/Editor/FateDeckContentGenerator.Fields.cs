using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.Fields.Kinds;
using AStergio.OmniCard.Runtime.Cards.Game.Zones;
using FateDeck.Runtime.Combat;

namespace FateDeck.Editor
{
    public static partial class FateDeckContentGenerator
    {
        private sealed class Fields
        {
            public CardFieldDefinition Force;
            public CardFieldDefinition Description;
            public CardFieldDefinition LawOffense;
            public CardFieldDefinition LawDefense;
            public CardFieldDefinition LawEnemy;
            public CardFieldDefinition LawLoot;
            public CardFieldDefinition LawRitual;
            public CardFieldDefinition CannotPocket;
            public CardFieldDefinition CannotStack;
            public CardFieldDefinition ExileWhenMilled;
            public CardFieldDefinition ExileAfterFlip;
            public CardFieldDefinition ForceColor;
            public CardFieldDefinition ForceGlyph;
            public CardFieldDefinition Hp;
            public CardFieldDefinition MaxHp;
            public CardFieldDefinition Bounty;
            public CardFieldDefinition Pattern;
            public CardFieldDefinition PatternIndex;
            public CardFieldDefinition ForceBonus;
            public CardFieldDefinition Block;
            public CardFieldDefinition Burn;
            public CardFieldDefinition Weak;
            public CardFieldDefinition PocketedGold;
            public CardFieldDefinition MantleBonusPer;
            public CardFieldDefinition ActionsPerRound;
            public CardFieldDefinition Gimmick;
            public CardFieldDefinition Effects;
            public CardFieldDefinition Triggers;
            public CardFieldDefinition Price;
            public CardFieldDefinition MainAction;
            public CardFieldDefinition PocketSlots;
            public CardFieldDefinition StartingDeck;

            public AStergio.OmniCard.Runtime.Cards.MetaData.MetadataKind ForceKind;
            public CardSchema FateCardSchema;
            public CardSchema EnemySchema;
            public CardSchema RelicSchema;
            public CardSchema CharmSchema;
            public CardSchema HeroSchema;
        }

        private sealed class Zones
        {
            public ZoneDefinition Draw;
            public ZoneDefinition Discard;
            public ZoneDefinition Wound;
            public ZoneDefinition Pocket;
            public ZoneDefinition Exile;
            public ZoneDefinition Enemies;
            public ZoneDefinition Slain;
            public ZoneDefinition Relics;
            public ZoneDefinition Charms;
            public ZoneDefinition Mantle;
        }

        private static Fields CreateFields()
        {
            var fields = new Fields
            {
                Description = Field("Description", new TextFieldKind { MultiLine = true }),
                LawOffense = Field("Law - Your Offense", new EffectListFieldKind()),
                LawDefense = Field("Law - Your Defense", new EffectListFieldKind()),
                LawEnemy = Field("Law - Enemy Action", new EffectListFieldKind()),
                LawLoot = Field("Law - Loot", new EffectListFieldKind()),
                LawRitual = Field("Law - Ritual", new EffectListFieldKind()),
                CannotPocket = Field("Cannot Pocket", new BooleanFieldKind()),
                CannotStack = Field("Cannot Stack", new BooleanFieldKind()),
                ExileWhenMilled = Field("Exile When Milled", new BooleanFieldKind()),
                ExileAfterFlip = Field("Exile After Flip", new BooleanFieldKind()),
                ForceColor = Field("Force Color", new ColorFieldKind()),
                ForceGlyph = Field("Force Glyph", new TextFieldKind()),
                Hp = Field("HP", new NumberFieldKind { MutableAtRuntime = true }),
                MaxHp = Field("Max HP", new NumberFieldKind()),
                Bounty = Field("Bounty", new NumberFieldKind()),
                Pattern = Field("Pattern", new EnemyPatternFieldKind()),
                PatternIndex = Field("Pattern Index", new NumberFieldKind { MutableAtRuntime = true }),
                ForceBonus = Field("Force Bonus", new NumberFieldKind { MutableAtRuntime = true }),
                Block = Field("Enemy Block", new NumberFieldKind { MutableAtRuntime = true }),
                Burn = Field("Burn", new NumberFieldKind { MutableAtRuntime = true }),
                Weak = Field("Weak", new NumberFieldKind { MutableAtRuntime = true }),
                PocketedGold = Field("Pocketed Gold", new NumberFieldKind { MutableAtRuntime = true }),
                MantleBonusPer = Field("Mantle Bonus Per", new NumberFieldKind()),
                ActionsPerRound = Field("Actions Per Round", new NumberFieldKind { DefaultValue = 1 }),
                Gimmick = Field("Gimmick", new TextFieldKind { MultiLine = true }),
                Effects = Field("Effects", new EffectListFieldKind()),
                Triggers = Field("Triggers", new TriggerListFieldKind()),
                Price = Field("Price", new NumberFieldKind()),
                MainAction = Field("Is Main Action", new BooleanFieldKind()),
                PocketSlots = Field("Pocket Slots", new NumberFieldKind { DefaultValue = 2 }),
                StartingDeck = Field("Starting Deck", new ObjectFieldKind())
            };

            fields.ForceKind = GetOrCreate<AStergio.OmniCard.Runtime.Cards.MetaData.MetadataKind>("Force Kind", null);
            EnsureKindFields(fields.ForceKind,
                fields.LawOffense, fields.LawDefense, fields.LawEnemy, fields.LawLoot, fields.LawRitual,
                fields.CannotPocket, fields.CannotStack, fields.ExileWhenMilled, fields.ExileAfterFlip,
                fields.ForceColor, fields.ForceGlyph, fields.Description);

            fields.Force = Field("Force", new ReferenceFieldKind { Kind = fields.ForceKind });

            fields.FateCardSchema = GetOrCreate<CardSchema>("Fate Card Schema", null);
            EnsureSchemaFields(fields.FateCardSchema, fields.Force, fields.Description);

            fields.EnemySchema = GetOrCreate<CardSchema>("Enemy Schema", null);
            EnsureSchemaFields(fields.EnemySchema,
                fields.Hp, fields.MaxHp, fields.Bounty, fields.Pattern, fields.PatternIndex,
                fields.ForceBonus, fields.Block, fields.Burn, fields.Weak, fields.PocketedGold,
                fields.MantleBonusPer, fields.ActionsPerRound, fields.Gimmick, fields.Description,
                fields.Triggers);

            fields.RelicSchema = GetOrCreate<CardSchema>("Relic Schema", null);
            EnsureSchemaFields(fields.RelicSchema,
                fields.Description, fields.Effects, fields.Triggers, fields.Price);

            fields.CharmSchema = GetOrCreate<CardSchema>("Charm Schema", null);
            EnsureSchemaFields(fields.CharmSchema,
                fields.Description, fields.Effects, fields.MainAction, fields.Price);

            fields.HeroSchema = GetOrCreate<CardSchema>("Hero Schema", null);
            EnsureSchemaFields(fields.HeroSchema,
                fields.Description, fields.PocketSlots, fields.StartingDeck, fields.Triggers);

            return fields;
        }

        /// <summary>
        /// Adds any missing fields to a schema so re-running the generator upgrades old
        /// projects in place (cards re-sync against the widened schema afterwards).
        /// </summary>
        private static void EnsureSchemaFields(CardSchema schema, params CardFieldDefinition[] required)
        {
            bool changed = false;
            foreach (CardFieldDefinition field in required)
            {
                if (field != null && !schema.Fields.Contains(field))
                {
                    schema.Fields.Add(field);
                    changed = true;
                }
            }

            if (changed)
            {
                UnityEditor.EditorUtility.SetDirty(schema);
            }
        }

        /// <summary>
        /// Adds any missing fields to an existing metadata kind so re-running the generator
        /// upgrades old projects in place (entries re-sync their values afterwards).
        /// </summary>
        private static void EnsureKindFields(AStergio.OmniCard.Runtime.Cards.MetaData.MetadataKind kind,
            params CardFieldDefinition[] required)
        {
            bool changed = false;
            foreach (CardFieldDefinition field in required)
            {
                if (field != null && !kind.EntryFields.Contains(field))
                {
                    kind.EntryFields.Add(field);
                    changed = true;
                }
            }

            if (changed)
            {
                UnityEditor.EditorUtility.SetDirty(kind);
            }
        }

        private static Zones CreateZones()
        {
            return new Zones
            {
                Draw = Zone("Draw Pile", ZoneVisibility.Hidden),
                Discard = Zone("Discard Pile", ZoneVisibility.Public),
                Wound = Zone("Escrow", ZoneVisibility.Public),
                Pocket = Zone("Pocket", ZoneVisibility.Public),
                Exile = Zone("Exile Pile", ZoneVisibility.Public),
                Enemies = Zone("Enemies", ZoneVisibility.Public),
                Slain = Zone("Slain", ZoneVisibility.Public),
                Relics = Zone("Relics", ZoneVisibility.Public),
                Charms = Zone("Charms", ZoneVisibility.Public),
                Mantle = Zone("Mantle", ZoneVisibility.Public)
            };
        }
    }
}
