using System;
using AStergio.OmniCard.Runtime.Cards.Conditions;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Conditions
{
    /// <summary>GDD atom <c>OncePerCombat</c>: passes the first time per fight, then locks.</summary>
    [Serializable]
    public class OncePerCombatCondition : Condition
    {
        public override string GetDescription() => "once per combat";

        public override bool Evaluate(EffectContext context)
        {
            if (!(context.Game is IFateSession session) || session.Combat == null)
            {
                return false;
            }

            return session.Combat.ClaimOnce(this);
        }
    }
}
