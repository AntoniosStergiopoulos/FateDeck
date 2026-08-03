using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    public enum StatusTarget
    {
        Player,
        SelectedEnemy,
        AllEnemies,
        SourceEnemy
    }

    /// <summary>Applies Burn or Weak stacks to a chosen side of the table.</summary>
    [Serializable]
    public class ApplyStatusEffect : FateEffect
    {
        public StatusKind Status = StatusKind.Burn;
        public int Stacks = 2;
        public StatusTarget Target = StatusTarget.SelectedEnemy;

        public override string GetName() => "Apply Status";

        public override string GetDescription() => $"apply {Stacks} {Status} to {TargetLabel()}";

        private string TargetLabel()
        {
            switch (Target)
            {
                case StatusTarget.Player: return "you";
                case StatusTarget.AllEnemies: return "all enemies";
                case StatusTarget.SourceEnemy: return "itself";
                default: return "the target";
            }
        }

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            switch (Target)
            {
                case StatusTarget.Player:
                    session.AddStatus(null, Status, Stacks);
                    break;

                case StatusTarget.SourceEnemy:
                    session.AddStatus(session.CurrentAction?.SourceEnemy ?? context.Source, Status, Stacks);
                    break;

                case StatusTarget.AllEnemies:
                    if (session.Combat != null)
                    {
                        foreach (CardInstance enemy in session.Combat.EnemiesSnapshot())
                        {
                            session.AddStatus(enemy, Status, Stacks);
                        }
                    }

                    break;

                default:
                    CardInstance target = session.CurrentAction?.TargetEnemy ?? session.Combat?.SelectedOrFirstEnemy();
                    if (target != null)
                    {
                        session.AddStatus(target, Status, Stacks);
                    }

                    break;
            }
        }
    }
}
