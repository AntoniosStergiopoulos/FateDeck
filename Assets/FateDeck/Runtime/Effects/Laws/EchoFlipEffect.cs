using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Laws
{
    /// <summary>The Echo law: flip one additional fate card and apply both laws (hard-capped per action).</summary>
    [Serializable]
    public class EchoFlipEffect : FateEffect, IActionLawPreview
    {
        public override string GetName() => "Echo Flip";

        public override string GetDescription() => "flip one additional fate card";

        public string PreviewNote => "flip again";

        public double PreviewForce(double force) => force;

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            session.QueueEchoFlip();
        }
    }
}
