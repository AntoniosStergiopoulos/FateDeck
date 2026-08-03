using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Laws
{
    /// <summary>
    /// The Mirror law: repeats the law of the force that resolved before it, in the current
    /// context. Copying another Mirror (or nothing) polishes into a flat bonus instead.
    /// </summary>
    [Serializable]
    public class CopyLastForceEffect : FateEffect, IActionLawPreview
    {
        public double FallbackDelta = 1;

        public override string GetName() => "Copy Last Force";

        public override string GetDescription() => "repeats the previous force's law";

        public string PreviewNote => "repeats the last law";

        public double PreviewForce(double force) => force;

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            FateAction action = session.CurrentAction;
            MetadataEntry last = session.LastFlippedForce;
            if (action == null)
            {
                return;
            }

            bool applied = false;
            if (last != null)
            {
                CardFieldDefinition lawField = session.Catalog.LawFieldFor(action.Context);
                var law = last.GetEffects(lawField);
                if (law != null)
                {
                    foreach (CardEffect effect in law)
                    {
                        if (effect == null || effect is CopyLastForceEffect)
                        {
                            continue;
                        }

                        effect.Resolve(new EffectContext(context.Source, session));
                        applied = true;

                        if (action.Negated)
                        {
                            return;
                        }
                    }
                }
            }

            if (!applied)
            {
                action.Force = Math.Max(0, action.Force + FallbackDelta);
            }
        }
    }
}
