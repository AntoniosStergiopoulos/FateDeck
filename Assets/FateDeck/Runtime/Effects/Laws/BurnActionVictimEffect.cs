using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Laws
{
    /// <summary>
    /// The Flame law for offense and enemy contexts: the action's target suffers Burn.
    /// On your Strike the enemy burns; on an enemy attack you burn; on an enemy Brace
    /// the flame turns inward and the enemy burns itself.
    /// </summary>
    [Serializable]
    public class BurnActionVictimEffect : FateEffect, IActionLawPreview
    {
        public int Stacks = 2;

        public override string GetName() => "Burn Action Victim";

        public override string GetDescription() => $"the target suffers {Stacks} Burn";

        public string PreviewNote => $"Burn {Stacks}";

        public double PreviewForce(double force) => force;

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
                        session.AddStatus(action.TargetEnemy, StatusKind.Burn, Stacks);
                    }

                    break;

                case FateActionKind.EnemyBrace:
                case FateActionKind.EnemySpecial:
                    if (action.SourceEnemy != null)
                    {
                        session.AddStatus(action.SourceEnemy, StatusKind.Burn, Stacks);
                    }

                    break;

                case FateActionKind.EnemyAttack:
                    session.AddStatus(null, StatusKind.Burn, Stacks);
                    break;
            }
        }
    }
}
