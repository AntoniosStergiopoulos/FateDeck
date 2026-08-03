using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.Game.Zones;
using AStergio.OmniCard.Runtime.Cards.Layout;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Run;
using UnityEngine;

namespace FateDeck.Runtime.Core
{
    /// <summary>
    /// The single dependency of every Fate Deck controller: typed references to all
    /// generated OmniCard content (fields, forces, schemas, zones, cards, rooms, layouts).
    /// </summary>
    public class FateContentCatalog : ScriptableObject
    {
        [Header("Rules")]
        public FateRulesDefinition Rules;

        [Header("Shared fields")]
        public CardFieldDefinition ForceField;
        public CardFieldDefinition DescriptionField;
        public CardFieldDefinition ColorField;
        public CardFieldDefinition GlyphField;

        [Header("Force entry fields (on the Force metadata kind)")]
        public CardFieldDefinition LawOffenseField;
        public CardFieldDefinition LawDefenseField;
        public CardFieldDefinition LawEnemyField;
        public CardFieldDefinition LawLootField;
        public CardFieldDefinition LawRitualField;
        public CardFieldDefinition CannotPocketField;
        public CardFieldDefinition CannotStackField;
        public CardFieldDefinition ExileWhenMilledField;
        public CardFieldDefinition ForceColorField;
        public CardFieldDefinition ForceGlyphField;

        [Header("Enemy fields")]
        public CardFieldDefinition HpField;
        public CardFieldDefinition MaxHpField;
        public CardFieldDefinition BountyField;
        public CardFieldDefinition PatternField;
        public CardFieldDefinition PatternIndexField;
        public CardFieldDefinition ForceBonusField;
        public CardFieldDefinition BlockField;
        public CardFieldDefinition GimmickField;
        public CardFieldDefinition BurnField;
        public CardFieldDefinition WeakField;
        public CardFieldDefinition PocketedGoldField;
        public CardFieldDefinition MantleBonusPerField;
        public CardFieldDefinition ActionsPerRoundField;

        [Header("Item fields")]
        public CardFieldDefinition EffectsField;
        public CardFieldDefinition TriggersField;
        public CardFieldDefinition PriceField;
        public CardFieldDefinition MainActionField;

        [Header("Hero fields")]
        public CardFieldDefinition PocketSlotsField;
        public CardFieldDefinition StartingDeckField;

        [Header("Metadata")]
        public MetadataKind ForceKind;

        [Header("Force entries")]
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

        [Header("Schemas")]
        public CardSchema FateCardSchema;
        public CardSchema EnemySchema;
        public CardSchema RelicSchema;
        public CardSchema CharmSchema;
        public CardSchema HeroSchema;

        [Header("Fate card definitions (one per force entry)")]
        public List<CardDefinition> FateCards = new List<CardDefinition>();

        [Header("Zones")]
        public ZoneDefinition DrawPile;
        public ZoneDefinition DiscardPile;
        public ZoneDefinition WoundRow;
        public ZoneDefinition Pocket;
        public ZoneDefinition ExilePile;
        public ZoneDefinition Enemies;
        public ZoneDefinition Slain;
        public ZoneDefinition Relics;
        public ZoneDefinition Charms;
        public ZoneDefinition Mantle;

        [Header("Layouts")]
        public CardLayout FateCardLayout;
        public CardLayout EnemyLayout;
        public CardLayout ItemLayout;

        [Header("UI Toolkit")]
        public UnityEngine.UIElements.PanelSettings Panel;

        [Header("Heroes")]
        public List<CardDefinition> Heroes = new List<CardDefinition>();

        [Header("Item pools")]
        public List<CardDefinition> CharmPool = new List<CardDefinition>();
        public List<CardDefinition> RelicPool = new List<CardDefinition>();

        [Header("Biome 1 content")]
        public List<RoomDefinition> Biome1Rooms = new List<RoomDefinition>();
        public FightRoomDefinition Biome1Opening;
        public FightRoomDefinition Biome1Elite;
        public BossRoomDefinition Biome1Boss;
        public ShrineRoomDefinition ForgeShrine;

        /// <summary>Finds the fate card definition whose Force reference is the given entry.</summary>
        public CardDefinition FateCardFor(MetadataEntry force)
        {
            for (int i = 0; i < FateCards.Count; i++)
            {
                CardDefinition card = FateCards[i];
                if (card != null && ReferenceEquals(card.GetObject(ForceField), force))
                {
                    return card;
                }
            }

            return null;
        }

        /// <summary>The force entry referenced by a fate card instance, or null for non-fate cards.</summary>
        public MetadataEntry ForceOf(AStergio.OmniCard.Runtime.Cards.Instances.CardInstance card)
        {
            if (card == null)
            {
                return null;
            }

            return card.Definition.GetObject(ForceField) as MetadataEntry;
        }

        /// <summary>The + tier of a basic force, or null (Echo, Void and Doom cannot upgrade).</summary>
        public MetadataEntry PlusVersionOf(MetadataEntry force)
        {
            if (force == Iron) return IronPlus;
            if (force == Flame) return FlamePlus;
            if (force == Decay) return DecayPlus;
            if (force == Fortune) return FortunePlus;
            return null;
        }

        public bool IsBasicForce(MetadataEntry force)
        {
            return force == Iron || force == Flame || force == Decay || force == Fortune;
        }

        public CardFieldDefinition LawFieldFor(LawContext context)
        {
            switch (context)
            {
                case LawContext.PlayerOffense: return LawOffenseField;
                case LawContext.PlayerDefense: return LawDefenseField;
                case LawContext.EnemyAction: return LawEnemyField;
                case LawContext.Loot: return LawLootField;
                default: return LawRitualField;
            }
        }
    }
}
