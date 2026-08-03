using System;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Instances;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Effects.Enemies
{
    public enum EnemySelfChange
    {
        Heal,
        GainBlock
    }

    /// <summary>Heals the acting enemy or grants it Block (Drain, Shell, Bloom-style moves).</summary>
    [Serializable]
    public class EnemySelfEffect : FateEffect
    {
        public EnemySelfChange Change = EnemySelfChange.Heal;
        public double Amount = 1;

        public override string GetName() => "Enemy Self";

        public override string GetDescription() =>
            Change == EnemySelfChange.Heal ? $"heals itself {Amount:0.##}" : $"gains {Amount:0.##} Block";

        protected override void Resolve(EffectContext context, IFateSession session)
        {
            CardInstance enemy = session.CurrentAction?.SourceEnemy ?? context.Source;
            if (enemy == null)
            {
                return;
            }

            if (Change == EnemySelfChange.Heal)
            {
                double hp = enemy.Fields.GetNumber(session.Catalog.HpField);
                double max = enemy.Fields.GetNumber(session.Catalog.MaxHpField);
                enemy.Fields.SetNumber(session.Catalog.HpField, Math.Min(max, hp + Amount));
            }
            else
            {
                enemy.Fields.ModifyNumber(session.Catalog.BlockField, Amount);
            }
        }
    }
}
