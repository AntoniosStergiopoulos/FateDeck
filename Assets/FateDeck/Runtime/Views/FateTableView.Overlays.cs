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
            UiFx.Pop(panel, 0.86f, 0.2f);

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
                panel.Add(FateUi.Text("Empty — the next flip shuffles the discard back and the House "
                    + "charges Interest.", 14, FateUi.BoneDim));
            }

            panel.Add(FateUi.MakeButton("CLOSE", CloseOverlay, FateUi.BoneDim, 14));
        }

        // ---------------------------------------------------------------- generic pile inspector

        /// <summary>Read-only inspector for any fate pile (discard, escrow, exile).</summary>
        private void ShowZoneOverlay(CardZone zone, string title, bool showOrder)
        {
            if (IsOverlayOpen)
            {
                return;
            }

            VisualElement panel = OpenOverlay(title);
            if (zone.Count == 0)
            {
                panel.Add(FateUi.Text("Nothing here.", 14, FateUi.BoneDim));
                panel.Add(FateUi.MakeButton("CLOSE", CloseOverlay, FateUi.BoneDim, 14));
                return;
            }

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.maxHeight = 430;
            panel.Add(scroll);

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.maxWidth = 840;
            scroll.Add(grid);

            if (showOrder)
            {
                for (int i = zone.Count - 1; i >= 0; i--)
                {
                    var slot = FateUi.Column(0);
                    slot.style.alignItems = Align.Center;
                    slot.Add(FateUi.Text(i == zone.Count - 1 ? "newest" : $"#{zone.Count - i}", 10, FateUi.BoneDim));
                    slot.Add(CardElementBuilder.ForceTile(_catalog, zone.Cards[i], 56));
                    grid.Add(slot);
                }
            }
            else
            {
                foreach (CardInstance card in zone.Cards)
                {
                    grid.Add(CardElementBuilder.ForceTile(_catalog, card, 56));
                }
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

        // ---------------------------------------------------------------- rules & glossary

        private void ShowHelpOverlay()
        {
            if (IsOverlayOpen)
            {
                return;
            }

            VisualElement panel = OpenOverlay("HOW THE TABLE WORKS");
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.maxHeight = 520;
            scroll.style.maxWidth = 840;
            panel.Add(scroll);

            void Rule(string title, string body)
            {
                Label heading = FateUi.Text(title, 14, FateUi.GoldLeaf);
                heading.style.unityFontStyleAndWeight = FontStyle.Bold;
                heading.style.marginTop = 6;
                scroll.Add(heading);
                scroll.Add(FateUi.Text(body, 13, FateUi.Bone));
            }

            Rule("YOU OWE THE HOUSE",
                "You died owing Fate. The House collateralized your soul into this deck - it is your "
                + "health, your luck and your build in one stack. Damage tears cards off the top into "
                + "ESCROW. If you must flip or mill and both draw and discard are empty, the House "
                + "forecloses: you die.");
            Rule("EVERY FLIP IS YOURS",
                "There is ONE deck at this table - yours. When an enemy attacks, it flips YOUR top "
                + "card and that card's Enemy-Action law applies (written from your point of view: "
                + "hover any force to read exactly what it does on each side). The Odds Panel on the "
                + "left prices every flip before you commit. No surprise math, ever.");
            Rule("THE POCKET",
                "When one of YOUR actions flips, you may POCKET IT: bank the card, and the action "
                + "resolves at base value. Later, during any pre-flip window (yours or an enemy's), "
                + "play a pocketed card to REPLACE that flip entirely. HONOR IT applies the law now.");
            Rule("INTEREST",
                "When the draw pile empties, the discard shuffles back in - and the House charges "
                + "Interest: +1 Debt card. The draw pile shows exactly when it's due. Thin, fast "
                + "decks cycle more and pay more; healing out of Escrow slows the clock.");
            Rule("DEBT & GRIT",
                "Debt is the House's lien: your actions collapse, enemy blows land +2 heavier. It "
                + "cannot be pocketed. But milled Debt burns off the books forever, and every Debt "
                + "that SURFACES banks you 1 Grit - at 3, spend it (hero panel) on Scry 2, +2 Force, "
                + "or a free mend. The House's insults harden you.");
            Rule("ESCROW & HEALING",
                "Escrowed cards are not lost - healing returns the ones YOU choose to your deck. "
                + "Which card you take back is a build decision: every heal is also a probability edit.");
            Rule("COMBAT",
                "One Main Action per turn: STRIKE or GUARD (both flip), plus free charms and pocket "
                + "plays. Outnumbered (2+ enemies): your Guard also strikes your target for 2. A "
                + "voided action refunds your Main Action once per fight. Fleeing costs a card and "
                + "the room's purse.");
            Rule("STATUSES",
                "Burn N: at round end the burning side loses N (you mill; enemies take damage), then "
                + "it ticks down 1. Weak: the next actions resolve at -2 Force, one stack per action. "
                + "Block soaks damage until the owner's next turn.");
            Rule("THE COLLECTOR",
                "The boss appraises up to 3 copies of your most-numerous draw-pile force into its "
                + "Mantle (+1 Force to its attacks per 3 held). Hits of 5+ shake a card loose; killing "
                + "it returns everything. It cannot see your discard, pocket or escrow.");
            Rule("YOUR CONTROL KIT",
                "Nine verbs bend fate: POCKET a reveal · PLAY a pocketed card into any pre-flip "
                + "window · SCRY and reorder · STACK a known card on top (Second Sleeve) · DRAW-2 "
                + "and choose · SCULPT the composition (every purchase, exile and heal is a "
                + "probability edit) · TARGET who pays · spend GRIT · and FLEE when the math is bad.");

            Label glossaryTitle = FateUi.Text("THE FORCES", 15, FateUi.GoldLeaf);
            glossaryTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            glossaryTitle.style.marginTop = 10;
            scroll.Add(glossaryTitle);

            foreach (MetadataEntry force in AllKnownForces())
            {
                var line = new VisualElement();
                line.style.flexDirection = FlexDirection.Row;
                line.style.marginBottom = 3;
                Color color = ForceColor(force);
                Label name = FateUi.Text($"{force.GetText(_catalog.ForceGlyphField)}  {force.name}", 13, color);
                name.style.width = 110;
                name.style.unityFontStyleAndWeight = FontStyle.Bold;
                name.style.flexShrink = 0;
                line.Add(name);
                Label text = FateUi.Text(force.GetText(_catalog.DescriptionField), 12, FateUi.BoneDim);
                text.style.flexShrink = 1;
                line.Add(text);
                scroll.Add(line);
            }

            panel.Add(FateUi.MakeButton("BACK TO THE TABLE", CloseOverlay, FateUi.BoneDim, 14));
        }

        private IEnumerable<MetadataEntry> AllKnownForces()
        {
            MetadataEntry[] forces =
            {
                _catalog.Iron, _catalog.IronPlus, _catalog.Flame, _catalog.FlamePlus,
                _catalog.Decay, _catalog.DecayPlus, _catalog.Fortune, _catalog.FortunePlus,
                _catalog.Echo, _catalog.Void, _catalog.Doom,
                _catalog.Tempest, _catalog.TempestPlus, _catalog.Serpent, _catalog.SerpentPlus,
                _catalog.Glass, _catalog.Gloom, _catalog.Key, _catalog.Mirror,
                _catalog.Anchor, _catalog.Rust, _catalog.Wisp
            };

            foreach (MetadataEntry force in forces)
            {
                if (force != null)
                {
                    yield return force;
                }
            }
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

                case ZoneChoiceKind.UpgradeFromDraw:
                    ShowZonePick(Session.Deck.Draw, "Choose a card to sharpen to its + tier",
                        card => Session.UpgradeFateCard(card),
                        card => _catalog.IsBasicForce(_catalog.ForceOf(card)));
                    return true;
            }

            return false;
        }
    }
}
