using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    /// <summary>Grants bonus force to the player's next declared action (Rustheart-style rewards).</summary>
    [Serializable]
    public class NextActionBonusEffect : FateEffect
    {
        public double Delta = 2;

        public override string GetName() => "Next Action Bonus";

        public override string GetDescription() => $"+{Delta:0.##} Force to your next action";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            session.AddNextPlayerActionBonus(Delta);
        }
    }
}
