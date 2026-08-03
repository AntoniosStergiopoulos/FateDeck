using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    /// <summary>
    /// Returns wound cards to the draw pile. When a view is present it opens the wound-choice
    /// overlay (healing which wound is a build decision); headless it heals oldest first.
    /// </summary>
    [Serializable]
    public class HealWoundsEffect : FateEffect
    {
        public int Count = 3;

        public override string GetName() => "Heal Wounds";

        public override string GetDescription() => $"heal {Count} (return wound cards to the deck)";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            if (session is FateSession concrete && concrete.RequestWoundChoice(Count))
            {
                return;
            }

            session.Deck.HealWounds(Count);
        }
    }
}
