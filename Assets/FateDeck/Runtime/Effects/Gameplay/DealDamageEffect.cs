using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    /// <summary>Direct damage with no fate flip (Firecracker). Hits the selected enemy or everyone.</summary>
    [Serializable]
    public class DealDamageEffect : FateEffect
    {
        public double Amount = 3;
        public bool AllEnemies;

        public override string GetName() => "Deal Damage";

        public override string GetDescription() =>
            AllEnemies ? $"deal {Amount:0.##} damage to all enemies. No flip" : $"deal {Amount:0.##} damage. No flip";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            if (session.Combat == null)
            {
                return;
            }

            if (AllEnemies)
            {
                foreach (CardInstance enemy in session.Combat.EnemiesSnapshot())
                {
                    session.Combat.DamageEnemy(enemy, Amount);
                }

                return;
            }

            CardInstance target = session.Combat.SelectedOrFirstEnemy();
            if (target != null)
            {
                session.Combat.DamageEnemy(target, Amount);
            }
        }
    }
}
