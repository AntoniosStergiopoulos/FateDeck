using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Laws
{
    /// <summary>The Doom law on loot: the chest is trapped - mill cards and gain nothing.</summary>
    [Serializable]
    public class DoomLootEffect : FateEffect, IActionLawPreview
    {
        public int Mill = 2;

        public override string GetName() => "Doom Loot";

        public override string GetDescription() => $"trapped: mill {Mill}, no loot";

        public string PreviewNote => $"trapped: mill {Mill}, no loot";

        public double PreviewForce(double force) => 0;

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            FateAction action = session.CurrentAction;
            if (action != null)
            {
                action.NoLoot = true;
                action.Force = 0;
            }

            session.MillPlayer(Mill);
        }
    }
}
