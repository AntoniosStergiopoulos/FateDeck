using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    /// <summary>Curses the next reshuffle with extra Doom tax (Hexweave), or permanently shifts the tax.</summary>
    [Serializable]
    public class ModifyReshuffleTaxEffect : FateEffect
    {
        public int Delta = 1;
        public bool NextReshuffleOnly = true;

        public override string GetName() => "Modify Reshuffle Tax";

        public override string GetDescription() =>
            NextReshuffleOnly ? $"next reshuffle: +{Delta} extra Doom" : $"reshuffle tax {(Delta >= 0 ? "+" : "")}{Delta}";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            if (NextReshuffleOnly)
            {
                session.Deck.ExtraTaxNextReshuffle += Delta;
            }
            else
            {
                session.Deck.TaxModifier += Delta;
            }
        }
    }
}
