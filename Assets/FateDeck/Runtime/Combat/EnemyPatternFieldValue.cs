using System;
using System.Collections.Generic;
using System.Text;
using AStergio.OmniCard.Runtime.Cards.Data;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;

namespace FateDeck.Runtime.Combat
{
    /// <summary>The stored pattern steps; resolves to a readable "[Attack 2, Brace 2]" summary.</summary>
    [Serializable]
    public class EnemyPatternFieldValue : CardFieldValue
    {
        public List<EnemyActionSpec> Steps = new List<EnemyActionSpec>();

        public override CardValue Resolve(ICardFieldOwner owner)
        {
            if (Steps == null || Steps.Count == 0)
            {
                return CardValue.FromText(string.Empty);
            }

            var builder = new StringBuilder("[");
            for (int i = 0; i < Steps.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(Steps[i].IntentLabel());
            }

            builder.Append("]");
            return CardValue.FromText(builder.ToString());
        }
    }
}
