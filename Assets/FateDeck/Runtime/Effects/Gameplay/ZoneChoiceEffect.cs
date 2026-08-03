using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    /// <summary>
    /// Opens an interactive zone pick (exile from discard, stack from discard). Falls back to a
    /// sensible automatic pick when no view is present.
    /// </summary>
    [Serializable]
    public class ZoneChoiceEffect : FateEffect
    {
        public ZoneChoiceKind Kind = ZoneChoiceKind.ExileFromDiscard;
        public int Count = 1;

        public override string GetName() => "Zone Choice";

        public override string GetDescription()
        {
            switch (Kind)
            {
                case ZoneChoiceKind.ExileFromDiscard: return $"exile {Count} card from your discard pile";
                case ZoneChoiceKind.StackFromDiscard: return "put a discard-pile card on top of your deck";
                default: return $"heal {Count}";
            }
        }

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            if (session is FateSession concrete && concrete.RequestZoneChoice(Kind, Count))
            {
                return;
            }

            for (int i = 0; i < Count; i++)
            {
                CardInstance first = session.Deck.Discard.Count > 0 ? session.Deck.Discard.Cards[0] : null;
                if (first == null)
                {
                    return;
                }

                if (Kind == ZoneChoiceKind.ExileFromDiscard)
                {
                    session.Deck.ExileCard(session.Deck.Discard, first);
                }
                else if (Kind == ZoneChoiceKind.StackFromDiscard)
                {
                    session.Deck.StackOnTop(first);
                }
                else
                {
                    session.Deck.HealWounds(1);
                }
            }
        }
    }
}
