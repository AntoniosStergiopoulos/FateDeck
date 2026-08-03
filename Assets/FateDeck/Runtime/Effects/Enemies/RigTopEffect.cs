using System;
using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Enemies
{
    /// <summary>
    /// The Cardsharp Rat's paws: peeks the top cards of your deck and puts the one
    /// worse-for-you on top. Order is attackable - Scry and Pocket are the answers.
    /// </summary>
    [Serializable]
    public class RigTopEffect : FateEffect
    {
        public int PeekCount = 2;

        public override string GetName() => "Rig";

        public override string GetDescription() => $"rigs your top {PeekCount} cards against you";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            List<CardInstance> top = session.Deck.PeekTop(PeekCount);
            if (top.Count < 2)
            {
                return;
            }

            top.Sort((a, b) => Badness(session, b).CompareTo(Badness(session, a)));
            session.Deck.SetTopOrder(top);
        }

        private static int Badness(IFateSession session, CardInstance card)
        {
            MetadataEntry force = session.Catalog.ForceOf(card);
            FateContentCatalog catalog = session.Catalog;
            if (force == catalog.Doom)
            {
                return 100;
            }

            if (force == catalog.Decay || force == catalog.DecayPlus)
            {
                return 80;
            }

            if (force == catalog.Void)
            {
                return 70;
            }

            if (force == catalog.Echo)
            {
                return 50;
            }

            if (force == catalog.Fortune || force == catalog.FortunePlus)
            {
                return 40;
            }

            if (force == catalog.Gloom)
            {
                return 45;
            }

            if (force == catalog.Flame || force == catalog.FlamePlus)
            {
                return 30;
            }

            if (force == catalog.Mirror || force == catalog.Key || force == catalog.Wisp)
            {
                return 25;
            }

            if (force == catalog.Glass || force == catalog.TempestPlus)
            {
                return 5;
            }

            return 10;
        }
    }
}
