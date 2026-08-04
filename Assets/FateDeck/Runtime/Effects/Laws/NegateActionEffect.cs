using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Laws
{
    /// <summary>The Void law: the action resolves at force zero with no effects; no further laws apply.</summary>
    [Serializable]
    public class NegateActionEffect : FateEffect, IActionLawPreview
    {
        public override string GetName() => "Negate Action";

        public override string GetDescription() => "the action fizzles entirely";

        public string PreviewNote => "negated";

        public double PreviewForce(double force) => 0;

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            FateAction action = session.CurrentAction;
            if (action != null)
            {
                action.Negated = true;
                action.Force = 0;
                if (action.IsPlayerAction)
                {
                    action.RequestsMainActionRefund = true;
                }
            }
        }
    }
}
