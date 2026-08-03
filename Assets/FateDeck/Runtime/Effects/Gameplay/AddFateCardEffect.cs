using System;
using AStergio.OmniCard.Runtime.Cards.Data;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Game.Zones;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    public enum FateDeckZone
    {
        DrawPile,
        DiscardPile
    }

    /// <summary>Adds a fate card to the player's deck (enemy injectors, the Card Sharp, shops).</summary>
    [Serializable]
    public class AddFateCardEffect : FateEffect
    {
        public CardDefinition Card;
        public FateDeckZone Zone = FateDeckZone.DrawPile;
        public bool RandomPosition = true;
        public int Count = 1;

        public override string GetName() => "Add Fate Card";

        public override string GetDescription() =>
            Card == null ? "add a card" : $"shuffle {Count}x {Card.name} into your deck";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            CardZone zone = Zone == FateDeckZone.DrawPile ? session.Deck.Draw : session.Deck.Discard;
            for (int i = 0; i < Count; i++)
            {
                session.Deck.AddCard(Card, zone, RandomPosition);
            }
        }
    }
}
