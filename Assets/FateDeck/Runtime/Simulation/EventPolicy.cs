using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Core;
using FateDeck.Runtime.Effects.Gameplay;
using FateDeck.Runtime.Run;

namespace FateDeck.Runtime.Simulation
{
    /// <summary>
    /// Heuristic value of an event option for the baseline <see cref="AutoPlayer"/>:
    /// sums per-effect worth (gold, heals, Debt exiles score high; taking Debt or
    /// milling scores negative), averages ritual outcomes, and discounts the costs.
    /// </summary>
    public static class EventPolicy
    {
        public static double Score(EventOption option, FateContentCatalog catalog)
        {
            double score = ScoreEffects(option.Effects, catalog);
            if (option.FlipsFate && option.RitualOutcomes.Count > 0)
            {
                double total = 0;
                foreach (RitualOutcome outcome in option.RitualOutcomes)
                {
                    total += ScoreEffects(outcome.Effects, catalog);
                }

                score += total / option.RitualOutcomes.Count;
            }

            return score - option.GoldCost * 0.15 - option.KeyCost * 1.5;
        }

        private static double ScoreEffects(List<CardEffect> effects, FateContentCatalog catalog)
        {
            double score = 0;
            foreach (CardEffect effect in effects)
            {
                score += ScoreEffect(effect, catalog);
            }

            return score;
        }

        private static double ScoreEffect(CardEffect effect, FateContentCatalog catalog)
        {
            switch (effect)
            {
                case GainGoldEffect gold:
                    return gold.Amount * 0.15;

                case HealWoundsEffect heal:
                    return heal.Count * 1.2;

                case MillPlayerEffect mill:
                    return -mill.Count * 1.5;

                case ExileForceFromDrawEffect exile:
                    return exile.Force == catalog.Doom ? exile.Count * 2.5 : -exile.Count * 0.5;

                case AddFateCardEffect add:
                {
                    var force = add.Card != null
                        ? add.Card.GetObject(catalog.ForceField) as MetadataEntry
                        : null;
                    if (force == null)
                    {
                        return 0;
                    }

                    if (force == catalog.Doom || force == catalog.Rust || force == catalog.Gloom)
                    {
                        return -2.5 * add.Count;
                    }

                    return 1.5 * add.Count;
                }

                case GainKeyEffect key:
                    return key.Count * 0.8;

                case AddPocketSlotEffect slot:
                    return slot.Delta * 2.0;

                case DoubleDrawNextEffect _:
                    return 1.0;

                case NextActionBonusEffect bonus:
                    return bonus.Delta * 0.4;

                case ScryEffect _:
                    return 0.3;

                case ModifyReshuffleTaxEffect tax:
                    return -tax.Delta * 1.2;

                case ZoneChoiceEffect choice:
                    return choice.Kind == ZoneChoiceKind.UpgradeFromDraw ? 2.0 : 0.8;

                default:
                    // Unknown atoms in events are authored as rewards more often than costs.
                    return 0.25;
            }
        }
    }
}
