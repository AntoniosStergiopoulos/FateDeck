using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Laws
{
    /// <summary>
    /// The Tempest law: lightning arcs off the action. On your offense every OTHER enemy
    /// takes damage; on an enemy action the storm bites the attacker itself.
    /// </summary>
    [Serializable]
    public class CleaveDamageEffect : FateEffect, IActionLawPreview
    {
        public double Amount = 2;

        public override string GetName() => "Cleave";

        public override string GetDescription() => $"lightning: {Amount:0.##} damage arcs to the others";

        public string PreviewNote => $"arc {Amount:0.##} to others";

        public double PreviewForce(double force) => force;

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            if (session.Combat == null)
            {
                return;
            }

            FateAction action = session.CurrentAction;
            if (action != null && action.SourceEnemy != null)
            {
                session.Combat.DamageEnemy(action.SourceEnemy, Amount);
                return;
            }

            CardInstance primary = action?.TargetEnemy;
            foreach (CardInstance enemy in session.Combat.EnemiesSnapshot())
            {
                if (enemy != primary)
                {
                    session.Combat.DamageEnemy(enemy, Amount);
                }
            }
        }
    }
}
