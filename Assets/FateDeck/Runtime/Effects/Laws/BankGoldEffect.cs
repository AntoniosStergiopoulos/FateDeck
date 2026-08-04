using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Laws
{
    /// <summary>
    /// The Fortune law: the action's owner banks gold. On enemy actions the enemy pockets it -
    /// recovered as bounty when it dies.
    /// </summary>
    [Serializable]
    public class BankGoldEffect : FateEffect, IActionLawPreview, IContextDescribed
    {
        public int Amount = 3;

        public override string GetName() => "Bank Gold";

        public override string GetDescription() => $"the owner banks {Amount}g";

        public string PreviewNote => $"+{Amount}g";

        public double PreviewForce(double force) => force;

        public string DescribeFor(LawContext context)
        {
            return context == LawContext.EnemyAction
                ? $"the enemy pockets {Amount}g (paid back as bounty when it dies)"
                : $"YOU bank {Amount}g";
        }

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            FateAction action = session.CurrentAction;
            if (action?.SourceEnemy != null)
            {
                action.SourceEnemy.Fields.ModifyNumber(session.Catalog.PocketedGoldField, Amount);
            }
            else
            {
                session.AddGold(Amount);
            }
        }
    }
}
