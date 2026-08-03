using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;

namespace FateDeck.Runtime.Core
{
    /// <summary>
    /// One declared action moving through the fate pipeline: deterministic base force,
    /// mutated by the laws of whatever fate cards flip for it.
    /// </summary>
    public sealed class FateAction
    {
        public FateAction(FateActionKind kind, string name, double baseForce, bool flipsFate)
        {
            Kind = kind;
            Name = name;
            BaseForce = baseForce;
            Force = baseForce;
            FlipsFate = flipsFate;
        }

        public FateActionKind Kind { get; }

        public string Name { get; }

        public double BaseForce { get; }

        public double Force { get; set; }

        public bool FlipsFate { get; }

        /// <summary>The enemy performing this action; null when the player owns it.</summary>
        public CardInstance SourceEnemy { get; set; }

        /// <summary>The enemy targeted by a player Strike or charm; null otherwise.</summary>
        public CardInstance TargetEnemy { get; set; }

        /// <summary>Set by the Void law: the action resolves at force zero with no effects.</summary>
        public bool Negated { get; set; }

        /// <summary>How many fate cards have been flipped for this action (Echo cap).</summary>
        public int FlipCount { get; set; }

        /// <summary>Pre-flip bonus folded into the declaration (Rustheart); survives banking.</summary>
        public double DeclaredBonus { get; set; }

        /// <summary>Force entries whose laws were applied to this action, in flip order.</summary>
        public List<MetadataEntry> AppliedForces { get; } = new List<MetadataEntry>();

        /// <summary>True when a pocketed card replaced the flip for this action.</summary>
        public bool ReplacedByPocket { get; set; }

        /// <summary>Set by the Flame loot law: this flip opens a locked chest.</summary>
        public bool OpensLock { get; set; }

        /// <summary>Set by the Doom loot law: the chest is a trap and pays nothing.</summary>
        public bool NoLoot { get; set; }

        /// <summary>Special-step effects resolved at commit (enemy gimmicks, ritual outcomes).</summary>
        public IReadOnlyList<CardEffect> SpecialEffects { get; set; }

        /// <summary>Loot only: the chest is locked and pays nothing unless a key or Flame opens it.</summary>
        public bool LockedChest { get; set; }

        /// <summary>Loot only: a key was spent, so the lock is already open.</summary>
        public bool KeyUsed { get; set; }

        public bool IsPlayerAction => Kind == FateActionKind.Strike || Kind == FateActionKind.Guard;

        public LawContext Context
        {
            get
            {
                switch (Kind)
                {
                    case FateActionKind.Strike:
                        return LawContext.PlayerOffense;
                    case FateActionKind.Guard:
                        return LawContext.PlayerDefense;
                    case FateActionKind.EnemyAttack:
                    case FateActionKind.EnemyBrace:
                    case FateActionKind.EnemySpecial:
                        return LawContext.EnemyAction;
                    case FateActionKind.Loot:
                        return LawContext.Loot;
                    default:
                        return LawContext.Ritual;
                }
            }
        }
    }
}
