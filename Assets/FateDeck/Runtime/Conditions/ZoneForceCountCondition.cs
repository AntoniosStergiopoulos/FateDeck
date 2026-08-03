using System;
using AStergio.OmniCard.Runtime.Cards.Conditions;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Game.Zones;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Conditions
{
    public enum FateZoneSelector
    {
        DrawPile,
        DiscardPile,
        WoundRow,
        ExilePile
    }

    /// <summary>GDD atom <c>ZoneCount</c>: at least N cards of a force in a fate zone.</summary>
    [Serializable]
    public class ZoneForceCountCondition : Condition
    {
        public FateZoneSelector Zone = FateZoneSelector.DrawPile;
        public MetadataEntry Force;
        public int Minimum = 1;

        public override string GetDescription() =>
            Force == null ? $"{Minimum}+ cards in {Zone}" : $"{Minimum}+ {Force.name} in {Zone}";

        public override bool Evaluate(EffectContext context)
        {
            if (!(context.Game is IFateSession session))
            {
                return false;
            }

            CardZone zone = Select(session);
            int count = Force == null ? zone.Count : session.Deck.CountForceIn(zone, Force);
            return count >= Minimum;
        }

        private CardZone Select(IFateSession session)
        {
            switch (Zone)
            {
                case FateZoneSelector.DiscardPile: return session.Deck.Discard;
                case FateZoneSelector.WoundRow: return session.Deck.Wound;
                case FateZoneSelector.ExilePile: return session.Deck.Exile;
                default: return session.Deck.Draw;
            }
        }
    }
}
