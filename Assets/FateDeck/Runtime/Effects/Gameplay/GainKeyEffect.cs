using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    /// <summary>Grants keys (the Key force, locksmith events, charms). Keys open locked chests.</summary>
    [Serializable]
    public class GainKeyEffect : FateEffect, IActionLawPreview
    {
        public int Count = 1;

        public override string GetName() => "Gain Key";

        public override string GetDescription() => Count == 1 ? "gain a Key" : $"gain {Count} Keys";

        public string PreviewNote => Count == 1 ? "+1 Key" : $"+{Count} Keys";

        public double PreviewForce(double force) => force;

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            session.AddKeys(Count);
        }
    }
}
