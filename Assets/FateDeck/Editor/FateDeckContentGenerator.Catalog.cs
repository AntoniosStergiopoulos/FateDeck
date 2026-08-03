using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using FateDeck.Runtime.Core;

namespace FateDeck.Editor
{
    public static partial class FateDeckContentGenerator
    {
        private static void FillCatalog(FateContentCatalog catalog, FateRulesDefinition rules, Fields fields,
            Forces forces, List<CardDefinition> fateCards, Zones zones, Layouts layouts, Items items,
            CardDefinition gambler, Rooms rooms)
        {
            catalog.Rules = rules;

            catalog.ForceField = fields.Force;
            catalog.DescriptionField = fields.Description;
            catalog.ColorField = fields.ForceColor;
            catalog.GlyphField = fields.ForceGlyph;
            catalog.LawOffenseField = fields.LawOffense;
            catalog.LawDefenseField = fields.LawDefense;
            catalog.LawEnemyField = fields.LawEnemy;
            catalog.LawLootField = fields.LawLoot;
            catalog.LawRitualField = fields.LawRitual;
            catalog.CannotPocketField = fields.CannotPocket;
            catalog.CannotStackField = fields.CannotStack;
            catalog.ExileWhenMilledField = fields.ExileWhenMilled;
            catalog.ForceColorField = fields.ForceColor;
            catalog.ForceGlyphField = fields.ForceGlyph;

            catalog.HpField = fields.Hp;
            catalog.MaxHpField = fields.MaxHp;
            catalog.BountyField = fields.Bounty;
            catalog.PatternField = fields.Pattern;
            catalog.PatternIndexField = fields.PatternIndex;
            catalog.ForceBonusField = fields.ForceBonus;
            catalog.BlockField = fields.Block;
            catalog.GimmickField = fields.Gimmick;
            catalog.BurnField = fields.Burn;
            catalog.WeakField = fields.Weak;
            catalog.PocketedGoldField = fields.PocketedGold;
            catalog.MantleBonusPerField = fields.MantleBonusPer;
            catalog.ActionsPerRoundField = fields.ActionsPerRound;

            catalog.EffectsField = fields.Effects;
            catalog.TriggersField = fields.Triggers;
            catalog.PriceField = fields.Price;
            catalog.MainActionField = fields.MainAction;
            catalog.PocketSlotsField = fields.PocketSlots;
            catalog.StartingDeckField = fields.StartingDeck;

            catalog.ForceKind = fields.ForceKind;
            catalog.Iron = forces.Iron;
            catalog.IronPlus = forces.IronPlus;
            catalog.Flame = forces.Flame;
            catalog.FlamePlus = forces.FlamePlus;
            catalog.Decay = forces.Decay;
            catalog.DecayPlus = forces.DecayPlus;
            catalog.Fortune = forces.Fortune;
            catalog.FortunePlus = forces.FortunePlus;
            catalog.Echo = forces.Echo;
            catalog.Void = forces.Void;
            catalog.Doom = forces.Doom;

            catalog.FateCardSchema = fields.FateCardSchema;
            catalog.EnemySchema = fields.EnemySchema;
            catalog.RelicSchema = fields.RelicSchema;
            catalog.CharmSchema = fields.CharmSchema;
            catalog.HeroSchema = fields.HeroSchema;

            catalog.FateCards.Clear();
            catalog.FateCards.AddRange(fateCards);

            catalog.DrawPile = zones.Draw;
            catalog.DiscardPile = zones.Discard;
            catalog.WoundRow = zones.Wound;
            catalog.Pocket = zones.Pocket;
            catalog.ExilePile = zones.Exile;
            catalog.Enemies = zones.Enemies;
            catalog.Slain = zones.Slain;
            catalog.Relics = zones.Relics;
            catalog.Charms = zones.Charms;
            catalog.Mantle = zones.Mantle;

            catalog.FateCardLayout = layouts.FateCard;
            catalog.EnemyLayout = layouts.Enemy;
            catalog.ItemLayout = layouts.Item;

            catalog.Heroes.Clear();
            catalog.Heroes.Add(gambler);

            catalog.CharmPool.Clear();
            catalog.CharmPool.AddRange(items.Charms);
            catalog.RelicPool.Clear();
            catalog.RelicPool.AddRange(items.Relics);

            catalog.Biome1Rooms.Clear();
            catalog.Biome1Rooms.AddRange(rooms.Pool);
            catalog.Biome1Opening = rooms.Opening;
            catalog.Biome1Elite = rooms.Elite;
            catalog.Biome1Boss = rooms.Boss;
            catalog.ForgeShrine = rooms.Forge;
        }
    }
}
