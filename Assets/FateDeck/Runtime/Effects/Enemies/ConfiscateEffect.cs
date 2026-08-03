using System;
using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Enemies
{
    /// <summary>
    /// The Collector's signature: removes draw-pile copies of your most-numerous force
    /// into its visible Mantle, at most <see cref="MaxTaken"/> per appraisal. It only reads
    /// the draw pile - discard, pocket and wounds are safe - and heavy hits shake cards loose.
    /// </summary>
    [Serializable]
    public class ConfiscateEffect : FateEffect
    {
        public int MaxTaken = 3;

        public override string GetName() => "Confiscate";

        public override string GetDescription() =>
            $"confiscates up to {MaxTaken} copies of your most-numerous force";

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
                    if (MaxTaken > 0 && taken.Count >= MaxTaken)
                    {
                        break;
                    }
                }
            }

            foreach (CardInstance card in taken)
            {
                session.Deck.Draw.Remove(card);
                session.Combat.Mantle.Add(card);
            }

            if (taken.Count > 0)
            {
                session.Events.Publish(new MantleTakenEvent(richest, taken.Count));
                session.Bark($"\"{richest.name}. Appraised. {taken.Count} pieces, into the Mantle.\"");
            }
        }
    }
}
