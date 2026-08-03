using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects
{
    /// <summary>Base for all Fate Deck effect atoms: resolves only inside a Fate Deck session.</summary>
    [Serializable]
    public abstract class FateEffect : CardEffect
    {
        public sealed override void Resolve(EffectContext context)
        {
            if (context?.Game is IFateSession session)
            {
                Resolve(context, session);
            }
        }

        protected abstract void Resolve(EffectContext context, IFateSession session);
    }
}
