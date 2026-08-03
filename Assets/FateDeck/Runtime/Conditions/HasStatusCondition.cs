using System;
using AStergio.OmniCard.Runtime.Cards.Conditions;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Conditions
{
    /// <summary>GDD atom <c>HasStatus</c> for the player side.</summary>
    [Serializable]
    public class HasStatusCondition : Condition
    {
        public StatusKind Status = StatusKind.Burn;
        public int MinimumStacks = 1;

        public override string GetDescription() => $"you have {MinimumStacks}+ {Status}";

        public override bool Evaluate(EffectContext context)
        {
            return context.Game is IFateSession session
                && session.GetStatus(null, Status) >= MinimumStacks;
        }
    }
}
