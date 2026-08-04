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

        /// <summary>Gold taken from the PLAYER and pocketed - recovered as bounty on kill.</summary>
        public int GoldStolen;

        public override string GetName() => "Tithe";

        public override string GetDescription()
        {
            if (GoldStolen > 0)
            {
                return $"steals {GoldStolen}g from you (recoverable as bounty)";
            }

            return $"mills your top card (ignores Block); it pockets {GoldPocketed}g";
        }

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            session.MillPlayer(Mill, "the tithe (ignores Block)");
            CardInstance enemy = session.CurrentAction?.SourceEnemy ?? context.Source;
            if (enemy == null)
            {
                return;
            }

            if (GoldPocketed > 0)
            {
                enemy.Fields.ModifyNumber(session.Catalog.PocketedGoldField, GoldPocketed);
            }

            if (GoldStolen > 0)
            {
                int taken = Math.Min(session.Gold, GoldStolen);
                if (taken > 0)
                {
                    session.AddGold(-taken);
                    enemy.Fields.ModifyNumber(session.Catalog.PocketedGoldField, taken);
                }
            }
        }
    }
}
