using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    [Serializable]
    public class GainBlockEffect : FateEffect
    {
        public double Amount = 2;

        public override string GetName() => "Gain Block";

        public override string GetDescription() => $"gain {Amount:0.##} Block";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            session.AddPlayerBlock(Amount);
        }
    }
}
