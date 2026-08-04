using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Laws
{
    /// <summary>
    /// The Serpent law: venom soaks whoever the action targets. On your Strike the enemy
    /// weakens; on an enemy attack the venom turns and weakens the attacker instead.
    /// </summary>
    [Serializable]
    public class WeakenActionVictimEffect : FateEffect, IActionLawPreview, IContextDescribed
    {
        public int Stacks = 1;

        public override string GetName() => "Weaken Action Victim";

        public override string GetDescription() => $"venom: the victim suffers {Stacks} Weak";

        public string PreviewNote => $"Weak {Stacks}";

        public double PreviewForce(double force) => force;

        public string DescribeFor(LawContext context)
        {
            switch (context)
            {
                case LawContext.PlayerOffense: return $"your target suffers {Stacks} Weak";
                case LawContext.EnemyAction: return $"the venom turns: the ATTACKER suffers {Stacks} Weak";
                default: return "the venom finds no one";
            }
        }

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            FateAction action = session.CurrentAction;
            if (action == null)
            {
                return;
            }

            switch (action.Kind)
            {
                case FateActionKind.Strike:
                    if (action.TargetEnemy != null)
                    {
                        session.AddStatus(action.TargetEnemy, StatusKind.Weak, Stacks);
                    }

                    break;

                case FateActionKind.EnemyAttack:
                case FateActionKind.EnemyBrace:
                case FateActionKind.EnemySpecial:
                    if (action.SourceEnemy != null)
                    {
                        session.AddStatus(action.SourceEnemy, StatusKind.Weak, Stacks);
                    }

                    break;
            }
        }
    }
}
