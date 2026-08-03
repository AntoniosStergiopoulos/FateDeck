using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Laws
{
    /// <summary>The Iron/Decay family law: the action's force changes by a flat delta.</summary>
    [Serializable]
    public class ModifyActionForceEffect : FateEffect, IActionLawPreview
    {
        public double Delta = 2;

        public override string GetName() => "Modify Action Force";

        public override string GetDescription() => Delta >= 0 ? $"force +{Delta:0.##}" : $"force {Delta:0.##}";

        public string PreviewNote => null;

        public double PreviewForce(double force) => Math.Max(0, force + Delta);

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            FateAction action = session.CurrentAction;
            if (action != null)
            {
                action.Force = Math.Max(0, action.Force + Delta);
            }
        }
    }
}
