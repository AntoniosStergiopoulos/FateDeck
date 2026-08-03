using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    [Serializable]
    public class GainGoldEffect : FateEffect
    {
        public int Amount = 2;

        public override string GetName() => "Gain Gold";

        public override string GetDescription() => Amount >= 0 ? $"gain {Amount}g" : $"lose {-Amount}g";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            session.AddGold(Amount);
        }
    }
}
