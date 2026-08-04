using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace FateDeck.Runtime.Views
{
    public sealed partial class FateTableView
    {
        private int _woundPicksRemaining;

        /// <summary>Rebuilds the bottom tableau (the deck-as-HUD) and the left composition column.</summary>
        private void RefreshTableau()
        {
            FateSession session = Session;
            if (session == null || _tableauBar == null)
            {
                return;
            }

            _tableauBar.Clear();
            _tableauBar.Add(TableauSection("DRAW PILE", BuildDrawSection(session)));
            _tableauBar.Add(TableauSection("DISCARD", BuildDiscardSection(session)));
            _tableauBar.Add(TableauSection($"WOUND ROW · {session.Deck.Wound.Count}", BuildWoundSection(session)));
            _tableauBar.Add(TableauSection($"POCKET · {session.Deck.Pocket.Count}/{session.PocketSlots}",
                BuildPocketSection(session)));
            _tableauBar.Add(TableauSection($"CHARMS · {session.CharmZone.Count}/{session.Rules.MaxCharms}",
                BuildCharmSection(session)));

            RefreshLeftColumn(session);
            RefreshRelicPanel(session);
        }

        private static VisualElement TableauSection(string title, VisualElement content)
        {
            var section = new VisualElement();
            section.style.marginRight = 18;
            Label header = FateUi.Text(title, 12, FateUi.GoldLeaf);
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 4;
            section.Add(header);
            section.Add(content);
            return section;
        }

        private VisualElement BuildDrawSection(FateSession session)
        {
            var row = FateUi.Row(0);
            VisualElement deck = CardElementBuilder.DeckTile(session.Deck.Draw.Count, 72, ShowPileOverlay);
            FateTip.Bind(deck, () =>
            {
                FateSession current = Session;
                int tax = Mathf.Max(0, current.Rules.ReshuffleTax + current.Deck.TaxModifier
                    + current.Deck.ExtraTaxNextReshuffle);
                return "<b>YOUR DECK = YOUR LIFE</b>\nEvery flip and every point of damage comes off "
                    + "the top. When it empties, the discard shuffles back in and the House adds "
                    + $"{tax} Doom (the reshuffle tax).\n\nClick to inspect the composition.";
            });
            row.Add(deck);
            var caption = FateUi.Column(0);
            caption.style.justifyContent = Justify.FlexEnd;
            Label reshuffles = FateUi.Text($"reshuffles {session.Deck.ReshuffleCount}", 11, FateUi.BoneDim);
            Label hint = FateUi.Text("click to\ninspect", 11, FateUi.BoneDim);
            caption.Add(hint);
            caption.Add(reshuffles);
            row.Add(caption);
            return row;
        }

        private VisualElement BuildDiscardSection(FateSession session)
        {
            var row = FateUi.Row(0);
            IReadOnlyList<CardInstance> cards = session.Deck.Discard.Cards;
            int shown = Mathf.Min(3, cards.Count);
            for (int i = cards.Count - shown; i < cards.Count; i++)
            {
                row.Add(CardElementBuilder.ForceTile(_catalog, cards[i], 52));
            }

            if (cards.Count == 0)
            {
                row.Add(EmptySlot(52, "empty"));
            }
            else if (cards.Count > shown)
            {
                Label more = FateUi.Text($"+{cards.Count - shown}", 13, FateUi.BoneDim);
                more.style.marginLeft = 2;
                row.Add(more);
            }

            return row;
        }

        private VisualElement BuildWoundSection(FateSession session)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.Wrap;
            row.style.maxWidth = 300;

            if (session.Deck.Wound.Count == 0)
            {
                row.Add(EmptySlot(52, "whole"));
                return row;
            }

            float tileWidth = session.Deck.Wound.Count > 5 ? 42 : 52;
            foreach (CardInstance wound in session.Deck.Wound.Cards)
            {
                CardInstance captured = wound;
                bool pickable = _woundPicksRemaining > 0;
                VisualElement tile = CardElementBuilder.ForceTile(_catalog, captured, tileWidth,
                    pickable ? () => OnWoundClicked(captured) : (System.Action)null,
                    pickable ? "mend?" : null);
                if (pickable)
                {
                    FateUi.SetBorder(tile, FateUi.Verdigris, 2, 6);
                }

                row.Add(tile);
            }

            // Deep wound rows scroll instead of stretching the tableau off screen.
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.maxHeight = 118;
            scroll.style.maxWidth = 316;
            scroll.Add(row);
            return scroll;
        }

        private VisualElement BuildPocketSection(FateSession session)
        {
            var holder = FateUi.Column(0);
            var row = FateUi.Row(0);
            holder.Add(row);
            bool playable = session.Phase == FateResolutionPhase.AwaitPreFlip;
            foreach (CardInstance pocketed in session.Deck.Pocket.Cards)
            {
                CardInstance captured = pocketed;
                VisualElement tile = CardElementBuilder.ForceTile(_catalog, captured, 66,
                    playable ? () => OnPocketClicked(captured) : (System.Action)null,
                    playable ? "PLAY" : null,
                    playable
                        ? "PLAY NOW: replaces the pending flip with this card. No card leaves your deck."
                        : "Pocketed. During any pre-flip window (the pause before a card is flipped - "
                          + "yours or an enemy's) click it to REPLACE that flip entirely.");
                if (playable)
                {
                    FateUi.SetBorder(tile, FateUi.GoldLeaf, 2, 6);
                }

                row.Add(tile);
            }

            for (int i = session.Deck.Pocket.Count; i < session.PocketSlots; i++)
            {
                VisualElement slot = EmptySlot(66, "sleeve");
                FateTip.Bind(slot,
                    "An empty Pocket slot. When one of YOUR actions flips a card, choose SLEEVE IT "
                    + "to bank the card here instead of applying its law.");
                row.Add(slot);
            }

            Label caption = FateUi.Text(
                playable && session.Deck.Pocket.Count > 0
                    ? "click a card to replace this flip!"
                    : session.Deck.Pocket.Count > 0
                        ? "plays during pre-flip windows"
                        : "SLEEVE IT banks cards here",
                10, playable && session.Deck.Pocket.Count > 0 ? FateUi.GoldLeaf : FateUi.BoneDim);
            caption.style.marginTop = 2;
            holder.Add(caption);
            return holder;
        }

        private VisualElement BuildCharmSection(FateSession session)
        {
            var row = FateUi.Row(0);
            if (session.CharmZone.Count == 0)
            {
                row.Add(EmptySlot(60, "none"));
                return row;
            }

            foreach (CardInstance charm in session.CharmZone.Cards)
            {
                CardInstance captured = charm;
                var tile = new VisualElement();
                tile.style.width = 96;
                tile.style.height = 86;
                tile.style.backgroundColor = FateUi.PanelLight;
                FateUi.SetBorder(tile, FateUi.Violet, 1, 6);
                FateUi.Pad(tile, 5);
                tile.style.marginRight = 5;
                tile.style.justifyContent = Justify.SpaceBetween;

                Label name = FateUi.Text(captured.DisplayName, 12, FateUi.Bone);
                name.style.unityFontStyleAndWeight = FontStyle.Bold;
                tile.Add(name);
                Label use = FateUi.Text("click to use", 10, FateUi.BoneDim);
                tile.Add(use);
                FateUi.MakeClickable(tile, () => OnCharmClicked(captured));
                bool isMain = captured.Definition.GetBoolean(_catalog.MainActionField);
                FateTip.Bind(tile, $"<b>{captured.DisplayName}</b> (charm - one use)\n"
                    + captured.Definition.GetText(_catalog.DescriptionField)
                    + (isMain
                        ? "\n\nCosts your Main Action for the turn."
                        : "\n\nFree - does not cost your Main Action."));
                row.Add(tile);
            }

            return row;
        }

        private static VisualElement EmptySlot(float width, string caption)
        {
            var slot = new VisualElement();
            slot.style.width = width;
            slot.style.height = width * 1.32f;
            FateUi.SetBorder(slot, new Color(0.28f, 0.25f, 0.20f, 0.7f), 1, 6);
            slot.style.alignItems = Align.Center;
            slot.style.justifyContent = Justify.Center;
            slot.style.marginRight = 5;
            slot.Add(FateUi.Text(caption, 10, FateUi.BoneDim));
            return slot;
        }

        // ---------------------------------------------------------------- left column

        private void RefreshLeftColumn(FateSession session)
        {
            _leftColumn.Clear();
            _leftColumn.Add(BuildHeroPanel(session));

            VisualElement composition = FateUi.MakePanel($"DRAW PILE · {session.Deck.Draw.Count} CARDS");
            composition.style.marginTop = 8;
            int total = session.Deck.Draw.Count;

            var rows = FateUi.Column(0);
            foreach (KeyValuePair<MetadataEntry, int> pair in session.Deck.DrawComposition())
            {
                rows.Add(CompositionRow(pair.Key, pair.Value, total));
            }

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.maxHeight = 220;
            scroll.Add(rows);
            composition.Add(scroll);

            if (total == 0)
            {
                composition.Add(FateUi.Text("empty — the next flip reshuffles", 12, FateUi.BoneDim));
            }

            composition.Add(FateUi.Divider());
            composition.Add(FateUi.Text(
                $"discard {session.Deck.Discard.Count} · exile {session.Deck.Exile.Count}", 12, FateUi.BoneDim));
            _leftColumn.Add(composition);

            VisualElement odds = BuildOddsColumn(session);
            if (odds != null)
            {
                odds.style.marginTop = 8;
                _leftColumn.Add(odds);
            }
        }

        /// <summary>The hero card: who you are, what your passive does, and its live charges.</summary>
        private VisualElement BuildHeroPanel(FateSession session)
        {
            VisualElement panel = FateUi.MakePanel();
            CardInstance hero = session.Hero;
            string heroName = hero != null ? hero.DisplayName : "THE NAMELESS";
            Label name = FateUi.Text(heroName.ToUpperInvariant(), 14, FateUi.GoldLeaf);
            name.style.unityFontStyleAndWeight = FontStyle.Bold;
            panel.Add(name);

            string passive = hero?.Definition.GetText(_catalog.DescriptionField);
            if (!string.IsNullOrEmpty(passive))
            {
                panel.Add(FateUi.Text(passive, 12, FateUi.BoneDim));
            }

            var chips = new VisualElement();
            chips.style.flexDirection = FlexDirection.Row;
            chips.style.flexWrap = Wrap.Wrap;
            chips.style.marginTop = 4;

            if (session.DoubleDrawCharges > 0)
            {
                VisualElement drawChip = FateUi.Chip($"Draw-2 ready x{session.DoubleDrawCharges}",
                    FateUi.Verdigris, 11);
                FateTip.Bind(drawChip,
                    "Your next flip reveals TWO cards and you choose which law applies. "
                    + "The other goes to your discard.");
                chips.Add(drawChip);
            }

            if (session.NextPlayerActionBonus > 0)
            {
                VisualElement bonusChip = FateUi.Chip($"Next action +{session.NextPlayerActionBonus:0.#}",
                    FateUi.GoldLeaf, 11);
                FateTip.Bind(bonusChip, "Bonus Force consumed by the next action you declare.");
                chips.Add(bonusChip);
            }

            if (session.Deck.ExtraTaxNextReshuffle != 0)
            {
                int extra = session.Deck.ExtraTaxNextReshuffle;
                VisualElement taxChip = FateUi.Chip(
                    extra > 0 ? $"Next reshuffle +{extra} Doom" : $"Next reshuffle {extra} Doom",
                    extra > 0 ? FateUi.Blood : FateUi.Verdigris, 11);
                FateTip.Bind(taxChip, extra > 0
                    ? "A curse: the next time your discard shuffles back in, the House adds this much EXTRA Doom."
                    : "A blessing: the next reshuffle adds this much LESS Doom.");
                chips.Add(taxChip);
            }

            if (chips.childCount > 0)
            {
                panel.Add(chips);
            }
            else
            {
                Label idle = FateUi.Text("passive charges appear here when ready", 10, FateUi.BoneDim);
                idle.style.marginTop = 2;
                panel.Add(idle);
            }

            return panel;
        }

        private VisualElement CompositionRow(MetadataEntry force, int count, int total)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 2;
            FateTip.Bind(row, CardElementBuilder.ForceTipText(_catalog, force));

            Color color = ForceColor(force);
            var swatch = new VisualElement();
            swatch.style.width = 10;
            swatch.style.height = 10;
            swatch.style.backgroundColor = color;
            swatch.style.borderTopLeftRadius = 2;
            swatch.style.borderTopRightRadius = 2;
            swatch.style.borderBottomLeftRadius = 2;
            swatch.style.borderBottomRightRadius = 2;
            swatch.style.marginRight = 6;
            row.Add(swatch);

            Label name = FateUi.Text(force.name, 13, color);
            name.style.width = 84;
            name.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(name);

            int percent = total > 0 ? Mathf.RoundToInt(count * 100f / total) : 0;
            row.Add(FateUi.Text($"{count}/{total} · {percent}%", 13, FateUi.Bone));
            return row;
        }

        private void RefreshRelicPanel(FateSession session)
        {
            VisualElement existing = _rightColumn.Q("relic-panel");
            existing?.RemoveFromHierarchy();

            if (session.RelicZone.Count == 0)
            {
                return;
            }

            VisualElement panel = FateUi.MakePanel($"RELICS · {session.RelicZone.Count}");
            panel.name = "relic-panel";
            panel.style.marginBottom = 8;

            var list = FateUi.Column(0);
            foreach (CardInstance relic in session.RelicZone.Cards)
            {
                Label name = FateUi.Text(relic.DisplayName, 13, FateUi.GoldLeaf);
                name.style.unityFontStyleAndWeight = FontStyle.Bold;
                list.Add(name);
                string rules = relic.Definition.GetText(_catalog.DescriptionField);
                if (!string.IsNullOrEmpty(rules))
                {
                    Label description = FateUi.Text(rules, 12, FateUi.BoneDim);
                    description.style.marginBottom = 4;
                    list.Add(description);
                }
            }

            // Many relics scroll inside the panel rather than crowding out the event log.
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.maxHeight = 190;
            scroll.Add(list);
            panel.Add(scroll);

            _rightColumn.Insert(0, panel);
        }

        // ---------------------------------------------------------------- tableau interactions

        private void OnPocketClicked(CardInstance pocketCard)
        {
            if (Session.Phase == FateResolutionPhase.AwaitPreFlip)
            {
                Session.PlayPocket(pocketCard);
            }
        }

        private void OnCharmClicked(CardInstance charm)
        {
            string rules = charm.Definition.GetText(_catalog.DescriptionField);
            if (Session.UseCharm(charm))
            {
                _log.Append($"Charm spent: {charm.DisplayName} — {rules}", FateUi.Violet, bold: true);
            }
            else
            {
                _log.Append($"{charm.DisplayName} can't be used right now ({rules})", FateUi.BoneDim);
            }

            RefreshTableau();
            MarkScreenDirty();
        }

        private void OnWoundClicked(CardInstance wound)
        {
            if (_woundPicksRemaining > 0 && Session.Deck.HealWound(wound))
            {
                _woundPicksRemaining--;
                UpdateWoundPrompt();
                RefreshTableau();
            }
        }

        private void UpdateWoundPrompt()
        {
            // BuildPromptArea renders the pending-pick prompt; a rebuild keeps it alive.
            MarkScreenDirty();
        }
    }
}
