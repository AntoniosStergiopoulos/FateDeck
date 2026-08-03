using System;
using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.Game.Zones;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace FateDeck.Runtime.Views
{
    public sealed partial class FateTableView
    {
        private readonly List<CardInstance> _scryOrder = new List<CardInstance>();

        /// <summary>Opens a modal overlay panel; the dimmer element blocks everything behind it.</summary>
        private VisualElement OpenOverlay(string title)
        {
            _overlayHost.Clear();

            var dimmer = new VisualElement();
            dimmer.style.position = Position.Absolute;
            dimmer.style.left = 0;
            dimmer.style.right = 0;
            dimmer.style.top = 0;
            dimmer.style.bottom = 0;
            dimmer.style.backgroundColor = new Color(0f, 0f, 0f, 0.78f);
            dimmer.style.alignItems = Align.Center;
            dimmer.style.justifyContent = Justify.Center;
            _overlayHost.Add(dimmer);

            VisualElement panel = FateUi.MakePanel();
            panel.style.minWidth = 560;
            panel.style.maxWidth = 900;
            FateUi.Pad(panel, 16);
            dimmer.Add(panel);

            Label heading = FateUi.Text(title, 18, FateUi.GoldLeaf);
            heading.style.unityFontStyleAndWeight = FontStyle.Bold;
            heading.style.marginBottom = 10;
            panel.Add(heading);
            return panel;
        }

        private void CloseOverlay()
        {
            _overlayHost.Clear();
            RefreshTableau();
            MarkScreenDirty();
        }

        // ---------------------------------------------------------------- draw pile inspection

        private void ShowPileOverlay()
        {
            if (IsOverlayOpen)
            {
                return;
            }

            VisualElement panel = OpenOverlay("THE DRAW PILE — composition is public. Order is not.");
            int total = Session.Deck.Draw.Count;
            foreach (KeyValuePair<MetadataEntry, int> pair in Session.Deck.DrawComposition())
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.marginBottom = 3;
                Color color = ForceColor(pair.Key);
                Label name = FateUi.Text(pair.Key.name, 15, color);
                name.style.width = 110;
                name.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(name);
                int percent = total > 0 ? Mathf.RoundToInt(pair.Value * 100f / total) : 0;
                row.Add(FateUi.Text($"{pair.Value}/{total} · {percent}%", 15, FateUi.Bone));
                string law = pair.Key.GetText(_catalog.DescriptionField);
                if (!string.IsNullOrEmpty(law))
                {
                    Label lawLabel = FateUi.Text($"   {law}", 13, FateUi.BoneDim);
                    lawLabel.style.flexShrink = 1;
                    row.Add(lawLabel);
                }

                panel.Add(row);
            }

            if (total == 0)
            {
                panel.Add(FateUi.Text("Empty — the next flip reshuffles the discard (and pays the tax).",
                    14, FateUi.BoneDim));
            }

            panel.Add(FateUi.MakeButton("CLOSE", CloseOverlay, FateUi.BoneDim, 14));
        }

        // ---------------------------------------------------------------- generic zone pick

        private void ShowZonePick(CardZone zone, string title, Func<CardInstance, bool> onPick,
            Func<CardInstance, bool> filter = null)
        {
            VisualElement panel = OpenOverlay(title);
            var candidates = new List<CardInstance>();
            foreach (CardInstance card in zone.Cards)
            {
                if (filter == null || filter(card))
                {
                    candidates.Add(card);
                }
            }

            if (candidates.Count == 0)
            {
                panel.Add(FateUi.Text("Nothing here qualifies.", 14, FateUi.BoneDim));
            }

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.maxWidth = 820;
            panel.Add(grid);

            foreach (CardInstance card in candidates)
            {
                CardInstance captured = card;
                grid.Add(CardElementBuilder.ForceTile(_catalog, captured, 66, () =>
                {
                    if (onPick(captured))
                    {
                        CloseOverlay();
                    }
                    else
                    {
                        _log.Append("That pick isn't possible (check gold or limits).", FateUi.BoneDim);
                    }
                }));
            }

            panel.Add(FateUi.MakeButton("NEVER MIND", CloseOverlay, FateUi.BoneDim, 14));
        }

        // ---------------------------------------------------------------- scry

        private void OnScry(ScryEvent scry)
        {
            _scryOrder.Clear();
            VisualElement panel = OpenOverlay(scry.AllowReorder
                ? "SCRY — click cards in the order they should sit, top first"
                : "SCRY — the top of your deck, top first");

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.Center;
            panel.Add(row);

            var orderBadges = new Dictionary<CardInstance, Label>();
            for (int i = 0; i < scry.Cards.Count; i++)
            {
                CardInstance card = scry.Cards[i];
                var slot = FateUi.Column(2);
                slot.style.alignItems = Align.Center;
                slot.style.marginLeft = 6;
                slot.style.marginRight = 6;

                Label position = FateUi.Text(i == 0 ? "top" : $"#{i + 1}", 12, FateUi.BoneDim);
                slot.Add(position);
                slot.Add(CardElementBuilder.ForceTile(_catalog, card, 86, !scry.AllowReorder
                    ? (Action)null
                    : () =>
                    {
                        if (!_scryOrder.Contains(card))
                        {
                            _scryOrder.Add(card);
                            orderBadges[card].text = $"new #{_scryOrder.Count}";
                            orderBadges[card].style.color = FateUi.Verdigris;
                        }
                    }));

                Label badge = FateUi.Text(" ", 12, FateUi.BoneDim);
                orderBadges[card] = badge;
                slot.Add(badge);
                row.Add(slot);
            }

            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.justifyContent = Justify.Center;
            panel.Add(buttons);

            if (scry.AllowReorder)
            {
                buttons.Add(FateUi.MakeButton("SET THE ORDER", () =>
                {
                    if (_scryOrder.Count > 0)
                    {
                        Session.Deck.SetTopOrder(_scryOrder);
                        _log.Append("You reorder the top of your deck.", FateUi.Verdigris, bold: true);
                    }

                    CloseOverlay();
                }, FateUi.Verdigris, 15));
            }

            buttons.Add(FateUi.MakeButton("LEAVE AS IS", () =>
            {
                _log.Append("You memorize the top of your deck.", FateUi.BoneDim);
                CloseOverlay();
            }, FateUi.BoneDim, 15));
        }

        // ---------------------------------------------------------------- session choice requests

        private bool OnZoneChoice(ZoneChoiceRequest request)
        {
            switch (request.Kind)
            {
                case ZoneChoiceKind.HealWounds:
                    if (Session.Deck.Wound.Count == 0)
                    {
                        return true;
                    }

                    _woundPicksRemaining = Mathf.Min(request.Count, Session.Deck.Wound.Count);
                    UpdateWoundPrompt();
                    RefreshTableau();
                    return true;

                case ZoneChoiceKind.ExileFromDiscard:
                    ShowZonePick(Session.Deck.Discard, "Exile a card from your discard pile",
                        card => Session.Deck.ExileCard(Session.Deck.Discard, card));
                    return true;

                case ZoneChoiceKind.StackFromDiscard:
                    ShowZonePick(Session.Deck.Discard, "Put a card on top of your deck",
                        card => Session.Deck.StackOnTop(card));
                    return true;
            }

            return false;
        }
    }
}
