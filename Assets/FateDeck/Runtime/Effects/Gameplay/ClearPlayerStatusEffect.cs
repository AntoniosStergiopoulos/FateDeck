using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    [Serializable]
    public class ClearPlayerStatusEffect : FateEffect
    {
        public StatusKind Status = StatusKind.Burn;

        public override string GetName() => "Clear Player Status";

        public override string GetDescription() => $"remove all {Status} on you";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            if (session is FateSession concrete)
            {
                concrete.SetStatus(null, Status, 0);
            }
        }
    }
}
