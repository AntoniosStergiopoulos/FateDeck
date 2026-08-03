using AStergio.OmniCard.Runtime.Cards.Data;
using FateDeck.Runtime.Combat;
using FateDeck.Runtime.Effects.Enemies;
using FateDeck.Runtime.Effects.Gameplay;
using FateDeck.Runtime.Triggers;

namespace FateDeck.Editor
{
    public static partial class FateDeckContentGenerator
    {
        private static CardDefinition Enemy(Fields fields, string name, double hp, int bounty,
            string gimmick, System.Action<CardDefinition> configure)
        {
            return Card(fields.EnemySchema, name, card =>
            {
                SetNumber(card, fields.Hp, hp);
                SetNumber(card, fields.MaxHp, hp);
                SetNumber(card, fields.Bounty, bounty);
                SetText(card, fields.Gimmick, gimmick ?? string.Empty);
                configure?.Invoke(card);
            }, "Enemies");
        }

        private static EnemyActionSpec Attack(string name, double force)
        {
            return new EnemyActionSpec { Name = name, Kind = EnemyActionKind.Attack, Force = force, FlipsFate = true };
        }

        private static EnemyActionSpec Brace(string name, double force)
        {
            return new EnemyActionSpec { Name = name, Kind = EnemyActionKind.Brace, Force = force, FlipsFate = true };
        }

        private static EnemyActionSpec Special(string name, bool flipsFate,
            params AStergio.OmniCard.Runtime.Cards.Effects.Base.CardEffect[] effects)
        {
            var spec = new EnemyActionSpec { Name = name, Kind = EnemyActionKind.Special, Force = 0, FlipsFate = flipsFate };
            spec.Effects.AddRange(effects);
            return spec;
        }

        private static CardDefinition TinderImp(Fields fields)
        {
            return Enemy(fields, "Tinder Imp", 3, 3, null, card =>
            {
                SetText(card, fields.Description, "The tutorial punching bag.");
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 2));
            });
        }

        private static CardDefinition Scrapling(Fields fields)
        {
            return Enemy(fields, "Scrapling", 5, 4, null, card =>
            {
                SetText(card, fields.Description, "Teaches intent reading and hit timing.");
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 2));
                PatternOf(card, fields.Pattern).Add(Brace("Brace", 2));
            });
        }

        private static CardDefinition CardsharpRat(Fields fields)
        {
            return Enemy(fields, "Cardsharp Rat", 5, 5,
                "Rig: its paws reorder your top two cards - the worse one surfaces first.", card =>
            {
                SetText(card, fields.Description, "Order is attackable. Scry and Pocket are the answers.");
                PatternOf(card, fields.Pattern).Add(Special("Rig", false, new RigTopEffect { PeekCount = 2 }));
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 3));
            });
        }

        private static CardDefinition TinderHound(Fields fields)
        {
            return Enemy(fields, "Tinder Hound", 6, 5,
                "Howl: it grows angrier - +1 Force permanently.", card =>
            {
                SetText(card, fields.Description, "Race it, or learn composition-aware guarding.");
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 2));
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 2));
                PatternOf(card, fields.Pattern).Add(Special("Howl", false, new HowlEffect { Delta = 1 }));
            });
        }

        private static CardDefinition WaxCherub(Fields fields)
        {
            return Enemy(fields, "Wax Cherub", 4, 4,
                "Cycler: two actions per turn burn your reshuffle clock.", card =>
            {
                SetText(card, fields.Description, "Every flip it forces is a tooth for the House.");
                SetNumber(card, fields.ActionsPerRound, 2);
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 1));
            });
        }

        private static CardDefinition TollCollector(Fields fields)
        {
            return Enemy(fields, "The Toll Collector", 16, 10,
                "Tithe: mills your top card, bypassing Block, and it pockets the toll.", card =>
            {
                SetText(card, fields.Description, "Some damage can't be blocked - only raced.");
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 3));
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 3));
                PatternOf(card, fields.Pattern).Add(Special("Tithe", false,
                    new TitheEffect { Mill = 1, GoldPocketed = 1 }));
            });
        }

        private static CardDefinition TheCollector(Fields fields)
        {
            return Enemy(fields, "THE COLLECTOR", 30, 25,
                "Confiscate: it holds all copies of your most-numerous force in its Mantle; "
                + "its attacks gain +1 Force per 3 cards held. Only the draw pile is read.", card =>
            {
                SetText(card, fields.Description,
                    "A vast pale hand in a sleeve of ledgers. It doesn't want you dead; it wants your assets.");
                SetNumber(card, fields.MantleBonusPer, 3);
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 4));
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 4));
                PatternOf(card, fields.Pattern).Add(Special("Appraise", false, new ConfiscateEffect()));

                var opening = new OnCombatStartTrigger();
                opening.Effects.Add(new ConfiscateEffect());
                TriggersOf(card, fields.Triggers).Add(opening);

                var death = new OnEnemyDeathTrigger { SelfOnly = true };
                death.Effects.Add(new ReturnMantleEffect());
                TriggersOf(card, fields.Triggers).Add(death);
            });
        }
    }
}
