using System.Collections.Generic;
using System.Text;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.MetaData;

namespace FateDeck.Runtime.Core
{
    /// <summary>One row of the Odds Panel: a possible force outcome with exact fraction and result.</summary>
    public sealed class OddsRow
    {
        public MetadataEntry Force;
        public int Count;
        public int Total;
        public double ResultForce;
        public string Note;

        public double Probability => Total > 0 ? (double)Count / Total : 0;

        public string FractionLabel => $"{Count}/{Total} · {(int)System.Math.Round(Probability * 100)}%";
    }

    /// <summary>
    /// Pure odds math over the public draw-pile composition. The game does all the counting;
    /// the player only decides.
    /// </summary>
    public static class OddsCalculator
    {
        /// <summary>
        /// Builds the outcome table for an action: one row per force present in the draw pile,
        /// with the post-law force and side-effect notes derived from the force's actual law data.
        /// </summary>
        public static List<OddsRow> Table(FateContentCatalog catalog, FateDeckService deck,
            LawContext context, double baseForce)
        {
            var rows = new List<OddsRow>();
            if (catalog == null || deck == null)
            {
                return rows;
            }

            Dictionary<MetadataEntry, int> composition = deck.DrawComposition();
            int total = 0;
            foreach (KeyValuePair<MetadataEntry, int> pair in composition)
            {
                total += pair.Value;
            }

            CardFieldDefinition lawField = catalog.LawFieldFor(context);
            foreach (KeyValuePair<MetadataEntry, int> pair in composition)
            {
                rows.Add(BuildRow(pair.Key, pair.Value, total, lawField, baseForce));
            }

            rows.Sort((a, b) => b.Count.CompareTo(a.Count));
            return rows;
        }

        public static OddsRow BuildRow(MetadataEntry force, int count, int total,
            CardFieldDefinition lawField, double baseForce)
        {
            var row = new OddsRow
            {
                Force = force,
                Count = count,
                Total = total,
                ResultForce = baseForce
            };

            IReadOnlyList<CardEffect> law = force.GetEffects(lawField);
            if (law == null)
            {
                return row;
            }

            var notes = new StringBuilder();
            double result = baseForce;
            foreach (CardEffect effect in law)
            {
                if (effect is IActionLawPreview preview)
                {
                    result = preview.PreviewForce(result);
                    Append(notes, preview.PreviewNote);
                }
                else if (effect != null)
                {
                    Append(notes, effect.GetDescription());
                }
            }

            row.ResultForce = System.Math.Max(0, result);
            row.Note = notes.ToString();
            return row;
        }

        private static void Append(StringBuilder notes, string note)
        {
            if (string.IsNullOrEmpty(note))
            {
                return;
            }

            if (notes.Length > 0)
            {
                notes.Append(", ");
            }

            notes.Append(note);
        }
    }
}
