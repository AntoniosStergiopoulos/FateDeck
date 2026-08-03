using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Laws
{
    /// <summary>
    /// The Flame law on your Guard: attackers who hit you before your next turn burn.
    /// Cleared together with Block at the start of your next turn.
    /// </summary>
    [Serializable]
    public class GuardRetaliateBurnEffect : FateEffect, IActionLawPreview
    {
        public int Stacks = 2;

        public override string GetName() => "Retaliate Burn";

        public override string GetDescription() => $"attackers who hit you burn {Stacks}";

        public string PreviewNote => $"retaliate Burn {Stacks}";

        public double PreviewForce(double force) => force;

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            if (session is FateSession concrete)
            {
                concrete.PlayerRetaliateBurn += Stacks;
            }
        }
    }
}
