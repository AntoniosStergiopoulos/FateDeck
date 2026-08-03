using System;
using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Enemies
{
    /// <summary>
    /// The Collector's signature: removes every draw-pile copy of your most-numerous force
    /// into its visible Mantle. It only reads the draw pile - discard, pocket and wounds are safe.
    /// </summary>
    [Serializable]
    public class ConfiscateEffect : FateEffect
    {
        public override string GetName() => "Confiscate";

        public override string GetDescription() => "confiscates all copies of your most-numerous force";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            if (session.Combat == null)
            {
                return;
            }

            Dictionary<MetadataEntry, int> composition = session.Deck.DrawComposition();
            MetadataEntry richest = null;
            int best = 0;
            foreach (KeyValuePair<MetadataEntry, int> pair in composition)
            {
                if (pair.Value > best)
                {
                    best = pair.Value;
                    richest = pair.Key;
                }
            }

            if (richest == null)
            {
                return;
            }

            var taken = new List<CardInstance>();
            foreach (CardInstance card in session.Deck.Draw.Cards)
            {
                if (session.Catalog.ForceOf(card) == richest)
                {
                    taken.Add(card);
                }
            }

            foreach (CardInstance card in taken)
            {
                session.Deck.Draw.Remove(card);
                session.Combat.Mantle.Add(card);
            }

            if (taken.Count > 0)
            {
                session.Bark($"\"{richest.name}. Confiscated. {taken.Count} pieces, into the Mantle.\"");
            }
        }
    }
}
