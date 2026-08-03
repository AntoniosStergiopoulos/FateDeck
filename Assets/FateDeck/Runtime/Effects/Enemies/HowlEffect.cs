using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Enemies
{
    /// <summary>Permanently raises the acting enemy's force (the Tinder Hound's Howl).</summary>
    [Serializable]
    public class HowlEffect : FateEffect
    {
        public double Delta = 1;

        public override string GetName() => "Howl";

        public override string GetDescription() => $"gains +{Delta:0.##} Force permanently";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            CardInstance enemy = session.CurrentAction?.SourceEnemy ?? context.Source;
            enemy?.Fields.ModifyNumber(session.Catalog.ForceBonusField, Delta);
        }
    }
}
