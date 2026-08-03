using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Laws
{
    /// <summary>
    /// The Rust law: Block corrodes. On your offense the target's Block flakes away before
    /// damage lands; on an enemy action your own Block rusts.
    /// </summary>
    [Serializable]
    public class CorrodeBlockEffect : FateEffect, IActionLawPreview
    {
        public double Amount = 2;

        public override string GetName() => "Corrode Block";

        public override string GetDescription() => $"corrodes {Amount:0.##} Block off the victim";

        public string PreviewNote => $"corrode {Amount:0.##} Block";

        public double PreviewForce(double force) => force;

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            FateAction action = session.CurrentAction;
            if (action == null)
            {
                return;
            }

            if (action.SourceEnemy != null)
            {
                session.AddPlayerBlock(-Amount);
                return;
            }

            CardInstance target = action.TargetEnemy ?? session.Combat?.SelectedOrFirstEnemy();
            if (target != null)
            {
                double block = target.Fields.GetNumber(session.Catalog.BlockField);
                target.Fields.SetNumber(session.Catalog.BlockField, Math.Max(0, block - Amount));
            }
        }
    }
}
