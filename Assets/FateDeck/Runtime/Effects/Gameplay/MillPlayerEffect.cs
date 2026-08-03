using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    /// <summary>Mills the player: top of draw pile to the Wound Row (Doom is exiled instead).</summary>
    [Serializable]
    public class MillPlayerEffect : FateEffect, IActionLawPreview
    {
        public int Count = 1;

        public override string GetName() => "Mill Player";

        public override string GetDescription() => $"mill {Count}";

        public string PreviewNote => $"mill {Count}";

        public double PreviewForce(double force) => force;

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            session.MillPlayer(Count);
        }
    }
}
