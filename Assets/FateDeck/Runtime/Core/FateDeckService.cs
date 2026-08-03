using System;
using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.Game.Decks;
using AStergio.OmniCard.Runtime.Cards.Game.Zones;
using AStergio.OmniCard.Runtime.Cards.Gameplay;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;

namespace FateDeck.Runtime.Core
{
    /// <summary>
    /// The fate deck itself: the five card zones and every verb that touches them.
    /// Pure deck mechanics only - no combat or run knowledge lives here.
    /// </summary>
    public sealed class FateDeckService
    {
        private readonly FateContentCatalog _catalog;
        private readonly FateRulesDefinition _rules;
        private readonly IGameContext _game;
        private readonly Random _rng;

        public FateDeckService(FateContentCatalog catalog, FateRulesDefinition rules, IGameContext game, Random rng)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));

            Draw = new CardZone(catalog.DrawPile);
            Discard = new CardZone(catalog.DiscardPile);
            Wound = new CardZone(catalog.WoundRow);
            Pocket = new CardZone(catalog.Pocket);
            Exile = new CardZone(catalog.ExilePile);
        }

        public CardZone Draw { get; }

        public CardZone Discard { get; }

        public CardZone Wound { get; }

        public CardZone Pocket { get; }

        public CardZone Exile { get; }

        public int ReshuffleCount { get; private set; }

        internal void RestoreReshuffleCount(int value)
        {
            ReshuffleCount = Math.Max(0, value);
        }

        /// <summary>Extra Doom added on the next reshuffle only (Hexweave-style curses).</summary>
        public int ExtraTaxNextReshuffle { get; set; }

        /// <summary>Permanent tax delta for this run (hero passives, stakes).</summary>
        public int TaxModifier { get; set; }

        /// <summary>True when a required flip or mill cannot be paid: draw and discard are both empty.</summary>
        public bool IsOutOfCards => Draw.Count == 0 && Discard.Count == 0;

        public void BuildStartingDeck(DeckDefinition deck)
        {
            if (deck == null)
            {
                return;
            }

            foreach (DeckEntry entry in deck.Cards)
            {
                if (entry?.Card == null)
                {
                    continue;
                }

                for (int i = 0; i < entry.Count; i++)
                {
                    Draw.Add(new CardInstance(entry.Card));
                }
            }

            Draw.Shuffle(_rng);
        }

        /// <summary>
        /// Removes and returns the top card of the draw pile, reshuffling (and paying the Doom tax)
        /// when it is empty. Returns null only when the deck is truly out of cards - death.
        /// </summary>
        public CardInstance TakeTop()
        {
            if (Draw.Count == 0)
            {
                if (!Reshuffle())
                {
                    return null;
                }
            }

            return Draw.RemoveTop();
        }

        /// <summary>Shuffles the discard into the draw pile and adds the Doom tax. False when nothing to shuffle.</summary>
        public bool Reshuffle()
        {
            if (Discard.Count == 0)
            {
                return false;
            }

            while (Discard.Count > 0)
            {
                CardInstance card = Discard.RemoveTop();
                Draw.Add(card);
            }

            int tax = Math.Max(0, _rules.ReshuffleTax + TaxModifier + ExtraTaxNextReshuffle);
            ExtraTaxNextReshuffle = 0;

            CardDefinition doomCard = _catalog.FateCardFor(_catalog.Doom);
            for (int i = 0; i < tax; i++)
            {
                if (doomCard != null)
                {
                    Draw.Add(new CardInstance(doomCard));
                }
            }

            Draw.Shuffle(_rng);
            ReshuffleCount++;
            _game.Events.Publish(new ReshuffleEvent(tax, ReshuffleCount));
            return true;
        }

        /// <summary>
        /// Mills up to <paramref name="count"/> cards (damage). Doom milled by damage is exiled
        /// forever; everything else lands in the Wound Row. Returns how many were actually milled -
        /// fewer than asked means the deck ran dry mid-mill.
        /// </summary>
        public int Mill(int count)
        {
            int milled = 0;
            for (int i = 0; i < count; i++)
            {
                CardInstance card = TakeTop();
                if (card == null)
                {
                    break;
                }

                MetadataEntry force = _catalog.ForceOf(card);
                bool exiled = force != null && force.GetBoolean(_catalog.ExileWhenMilledField);
                if (exiled)
                {
                    Exile.Add(card);
                    _game.Events.Publish(new CardExiledEvent(card));
                }
                else
                {
                    Wound.Add(card);
                }

                _game.Events.Publish(new CardMilledEvent(card, force, exiled));
                milled++;
            }

            return milled;
        }

        public void ToDiscard(CardInstance card)
        {
            if (card != null)
            {
                Discard.Add(card);
            }
        }

        public bool CanPocket(CardInstance card, int pocketSlots)
        {
            if (card == null || Pocket.Count >= pocketSlots)
            {
                return false;
            }

            MetadataEntry force = _catalog.ForceOf(card);
            return force == null || !force.GetBoolean(_catalog.CannotPocketField);
        }

        public void BankToPocket(CardInstance card)
        {
            Pocket.Add(card);
            _game.Events.Publish(new PocketBankedEvent(card));
        }

        public bool TakeFromPocket(CardInstance card)
        {
            return Pocket.Remove(card);
        }

        /// <summary>Returns a chosen wound card to the draw pile and shuffles. Healing is a build decision.</summary>
        public bool HealWound(CardInstance card)
        {
            if (card == null || !Wound.Remove(card))
            {
                return false;
            }

            Draw.Add(card);
            Draw.Shuffle(_rng);
            _game.Events.Publish(new WoundHealedEvent(card));
            return true;
        }

        /// <summary>Heals the oldest wounds first when no explicit choice is provided.</summary>
        public int HealWounds(int count)
        {
            int healed = 0;
            while (healed < count && Wound.Count > 0)
            {
                if (!HealWound(Wound.Cards[0]))
                {
                    break;
                }

                healed++;
            }

            return healed;
        }

        public bool ExileCard(CardZone zone, CardInstance card)
        {
            if (zone == null || card == null || !zone.Remove(card))
            {
                return false;
            }

            Exile.Add(card);
            _game.Events.Publish(new CardExiledEvent(card));
            return true;
        }

        /// <summary>Puts a known card from the discard pile on top of the draw pile. Doom cannot be stacked.</summary>
        public bool StackOnTop(CardInstance card)
        {
            if (card == null)
            {
                return false;
            }

            MetadataEntry force = _catalog.ForceOf(card);
            if (force != null && force.GetBoolean(_catalog.CannotStackField))
            {
                return false;
            }

            if (!Discard.Remove(card))
            {
                return false;
            }

            Draw.Add(card);
            return true;
        }

        /// <summary>Peeks the top cards of the draw pile without removing them; index 0 is the topmost.</summary>
        public List<CardInstance> PeekTop(int count)
        {
            var result = new List<CardInstance>();
            for (int i = Draw.Count - 1; i >= 0 && result.Count < count; i--)
            {
                result.Add(Draw.Cards[i]);
            }

            return result;
        }

        /// <summary>Creates an instance of a fate card and shuffles it into a zone at a random position.</summary>
        public CardInstance AddCard(CardDefinition definition, CardZone zone, bool randomPosition)
        {
            if (definition == null || zone == null)
            {
                return null;
            }

            var instance = new CardInstance(definition);
            if (!randomPosition || zone.Count == 0)
            {
                zone.Add(instance);
                return instance;
            }

            var buffer = new List<CardInstance>();
            while (zone.Count > 0)
            {
                buffer.Add(zone.RemoveTop());
            }

            buffer.Insert(_rng.Next(buffer.Count + 1), instance);
            for (int i = buffer.Count - 1; i >= 0; i--)
            {
                zone.Add(buffer[i]);
            }

            return instance;
        }

        /// <summary>Moves the first draw-pile card of the given force to the top (scripted openings, Stack verbs).</summary>
        public bool MoveForceToTop(MetadataEntry force)
        {
            for (int i = Draw.Count - 1; i >= 0; i--)
            {
                CardInstance card = Draw.Cards[i];
                if (_catalog.ForceOf(card) == force)
                {
                    Draw.Remove(card);
                    Draw.Add(card);
                    return true;
                }
            }

            return false;
        }

        /// <summary>Reorders the top of the draw pile to match the given list; index 0 becomes the new top.</summary>
        public void SetTopOrder(IReadOnlyList<CardInstance> topFirst)
        {
            if (topFirst == null || topFirst.Count == 0)
            {
                return;
            }

            foreach (CardInstance card in topFirst)
            {
                Draw.Remove(card);
            }

            for (int i = topFirst.Count - 1; i >= 0; i--)
            {
                Draw.Add(topFirst[i]);
            }
        }

        /// <summary>Counts draw-pile cards per force entry - the public composition of your luck.</summary>
        public Dictionary<MetadataEntry, int> DrawComposition()
        {
            var counts = new Dictionary<MetadataEntry, int>();
            foreach (CardInstance card in Draw.Cards)
            {
                MetadataEntry force = _catalog.ForceOf(card);
                if (force == null)
                {
                    continue;
                }

                counts.TryGetValue(force, out int current);
                counts[force] = current + 1;
            }

            return counts;
        }

        public int CountForceIn(CardZone zone, MetadataEntry force)
        {
            int count = 0;
            foreach (CardInstance card in zone.Cards)
            {
                if (_catalog.ForceOf(card) == force)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
