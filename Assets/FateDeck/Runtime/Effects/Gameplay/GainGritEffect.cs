using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    /// <summary>Banks Grit directly (hero passives, events, charms). Capped by GritMax.</summary>
    [Serializable]
    public class GainGritEffect : FateEffect, IActionLawPreview
    {
        public int Amount = 1;

        public override string GetName() => "Gain Grit";

        public override string GetDescription() => Amount == 1 ? "gain 1 Grit" : $"gain {Amount} Grit";

        public string PreviewNote => $"+{Amount} Grit";

        public double PreviewForce(double force) => force;

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            session.AddGrit(Amount);
        }
    }
}
