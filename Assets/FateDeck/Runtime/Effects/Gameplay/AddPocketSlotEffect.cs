using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    [Serializable]
    public class AddPocketSlotEffect : FateEffect
    {
        public int Delta = 1;

        public override string GetName() => "Add Pocket Slot";

        public override string GetDescription() => Delta >= 0 ? $"+{Delta} Pocket slot" : $"{Delta} Pocket slot";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            session.AddPocketSlots(Delta);
        }
    }
}
