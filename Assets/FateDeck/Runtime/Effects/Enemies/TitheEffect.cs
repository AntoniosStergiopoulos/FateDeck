using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Enemies
{
    /// <summary>
    /// The Toll Collector's Tithe: mills your top card, bypassing Block, and the enemy
    /// pockets gold - recovered as bounty when it dies.
    /// </summary>
    [Serializable]
    public class TitheEffect : FateEffect
    {
        public int Mill = 1;
        public int GoldPocketed = 1;

        public override string GetName() => "Tithe";

        public override string GetDescription() => $"mills your top card (ignores Block); it pockets {GoldPocketed}g";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            session.MillPlayer(Mill);
            CardInstance enemy = session.CurrentAction?.SourceEnemy ?? context.Source;
            if (enemy != null && GoldPocketed > 0)
            {
                enemy.Fields.ModifyNumber(session.Catalog.PocketedGoldField, GoldPocketed);
            }
        }
    }
}
