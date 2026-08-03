using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    /// <summary>Exiles the first draw-pile cards of a force (the Beggar of Odds eats a Doom).</summary>
    [Serializable]
    public class ExileForceFromDrawEffect : FateEffect
    {
        public MetadataEntry Force;
        public int Count = 1;

        public override string GetName() => "Exile Force From Draw";

        public override string GetDescription() =>
            Force == null ? "exile a card" : $"exile {Count} {Force.name} from your deck";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            int remaining = Count;
            for (int i = session.Deck.Draw.Count - 1; i >= 0 && remaining > 0; i--)
            {
                CardInstance card = session.Deck.Draw.Cards[i];
                if (session.Catalog.ForceOf(card) == Force)
                {
                    session.Deck.ExileCard(session.Deck.Draw, card);
                    remaining--;
                }
            }
        }
    }
}
