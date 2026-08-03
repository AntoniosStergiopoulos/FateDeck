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

        private static CardDefinition DustUsurer(Fields fields)
        {
            return Enemy(fields, "Dust Usurer", 7, 6,
                "Skim: mills your top card past any Block.", card =>
            {
                SetText(card, fields.Description, "A minor creditor with major appetites.");
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 2));
                PatternOf(card, fields.Pattern).Add(Special("Skim", false,
                    new TitheEffect { Mill = 1, GoldPocketed = 0 }));
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 3));
            });
        }

        private static CardDefinition ChandlerGrub(Fields fields)
        {
            return Enemy(fields, "Chandler Grub", 5, 4,
                "Winds up, then swings hard. Guard on the wind-up.", card =>
            {
                SetText(card, fields.Description, "A fat white worm dipped in wax.");
                PatternOf(card, fields.Pattern).Add(Brace("Wind Up", 2));
                PatternOf(card, fields.Pattern).Add(Attack("Slam", 4));
            });
        }

        private static CardDefinition InkwellLeech(Fields fields)
        {
            return Enemy(fields, "Inkwell Leech", 6, 5,
                "Drain: heals itself 2 every third action.", card =>
            {
                SetText(card, fields.Description, "It drinks the ledger's red ink. And yours.");
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 2));
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 2));
                PatternOf(card, fields.Pattern).Add(Special("Drain", false,
                    new EnemySelfEffect { Change = EnemySelfChange.Heal, Amount = 2 }));
            });
        }

        private static CardDefinition PorcelainDoll(Fields fields)
        {
            return Enemy(fields, "Porcelain Doll", 4, 5,
                "Shatter: when it dies, the shards mill 1 of your cards.", card =>
            {
                SetText(card, fields.Description, "Kill it from a distance you don't have.");
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 3));
                PatternOf(card, fields.Pattern).Add(Brace("Pose", 2));

                var death = new OnEnemyDeathTrigger { SelfOnly = true };
                death.Effects.Add(new MillPlayerEffect { Count = 1 });
                TriggersOf(card, fields.Triggers).Add(death);
            });
        }

        private static CardDefinition LedgerWisp(Fields fields)
        {
            return Enemy(fields, "Ledger Wisp", 3, 4,
                "Cycler: two quick actions per turn; Audit rigs your top cards.", card =>
            {
                SetText(card, fields.Description, "A page of debts that learned to fly.");
                SetNumber(card, fields.ActionsPerRound, 2);
                PatternOf(card, fields.Pattern).Add(Attack("Papercut", 1));
                PatternOf(card, fields.Pattern).Add(Special("Audit", false, new RigTopEffect { PeekCount = 2 }));
            });
        }

        private static CardDefinition MatchSergeant(Fields fields)
        {
            return Enemy(fields, "Match Sergeant", 8, 7,
                "Kindle: sets you alight - 1 Burn (mills a card at round end).", card =>
            {
                SetText(card, fields.Description, "It inspects your deck for fire-code violations.");
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 3));
                PatternOf(card, fields.Pattern).Add(Special("Kindle", false,
                    new ApplyStatusEffect { Status = FateDeck.Runtime.Core.StatusKind.Burn, Stacks = 1,
                        Target = StatusTarget.Player }));
                PatternOf(card, fields.Pattern).Add(Brace("Brace", 3));
            });
        }

        private static CardDefinition MothBroker(Fields fields)
        {
            return Enemy(fields, "Moth Broker", 6, 5,
                "Dust: its wings Weaken your next action by 2 Force.", card =>
            {
                SetText(card, fields.Description, "It trades in weakness futures.");
                PatternOf(card, fields.Pattern).Add(Special("Dust", false,
                    new ApplyStatusEffect { Status = FateDeck.Runtime.Core.StatusKind.Weak, Stacks = 1,
                        Target = StatusTarget.Player }));
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 3));
            });
        }

        private static CardDefinition GildedBeetle(Fields fields)
        {
            return Enemy(fields, "Gilded Beetle", 9, 14,
                "Hoard: slow, armored, and worth a small fortune.", card =>
            {
                SetText(card, fields.Description, "A walking coin purse with opinions.");
                PatternOf(card, fields.Pattern).Add(Brace("Shell", 4));
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 2));
            });
        }

        private static CardDefinition PaperGolem(Fields fields)
        {
            return Enemy(fields, "Paper Golem", 12, 9,
                "Slow and enormous: guard the Crush or race it down.", card =>
            {
                SetText(card, fields.Description, "Ten thousand unpaid invoices, folded into a fist.");
                PatternOf(card, fields.Pattern).Add(Brace("Fold", 3));
                PatternOf(card, fields.Pattern).Add(Attack("Crush", 5));
            });
        }

        private static CardDefinition CandleSmotherer(Fields fields)
        {
            return Enemy(fields, "Candle Smotherer", 10, 8,
                "Snuff: pinches out all of your Block before its follow-up.", card =>
            {
                SetText(card, fields.Description, "Two damp fingers the size of oars.");
                PatternOf(card, fields.Pattern).Add(Special("Snuff", false,
                    new FateDeck.Runtime.Effects.Laws.CorrodeBlockEffect { Amount = 99 }));
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 4));
            });
        }

        private static CardDefinition TheUnderwriter(Fields fields)
        {
            return Enemy(fields, "The Underwriter", 18, 12,
                "Premium: the next reshuffle adds +1 extra Doom. It taxes your clock itself.", card =>
            {
                SetText(card, fields.Description, "Elite. It doesn't hurt you; it re-prices you.");
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 4));
                PatternOf(card, fields.Pattern).Add(Special("Premium", false,
                    new ModifyReshuffleTaxEffect { Delta = 1, NextReshuffleOnly = true }));
                PatternOf(card, fields.Pattern).Add(Brace("Brace", 3));
            });
        }

        private static CardDefinition TheNotary(Fields fields)
        {
            return Enemy(fields, "The Notary", 15, 11,
                "Seal: stamps you Weak 1 and itself +1 Block per stamp.", card =>
            {
                SetText(card, fields.Description, "Elite. Every stamp makes you smaller on paper.");
                PatternOf(card, fields.Pattern).Add(Special("Seal", false,
                    new ApplyStatusEffect { Status = FateDeck.Runtime.Core.StatusKind.Weak, Stacks = 1,
                        Target = StatusTarget.Player },
                    new EnemySelfEffect { Change = EnemySelfChange.GainBlock, Amount = 1 }));
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 3));
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 4));
            });
        }

        private static CardDefinition TheCollector(Fields fields)
        {
            return Enemy(fields, "THE COLLECTOR", 32, 25,
                "Confiscate: it appraises up to 3 copies of your most-numerous force into its Mantle; "
                + "attacks gain +1 Force per 3 held. Hits of 6+ shake a card loose. Only the draw "
                + "pile is read - discard, pocket and wounds are invisible to it.", card =>
            {
                SetText(card, fields.Description,
                    "A vast pale hand in a sleeve of ledgers. It doesn't want you dead; it wants your assets.");
                SetNumber(card, fields.MantleBonusPer, 3);
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 4));
                PatternOf(card, fields.Pattern).Add(Attack("Attack", 4));
                PatternOf(card, fields.Pattern).Add(Special("Appraise", false,
                    new ConfiscateEffect { MaxTaken = 3 }));

                var opening = new OnCombatStartTrigger();
                opening.Effects.Add(new ConfiscateEffect { MaxTaken = 3 });
                TriggersOf(card, fields.Triggers).Add(opening);

                var death = new OnEnemyDeathTrigger { SelfOnly = true };
                death.Effects.Add(new ReturnMantleEffect());
                TriggersOf(card, fields.Triggers).Add(death);
            });
        }
    }
}
