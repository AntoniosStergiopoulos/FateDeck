using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Enemies
{
    /// <summary>On the Collector's death every mantle card returns to your discard pile.</summary>
    [Serializable]
    public class ReturnMantleEffect : FateEffect
    {
        public override string GetName() => "Return Mantle";

        public override string GetDescription() => "on death, every confiscated card returns to your discard";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            if (session.Combat == null)
            {
                return;
            }

            while (session.Combat.Mantle.Count > 0)
            {
                CardInstance card = session.Combat.Mantle.RemoveTop();
                session.Deck.ToDiscard(card);
            }
        }
    }
}
