using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    /// <summary>Look at the top cards of the draw pile; upgrades allow reordering.</summary>
    [Serializable]
    public class ScryEffect : FateEffect
    {
        public int Count = 3;
        public bool AllowReorder;

        public override string GetName() => "Scry";

        public override string GetDescription() => AllowReorder ? $"scry {Count}, reorder" : $"scry {Count}";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            session.Scry(Count, AllowReorder);
        }
    }
}
