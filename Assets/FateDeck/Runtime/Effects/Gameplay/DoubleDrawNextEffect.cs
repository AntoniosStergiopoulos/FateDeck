using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Gameplay
{
    /// <summary>Your next flip: reveal two cards and choose which law applies (Loaded Coin, Opening Hand).</summary>
    [Serializable]
    public class DoubleDrawNextEffect : FateEffect
    {
        public int Charges = 1;

        public override string GetName() => "Double Draw";

        public override string GetDescription() => "next flip: draw 2, choose which applies";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            session.AddDoubleDrawCharges(Charges);
        }
    }
}
