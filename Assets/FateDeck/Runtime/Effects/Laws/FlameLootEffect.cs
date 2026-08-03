using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Laws
{
    /// <summary>The Flame law on loot: opens locked chests but burns part of the haul.</summary>
    [Serializable]
    public class FlameLootEffect : FateEffect, IActionLawPreview
    {
        public double GoldBurned = 3;

        public override string GetName() => "Flame Loot";

        public override string GetDescription() => $"opens locked chests; burns {GoldBurned:0.##}g of the haul";

        public string PreviewNote => "opens locks";

        public double PreviewForce(double force) => Math.Max(0, force - GoldBurned);

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            FateAction action = session.CurrentAction;
            if (action != null)
            {
                action.OpensLock = true;
                action.Force = Math.Max(0, action.Force - GoldBurned);
            }
        }
    }
}
