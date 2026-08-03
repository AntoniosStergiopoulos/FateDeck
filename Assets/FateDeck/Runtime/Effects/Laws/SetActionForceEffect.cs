using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Laws
{
    /// <summary>Sets the action's force to a fixed value (the Doom whiff half of its law).</summary>
    [Serializable]
    public class SetActionForceEffect : FateEffect, IActionLawPreview
    {
        public double Value;

        public override string GetName() => "Set Action Force";

        public override string GetDescription() => $"force becomes {Value:0.##}";

        public string PreviewNote => null;

        public double PreviewForce(double force) => Math.Max(0, Value);

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            FateAction action = session.CurrentAction;
            if (action != null)
            {
                action.Force = Math.Max(0, Value);
            }
        }
    }
}
