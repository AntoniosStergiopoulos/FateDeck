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
            public MetadataEntry Tempest;
            public MetadataEntry TempestPlus;
            public MetadataEntry Serpent;
            public MetadataEntry SerpentPlus;
            public MetadataEntry Glass;
            public MetadataEntry Gloom;
            public MetadataEntry Key;
            public MetadataEntry Mirror;
            public MetadataEntry Anchor;
            public MetadataEntry Rust;
            public MetadataEntry Wisp;

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
                yield return Tempest;
                yield return TempestPlus;
                yield return Serpent;
                yield return SerpentPlus;
                yield return Glass;
                yield return Gloom;
                yield return Key;
                yield return Mirror;
                yield return Anchor;
                yield return Rust;
                yield return Wisp;
            }
        }

        /// <summary>A builder for one force entry: per-context law lists plus flags, assembled fluently.</summary>
        private sealed class LawSet
        {
            public readonly List<CardEffect> Offense = new List<CardEffect>();
            public readonly List<CardEffect> Defense = new List<CardEffect>();
            public readonly List<CardEffect> Enemy = new List<CardEffect>();
            public readonly List<CardEffect> Loot = new List<CardEffect>();
            public readonly List<CardEffect> Ritual = new List<CardEffect>();
        }

        private static Forces CreateForces(Fields fields)
        {
            var forces = new Forces();

            forces.Iron = ForceEntry(fields, "Iron", "I", new Color(0.62f, 0.66f, 0.72f),
                "The action's Force +2.", Uniform(() => new ModifyActionForceEffect { Delta = 2 }));
            forces.IronPlus = ForceEntry(fields, "Iron+", "I+", new Color(0.75f, 0.79f, 0.85f),
                "The action's Force +3.", Uniform(() => new ModifyActionForceEffect { Delta = 3 }));
            forces.Decay = ForceEntry(fields, "Decay", "D", new Color(0.26f, 0.70f, 0.55f),
                "The action's Force -2 (min 0).", Uniform(() => new ModifyActionForceEffect { Delta = -2 }));
            forces.DecayPlus = ForceEntry(fields, "Decay+", "D+", new Color(0.36f, 0.82f, 0.65f),
                "The action's Force -3 (min 0).", Uniform(() => new ModifyActionForceEffect { Delta = -3 }));
            forces.Fortune = ForceEntry(fields, "Fortune", "$", new Color(0.79f, 0.64f, 0.15f),
                "The action's owner banks 3 gold; Force +0.", Uniform(() => new BankGoldEffect { Amount = 3 }));
            forces.FortunePlus = ForceEntry(fields, "Fortune+", "$+", new Color(0.91f, 0.76f, 0.25f),
                "The action's owner banks 5 gold; Force +0.", Uniform(() => new BankGoldEffect { Amount = 5 }));
            forces.Echo = ForceEntry(fields, "Echo", "E", new Color(0.43f, 0.35f, 0.56f),
                "Flip one additional fate card; apply both laws (max 3 per action).",
                Uniform(() => new EchoFlipEffect()));
            forces.Void = ForceEntry(fields, "Void", "O", new Color(0.82f, 0.82f, 0.86f),
                "The action resolves at Force 0 with no effects; no other laws trigger.",
                Uniform(() => new NegateActionEffect()));
            forces.Flame = FlameEntry(fields, "Flame", "F", new Color(0.89f, 0.35f, 0.13f), 2,
                "The action's target suffers 2 Burn.");
            forces.FlamePlus = FlameEntry(fields, "Flame+", "F+", new Color(0.98f, 0.48f, 0.22f), 3,
                "The action's target suffers 3 Burn.");
            forces.Doom = DoomEntry(fields);

            forces.Tempest = ForceEntry(fields, "Tempest", "T", new Color(0.35f, 0.62f, 0.92f),
                "Lightning: your actions +1 Force and 2 damage arcs to the other enemies; "
                + "on enemy actions the storm bites the attacker for 2.",
                TempestLaws(1, 2));
            forces.TempestPlus = ForceEntry(fields, "Tempest+", "T+", new Color(0.50f, 0.74f, 1.00f),
                "Lightning: your actions +2 Force and 3 damage arcs to the other enemies; "
                + "on enemy actions the storm bites the attacker for 3.",
                TempestLaws(2, 3));

            forces.Serpent = ForceEntry(fields, "Serpent", "S", new Color(0.47f, 0.78f, 0.29f),
                "Venom: your Strike also Weakens the target 1; enemy actions -1 Force and the "
                + "venom turns on the attacker (Weak 1).",
                SerpentLaws(1));
            forces.SerpentPlus = ForceEntry(fields, "Serpent+", "S+", new Color(0.60f, 0.90f, 0.40f),
                "Venom: your Strike also Weakens the target 2; enemy actions -1 Force and the "
                + "venom turns on the attacker (Weak 2).",
                SerpentLaws(2));

            forces.Glass = GlassEntry(fields);

            forces.Gloom = ForceEntry(fields, "Gloom", "N", new Color(0.52f, 0.44f, 0.60f),
                "Mending dark: your actions -1 Force but a wound card returns to your deck; "
                + "enemy actions feed on it (+1 Force).",
                set =>
                {
                    set.Offense.Add(new ModifyActionForceEffect { Delta = -1 });
                    set.Offense.Add(new HealWoundsEffect { Count = 1 });
                    set.Defense.Add(new HealWoundsEffect { Count = 1 });
                    set.Enemy.Add(new ModifyActionForceEffect { Delta = 1 });
                    set.Loot.Add(new ModifyActionForceEffect { Delta = -2 });
                    set.Loot.Add(new HealWoundsEffect { Count = 1 });
                    set.Ritual.Add(new HealWoundsEffect { Count = 1 });
                });

            forces.Key = ForceEntry(fields, "Key", "K", new Color(0.76f, 0.60f, 0.34f),
                "Brass teeth: +1 Force on your actions, -1 on enemy actions; loot and ritual "
                + "flips mint a Key (opens locked chests).",
                set =>
                {
                    set.Offense.Add(new ModifyActionForceEffect { Delta = 1 });
                    set.Defense.Add(new ModifyActionForceEffect { Delta = 1 });
                    set.Enemy.Add(new ModifyActionForceEffect { Delta = -1 });
                    set.Loot.Add(new GainKeyEffect { Count = 1 });
                    set.Ritual.Add(new GainKeyEffect { Count = 1 });
                });

            forces.Mirror = ForceEntry(fields, "Mirror", "R", new Color(0.72f, 0.76f, 0.80f),
                "Repeats the law of the force that surfaced before it (in this context). "
                + "A Mirror with nothing to reflect polishes into +1 Force.",
                Uniform(() => new CopyLastForceEffect { FallbackDelta = 1 }));

            forces.Anchor = ForceEntry(fields, "Anchor", "A", new Color(0.30f, 0.42f, 0.58f),
                "Dead weight that holds: -1 on your offense, +3 on your Guard, and it drags "
                + "enemy blows down by 2.",
                set =>
                {
                    set.Offense.Add(new ModifyActionForceEffect { Delta = -1 });
                    set.Defense.Add(new ModifyActionForceEffect { Delta = 3 });
                    set.Enemy.Add(new ModifyActionForceEffect { Delta = -2 });
                    set.Loot.Add(new ModifyActionForceEffect { Delta = 1 });
                });

            forces.Rust = ForceEntry(fields, "Rust", "U", new Color(0.72f, 0.45f, 0.20f),
                "Corrosion: your offense +1 and strips 2 Block off the target; enemy actions "
                + "-1 Force but 1 of your Block flakes away.",
                set =>
                {
                    set.Offense.Add(new ModifyActionForceEffect { Delta = 1 });
                    set.Offense.Add(new CorrodeBlockEffect { Amount = 2 });
                    set.Defense.Add(new ModifyActionForceEffect { Delta = 2 });
                    set.Enemy.Add(new ModifyActionForceEffect { Delta = -1 });
                    set.Enemy.Add(new CorrodeBlockEffect { Amount = 1 });
                    set.Loot.Add(new ModifyActionForceEffect { Delta = 2 });
                    set.Ritual.Add(new ModifyActionForceEffect { Delta = 1 });
                });

            forces.Wisp = ForceEntry(fields, "Wisp", "W", new Color(0.78f, 0.86f, 0.72f),
                "A guiding light: +1 on your actions, -1 on enemy actions, and every flip "
                + "shows you the top of your deck.",
                set =>
                {
                    set.Offense.Add(new ModifyActionForceEffect { Delta = 1 });
                    set.Offense.Add(new ScryEffect { Count = 1, AllowReorder = false });
                    set.Defense.Add(new ModifyActionForceEffect { Delta = 1 });
                    set.Defense.Add(new ScryEffect { Count = 1, AllowReorder = false });
                    set.Enemy.Add(new ModifyActionForceEffect { Delta = -1 });
                    set.Enemy.Add(new ScryEffect { Count = 1, AllowReorder = false });
                    set.Loot.Add(new ModifyActionForceEffect { Delta = 2 });
                    set.Loot.Add(new ScryEffect { Count = 1, AllowReorder = false });
                    set.Ritual.Add(new ScryEffect { Count = 2, AllowReorder = false });
                });

            return forces;
        }

        private static System.Action<LawSet> Uniform(System.Func<CardEffect> make)
        {
            return set =>
            {
                set.Offense.Add(make());
                set.Defense.Add(make());
                set.Enemy.Add(make());
                set.Loot.Add(make());
                set.Ritual.Add(make());
            };
        }

        private static System.Action<LawSet> TempestLaws(int delta, double arc)
        {
            return set =>
            {
                set.Offense.Add(new ModifyActionForceEffect { Delta = delta });
                set.Offense.Add(new CleaveDamageEffect { Amount = arc });
                set.Defense.Add(new ModifyActionForceEffect { Delta = delta });
                set.Defense.Add(new CleaveDamageEffect { Amount = arc - 1 });
                set.Enemy.Add(new ModifyActionForceEffect { Delta = -1 });
                set.Enemy.Add(new CleaveDamageEffect { Amount = arc });
                set.Loot.Add(new ModifyActionForceEffect { Delta = delta + 2 });
                set.Ritual.Add(new ModifyActionForceEffect { Delta = delta + 1 });
            };
        }

        private static System.Action<LawSet> SerpentLaws(int stacks)
        {
            return set =>
            {
                set.Offense.Add(new ModifyActionForceEffect { Delta = 1 });
                set.Offense.Add(new WeakenActionVictimEffect { Stacks = stacks });
                set.Defense.Add(new ModifyActionForceEffect { Delta = 1 });
                set.Enemy.Add(new ModifyActionForceEffect { Delta = -1 });
                set.Enemy.Add(new WeakenActionVictimEffect { Stacks = stacks });
                set.Loot.Add(new ModifyActionForceEffect { Delta = 2 });
                set.Ritual.Add(new ModifyActionForceEffect { Delta = 1 });
            };
        }

        private static MetadataEntry ForceEntry(Fields fields, string name, string glyph, Color color,
            string description, System.Action<LawSet> laws)
        {
            MetadataEntry entry = GetOrCreate<MetadataEntry>(name, created =>
            {
                created.Kind = fields.ForceKind;
                created.SyncValuesWithKind();
                SetText(created, fields.ForceGlyph, glyph);
                SetColor(created, fields.ForceColor, color);
                SetText(created, fields.Description, description);
                var set = new LawSet();
                laws?.Invoke(set);
                EffectsOf(created, fields.LawOffense).AddRange(set.Offense);
                EffectsOf(created, fields.LawDefense).AddRange(set.Defense);
                EffectsOf(created, fields.LawEnemy).AddRange(set.Enemy);
                EffectsOf(created, fields.LawLoot).AddRange(set.Loot);
                EffectsOf(created, fields.LawRitual).AddRange(set.Ritual);
            }, "Forces");
            SyncEntry(entry);
            return entry;
        }

        private static MetadataEntry FlameEntry(Fields fields, string name, string glyph, Color color,
            int stacks, string description)
        {
            MetadataEntry entry = GetOrCreate<MetadataEntry>(name, created =>
            {
                created.Kind = fields.ForceKind;
                created.SyncValuesWithKind();
                SetText(created, fields.ForceGlyph, glyph);
                SetColor(created, fields.ForceColor, color);
                SetText(created, fields.Description, description);
                EffectsOf(created, fields.LawOffense).Add(new BurnActionVictimEffect { Stacks = stacks });
                EffectsOf(created, fields.LawDefense).Add(new GuardRetaliateBurnEffect { Stacks = stacks });
                EffectsOf(created, fields.LawEnemy).Add(new BurnActionVictimEffect { Stacks = stacks });
                EffectsOf(created, fields.LawLoot).Add(new FlameLootEffect { GoldBurned = 3 });
                EffectsOf(created, fields.LawRitual).Add(new BurnActionVictimEffect { Stacks = stacks });
            }, "Forces");
            SyncEntry(entry);
            return entry;
        }

        private static MetadataEntry DoomEntry(Fields fields)
        {
            MetadataEntry entry = GetOrCreate<MetadataEntry>("Doom", created =>
            {
                created.Kind = fields.ForceKind;
                created.SyncValuesWithKind();
                SetText(created, fields.ForceGlyph, "X");
                SetColor(created, fields.ForceColor, new Color(0.62f, 0.16f, 0.19f));
                SetText(created, fields.Description,
                    "The worst, always. Cannot be pocketed or stacked; milled Doom is exiled forever.");
                SetBoolean(created, fields.CannotPocket, true);
                SetBoolean(created, fields.CannotStack, true);
                SetBoolean(created, fields.ExileWhenMilled, true);
                EffectsOf(created, fields.LawOffense).Add(new SetActionForceEffect { Value = 0 });
                EffectsOf(created, fields.LawOffense).Add(new MillPlayerEffect { Count = 1 });
                EffectsOf(created, fields.LawDefense).Add(new SetActionForceEffect { Value = 0 });
                EffectsOf(created, fields.LawDefense).Add(new MillPlayerEffect { Count = 1 });
                EffectsOf(created, fields.LawEnemy).Add(new ModifyActionForceEffect { Delta = 3 });
                EffectsOf(created, fields.LawLoot).Add(new DoomLootEffect { Mill = 2 });
                EffectsOf(created, fields.LawRitual).Add(new MillPlayerEffect { Count = 1 });
            }, "Forces");
            SyncEntry(entry);
            return entry;
        }

        private static MetadataEntry GlassEntry(Fields fields)
        {
            MetadataEntry entry = GetOrCreate<MetadataEntry>("Glass", created =>
            {
                created.Kind = fields.ForceKind;
                created.SyncValuesWithKind();
                SetText(created, fields.ForceGlyph, "G");
                SetColor(created, fields.ForceColor, new Color(0.62f, 0.90f, 0.92f));
                SetText(created, fields.Description,
                    "One flash of brilliance: +4 Force on your actions, -3 on enemy actions, +8g loot - "
                    + "then it shatters (exiled after any flip, and when milled).");
                SetBoolean(created, fields.ExileWhenMilled, true);
                SetBoolean(created, fields.ExileAfterFlip, true);
                EffectsOf(created, fields.LawOffense).Add(new ModifyActionForceEffect { Delta = 4 });
                EffectsOf(created, fields.LawDefense).Add(new ModifyActionForceEffect { Delta = 4 });
                EffectsOf(created, fields.LawEnemy).Add(new ModifyActionForceEffect { Delta = -3 });
                EffectsOf(created, fields.LawLoot).Add(new ModifyActionForceEffect { Delta = 8 });
                EffectsOf(created, fields.LawRitual).Add(new ModifyActionForceEffect { Delta = 3 });
            }, "Forces");
            SyncEntry(entry);
            return entry;
        }

        /// <summary>Re-syncs an existing entry with its (possibly upgraded) kind so new fields appear.</summary>
        private static void SyncEntry(MetadataEntry entry)
        {
            entry.SyncValuesWithKind();
            UnityEditor.EditorUtility.SetDirty(entry);
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
