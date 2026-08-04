using AStergio.OmniCard.Runtime.Cards.Data;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.Game.Decks;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Core;
using FateDeck.Runtime.Run;
using UnityEngine;
using UnityEngine.UIElements;

namespace FateDeck.Runtime.Views
{
    public sealed partial class FateTableView
    {
        private void RebuildScreen()
        {
            if (Session == null)
            {
                return;
            }

            _screenHost.Clear();
            switch (_run.Screen)
            {
                case RunScreen.Doors: BuildDoorsScreen(); break;
                case RunScreen.Combat: BuildCombatScreen(); break;
                case RunScreen.Chest: BuildChestScreen(); break;
                case RunScreen.Shrine: BuildShrineScreen(); break;
                case RunScreen.Event: BuildEventScreen(); break;
                case RunScreen.Rest: BuildRestScreen(); break;
                case RunScreen.Shop: BuildShopScreen(); break;
                case RunScreen.Rewards: BuildRewardsScreen(); break;
                case RunScreen.Dead: BuildDeathScreen(); break;
                case RunScreen.Victory: BuildVictoryScreen(); break;
            }

            BuildPromptArea();
            RefreshTableau();
            RefreshHud();
        }

        // ---------------------------------------------------------------- hero select

        private void BuildHeroSelectScreen()
        {
            _screenHost.Clear();
            _promptHost.Clear();

            var stage = FateUi.Column();
            stage.style.flexGrow = 1;
            stage.style.alignItems = Align.Center;
            stage.style.justifyContent = Justify.Center;
            _screenHost.Add(stage);

            stage.Add(FateUi.Heading("CHOOSE WHO SITS DOWN", 30, FateUi.Bone));
            Label hint = FateUi.Text("Each player is a different shape of luck: a deck, a passive, a pocket.",
                14, FateUi.BoneDim);
            hint.style.marginBottom = 10;
            stage.Add(hint);

            var seedRow = new VisualElement();
            seedRow.style.flexDirection = FlexDirection.Row;
            seedRow.style.alignItems = Align.Center;
            seedRow.style.marginBottom = 14;
            Label seedLabel = FateUi.Text("SEED", 12, FateUi.GoldLeaf);
            seedLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            seedLabel.style.marginRight = 6;
            seedRow.Add(seedLabel);
            var seedField = new TextField();
            seedField.style.width = 180;
            seedField.value = string.Empty;
            seedRow.Add(seedField);
            Label seedHint = FateUi.Text("  optional — same seed, same run. Words work too.", 11, FateUi.BoneDim);
            seedRow.Add(seedHint);
            FateTip.Bind(seedRow,
                "Type a number (or any word) to play a deterministic table: the same seed deals "
                + "the same doors, enemies, chests and flips every time. Leave empty for a random "
                + "run. The current run's seed is printed in the event log and on the end screens.");
            stage.Add(seedRow);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.Wrap;
            row.style.justifyContent = Justify.Center;
            stage.Add(row);

            foreach (CardDefinition hero in _catalog.Heroes)
            {
                if (hero != null)
                {
                    row.Add(BuildHeroCard(hero, seedField));
                }
            }

            UiFx.FadeSlideIn(stage, -16f, 0.32f);
        }

        private VisualElement BuildHeroCard(CardDefinition hero, TextField seedField)
        {
            VisualElement panel = FateUi.MakePanel();
            panel.style.width = 252;
            panel.style.marginLeft = 8;
            panel.style.marginRight = 8;
            panel.style.marginBottom = 10;
            panel.style.justifyContent = Justify.SpaceBetween;

            Label name = FateUi.Text(hero.name, 17, FateUi.GoldLeaf);
            name.style.unityFontStyleAndWeight = FontStyle.Bold;
            panel.Add(name);

            string passive = hero.GetText(_catalog.DescriptionField);
            if (!string.IsNullOrEmpty(passive))
            {
                Label passiveLabel = FateUi.Text(passive, 13, FateUi.Bone);
                passiveLabel.style.marginTop = 4;
                panel.Add(passiveLabel);
            }

            double slots = hero.GetNumber(_catalog.PocketSlotsField);
            panel.Add(FateUi.Text($"Pocket slots: {slots:0}", 12, FateUi.BoneDim));

            var deckList = FateUi.Column(0);
            deckList.style.marginTop = 6;
            deckList.style.marginBottom = 8;
            panel.Add(deckList);
            if (hero.GetObject(_catalog.StartingDeckField) is DeckDefinition deck)
            {
                int total = 0;
                foreach (DeckEntry entry in deck.Cards)
                {
                    if (entry?.Card == null)
                    {
                        continue;
                    }

                    total += entry.Count;
                    var force = entry.Card.GetObject(_catalog.ForceField) as MetadataEntry;
                    Color color = CardElementBuilder.ForceColor(_catalog, force);
                    Label line = FateUi.Text($"{entry.Count}x {entry.Card.name}", 12, color);
                    line.style.unityFontStyleAndWeight = FontStyle.Bold;
                    deckList.Add(line);
                }

                deckList.Insert(0, FateUi.Text($"Starting deck — {total} cards:", 12, FateUi.BoneDim));
            }

            panel.Add(FateUi.MakeButton("DEAL ME IN",
                () => StartNewRun(hero, ParseSeedInput(seedField.value)), FateUi.GoldLeaf, 15));
            return panel;
        }

        private VisualElement CenterStage(string heading = null, Color? headingColor = null)
        {
            var stage = FateUi.Column();
            stage.style.flexGrow = 1;
            stage.style.alignItems = Align.Center;
            stage.style.justifyContent = Justify.Center;
            if (!string.IsNullOrEmpty(heading))
            {
                Label title = FateUi.Heading(heading, 30, headingColor ?? FateUi.Bone);
                title.style.marginBottom = 8;
                stage.Add(title);
            }

            _screenHost.Add(stage);
            return stage;
        }

        // ---------------------------------------------------------------- doors

        private void BuildDoorsScreen()
        {
            // Unspent wound picks resolve now (oldest first) so the granted heal is never lost.
            if (_woundPicksRemaining > 0 && Session.Deck.Wound.Count > 0)
            {
                int healed = Session.Deck.HealWounds(_woundPicksRemaining);
                if (healed > 0)
                {
                    _log.Append($"Unclaimed mending resolves on its own: {healed} wound"
                        + $"{(healed == 1 ? "" : "s")} return to your deck.", FateUi.Verdigris);
                }
            }

            _woundPicksRemaining = 0;
            VisualElement stage = CenterStage($"STEP {_run.Step} — PICK A DOOR");
            Label hint = FateUi.Text("Doors show their contents up front. Unpicked doors vanish.", 14, FateUi.BoneDim);
            hint.style.marginBottom = 14;
            stage.Add(hint);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.Center;
            row.style.alignItems = Align.Stretch;
            stage.Add(row);

            for (int i = 0; i < _run.Doors.Count; i++)
            {
                RoomDefinition room = _run.Doors[i];
                int index = i;
                VisualElement door = FateUi.MakePanel();
                door.style.width = 250;
                door.style.marginLeft = 8;
                door.style.marginRight = 8;
                door.style.justifyContent = Justify.SpaceBetween;

                Label title = FateUi.Text(room.DoorLabel(), 15, FateUi.GoldLeaf);
                title.style.unityFontStyleAndWeight = FontStyle.Bold;
                door.Add(title);

                Label blurb = FateUi.Text(room.Blurb, 13, FateUi.BoneDim);
                blurb.style.marginTop = 6;
                blurb.style.marginBottom = 10;
                blurb.style.flexGrow = 1;
                door.Add(blurb);

                string stakes = DoorStakes(room);
                if (!string.IsNullOrEmpty(stakes))
                {
                    Label stakesLabel = FateUi.Text(stakes, 12, FateUi.GoldLeaf);
                    stakesLabel.style.marginBottom = 8;
                    FateTip.Bind(stakesLabel,
                        "Every door states its stakes up front. Purse = bounty gold for winning; "
                        + "extra enemies pay a squad bonus on top.");
                    door.Add(stakesLabel);
                }

                door.Add(FateUi.MakeButton("ENTER", () =>
                {
                    _log.Append($"You take the door: {room.DoorLabel()}", FateUi.Bone, bold: true);
                    _run.ChooseDoor(index);
                }, FateUi.GoldLeaf, 15));
                row.Add(door);
            }
        }

        /// <summary>The advertised stakes line for a door - visible risk, visible pay.</summary>
        private string DoorStakes(RoomDefinition room)
        {
            switch (room)
            {
                case FightRoomDefinition fight when fight.Encounter != null:
                {
                    int purse = 0;
                    int enemies = 0;
                    foreach (DeckEntry entry in fight.Encounter.Cards)
                    {
                        if (entry?.Card == null)
                        {
                            continue;
                        }

                        purse += (int)(entry.Card.GetNumber(_catalog.BountyField) * entry.Count);
                        enemies += entry.Count;
                    }

                    if (enemies > 1)
                    {
                        purse += (enemies - 1) * Session.Rules.SquadPursePerExtraEnemy;
                    }

                    string charm = fight.CharmDropChance > 0
                        ? $" · charm {fight.CharmDropChance:P0}"
                        : string.Empty;
                    string relic = fight.IsElite ? " · RELIC choice" : string.Empty;
                    return $"Purse: {purse}g{charm}{relic}";
                }

                case ChestRoomDefinition chest:
                {
                    double baseGold = chest.Locked
                        ? Session.Rules.LockedChestBaseGold
                        : Session.Rules.ChestBaseGold;
                    return $"Base haul: {baseGold:0}g{(chest.Locked ? " (locked)" : "")}";
                }

                default:
                    return null;
            }
        }

        // ---------------------------------------------------------------- chest

        private void BuildChestScreen()
        {
            var chest = _run.CurrentRoom as ChestRoomDefinition;
            if (chest == null)
            {
                return;
            }

            VisualElement stage = CenterStage(chest.Locked ? "A LOCKED CHEST" : "A CHEST", FateUi.GoldLeaf);
            Label blurb = FateUi.Text(chest.Blurb, 14, FateUi.BoneDim);
            blurb.style.maxWidth = 520;
            blurb.style.marginBottom = 12;
            stage.Add(blurb);

            if (!_run.ChestOpened && Session.Phase == FateResolutionPhase.Idle)
            {
                Label explain = FateUi.Text(
                    "Opening the chest flips fate — the odds table on the left shows every outcome.",
                    14, FateUi.Bone);
                explain.style.marginBottom = 10;
                stage.Add(explain);

                var buttons = new VisualElement();
                buttons.style.flexDirection = FlexDirection.Row;
                stage.Add(buttons);
                buttons.Add(FateUi.MakeButton("FLIP THE LID", () => _run.OpenChest(useKey: false), FateUi.Ember, 16));
                if (chest.Locked && Session.Keys > 0)
                {
                    buttons.Add(FateUi.MakeButton("USE A KEY, THEN FLIP",
                        () => _run.OpenChest(useKey: true), FateUi.Verdigris, 16));
                }

                buttons.Add(FateUi.MakeButton("WALK AWAY", () => _run.CompleteRoom(), FateUi.BoneDim, 14));
            }
            else if (_run.ChestOpened && Session.Phase == FateResolutionPhase.Idle)
            {
                stage.Add(FateUi.Text("The lid settles. The log tells the tale.", 15, FateUi.Bone));
                stage.Add(FateUi.MakeButton("CONTINUE", () => _run.CompleteRoom(), FateUi.GoldLeaf, 16));
            }
        }

        // ---------------------------------------------------------------- shrine

        private void BuildShrineScreen()
        {
            var shrine = _run.CurrentRoom as ShrineRoomDefinition;
            if (shrine == null)
            {
                return;
            }

            VisualElement stage = CenterStage(shrine.DoorLabel().ToUpperInvariant(), FateUi.Verdigris);
            Label blurb = FateUi.Text(shrine.Blurb, 14, FateUi.BoneDim);
            blurb.style.maxWidth = 520;
            blurb.style.marginBottom = 12;
            stage.Add(blurb);

            switch (shrine.Kind)
            {
                case ShrineKind.Ash:
                    if (_run.ShrineExilesRemaining > 0)
                    {
                        stage.Add(FateUi.Text(
                            $"Exile 1 card, free. Debt clings — it costs {Session.Rules.DoomExileShrinePrice}g to burn.",
                            14, FateUi.Bone));
                        var buttons = new VisualElement();
                        buttons.style.flexDirection = FlexDirection.Row;
                        stage.Add(buttons);
                        buttons.Add(FateUi.MakeButton("BURN FROM DRAW PILE",
                            () => ShowZonePick(Session.Deck.Draw, "Choose a card to exile forever",
                                card => _run.ShrineExile(Session.Deck.Draw, card)), FateUi.Ember, 14));
                        buttons.Add(FateUi.MakeButton("BURN FROM DISCARD",
                            () => ShowZonePick(Session.Deck.Discard, "Choose a card to exile forever",
                                card => _run.ShrineExile(Session.Deck.Discard, card)), FateUi.Ember, 14));
                    }
                    else
                    {
                        stage.Add(FateUi.Text("The ash settles, satisfied.", 14, FateUi.BoneDim));
                    }

                    break;

                case ShrineKind.Forge:
                    if (_run.ForgeGiftsRemaining > 0)
                    {
                        stage.Add(FateUi.Text(
                            $"The Forge offers {_run.ForgeGiftsRemaining} Flame for your deck. "
                            + "Flame burns whoever the action targets — aim accordingly.", 14, FateUi.Bone));
                        stage.Add(FateUi.MakeButton("ACCEPT A FLAME", () =>
                        {
                            _run.ForgeGift();
                            _log.Append("A Flame is shuffled into your draw pile.", FateUi.Ember, bold: true);
                        }, FateUi.Ember, 16));
                    }
                    else
                    {
                        stage.Add(FateUi.Text("The Forge cools.", 14, FateUi.BoneDim));
                    }

                    break;

                case ShrineKind.Stitches:
                    if (_run.ShrineHealsRemaining > 0 && Session.Deck.Wound.Count > 0)
                    {
                        stage.Add(FateUi.Text(
                            $"Return up to {_run.ShrineHealsRemaining} wounds — click the highlighted cards "
                            + "in Escrow below.", 14, FateUi.Bone));
                        _woundPicksRemaining = _run.ShrineHealsRemaining;
                        RefreshTableau();
                    }
                    else
                    {
                        stage.Add(FateUi.Text(Session.Deck.Wound.Count == 0
                            ? "You carry no wounds. The needle rests."
                            : "The thread runs out.", 14, FateUi.BoneDim));
                    }

                    break;
            }

            stage.Add(FateUi.MakeButton("LEAVE THE SHRINE", () =>
            {
                _woundPicksRemaining = 0;
                _run.CompleteRoom();
            }, FateUi.BoneDim, 14));
        }

        // ---------------------------------------------------------------- event

        private void BuildEventScreen()
        {
            EventDefinition active = _run.ActiveEvent;
            if (active == null)
            {
                VisualElement done = CenterStage();
                if (!string.IsNullOrEmpty(_run.LastEventResult))
                {
                    Label result = FateUi.Text(_run.LastEventResult, 16, FateUi.GoldLeaf);
                    result.style.maxWidth = 560;
                    result.style.marginBottom = 10;
                    done.Add(result);
                }

                done.Add(FateUi.MakeButton("CONTINUE", () => _run.CompleteRoom(), FateUi.GoldLeaf, 16));
                return;
            }

            VisualElement stage = CenterStage(active.name.ToUpperInvariant(), FateUi.Violet);
            Label intro = FateUi.Text(active.Intro, 14, FateUi.Bone);
            intro.style.maxWidth = 560;
            intro.style.marginBottom = 10;
            stage.Add(intro);

            if (!string.IsNullOrEmpty(_run.LastEventResult))
            {
                Label result = FateUi.Text(_run.LastEventResult, 14, FateUi.GoldLeaf);
                result.style.maxWidth = 560;
                result.style.marginBottom = 10;
                stage.Add(result);
            }

            if (Session.Phase != FateResolutionPhase.Idle)
            {
                stage.Add(FateUi.Text("…fate is being consulted…", 13, FateUi.Violet));
                return;
            }

            for (int i = 0; i < active.Options.Count; i++)
            {
                EventOption option = active.Options[i];
                int index = i;
                string cost = option.GoldCost > 0 ? $"  ({option.GoldCost}g)" : string.Empty;
                if (option.KeyCost > 0)
                {
                    cost += $"  ({option.KeyCost} Key{(option.KeyCost == 1 ? "" : "s")})";
                }

                bool affordable = Session.Gold >= option.GoldCost && Session.Keys >= option.KeyCost;
                Button button = FateUi.MakeButton($"{option.Label}{cost}",
                    () => _run.TakeEventOption(index),
                    affordable ? FateUi.GoldLeaf : FateUi.BoneDim, 14, affordable);
                button.style.width = 480;
                if (!affordable)
                {
                    FateTip.Bind(button, option.KeyCost > Session.Keys
                        ? "You need a Key for this."
                        : "Not enough gold.");
                }

                stage.Add(button);
            }

            stage.Add(FateUi.MakeButton("WALK AWAY", () => _run.CompleteRoom(), FateUi.BoneDim, 13));
        }

        // ---------------------------------------------------------------- rest

        private void BuildRestScreen()
        {
            VisualElement stage = CenterStage("A QUIET LANDING", FateUi.Verdigris);
            stage.Add(FateUi.Text("Choose one. The House allows a single kindness per rest.", 14, FateUi.BoneDim));

            if (!_run.RestUsed)
            {
                var options = FateUi.Column(4);
                options.style.marginTop = 10;
                stage.Add(options);
                Button mend = FateUi.MakeButton($"MEND — return {Session.Rules.RestHeal} wound cards to your deck",
                    () =>
                    {
                        _run.RestMend();
                        _woundPicksRemaining = Session.Deck.Wound.Count > 0
                            ? Mathf.Min(Session.Rules.RestHeal, Session.Deck.Wound.Count)
                            : 0;
                        RefreshTableau();
                        MarkScreenDirty();
                    }, FateUi.Verdigris, 15);
                mend.style.width = 520;
                options.Add(mend);

                Button sharpen = FateUi.MakeButton("SHARPEN — upgrade 1 basic card to its + version",
                    () => ShowZonePick(Session.Deck.Draw, "Choose a basic card to upgrade",
                        card => _run.RestSharpen(card),
                        card => _catalog.IsBasicForce(_catalog.ForceOf(card))), FateUi.GoldLeaf, 15);
                sharpen.style.width = 520;
                options.Add(sharpen);

                Button cleanse = FateUi.MakeButton(
                    $"CLEANSE — exile 1 card (Debt costs {Session.Rules.DoomCleansePrice}g)",
                    () => ShowZonePick(Session.Deck.Draw, "Choose a card to exile",
                        card => _run.RestCleanse(Session.Deck.Draw, card)), FateUi.Ember, 15);
                cleanse.style.width = 520;
                options.Add(cleanse);
            }
            else
            {
                stage.Add(FateUi.Text("You feel almost whole.", 15, FateUi.Bone));
            }

            bool guaranteedShopNext = _run.Step == Session.Rules.TrackSteps - 1;
            stage.Add(FateUi.MakeButton(guaranteedShopNext ? "ON TO THE SHOP" : "CONTINUE", () =>
            {
                _woundPicksRemaining = 0;
                if (guaranteedShopNext)
                {
                    _run.ContinueRestToShop();
                }
                else
                {
                    _run.CompleteRoom();
                }
            }, FateUi.BoneDim, 15));
        }

        // ---------------------------------------------------------------- shop

        private void BuildShopScreen()
        {
            ShopService shop = _run.Shop;
            if (shop == null)
            {
                return;
            }

            VisualElement stage = CenterStage("THE SHOP", FateUi.GoldLeaf);
            Label motto = FateUi.Text("\"Cards are hit points. Hit points are odds. Spend wisely.\"",
                13, FateUi.BoneDim);
            motto.style.marginBottom = 10;
            stage.Add(motto);

            var list = FateUi.Column(0);
            list.style.width = 660;
            stage.Add(list);

            foreach (ShopItem item in shop.Stock)
            {
                list.Add(ShopRow(shop, item));
            }

            stage.Add(FateUi.MakeButton("LEAVE", () => _run.CompleteRoom(), FateUi.BoneDim, 15));
        }

        /// <summary>A labeled chip that says what KIND of thing a purchasable is.</summary>
        private VisualElement KindBadge(string text, Color color, string tip)
        {
            VisualElement chip = FateUi.Chip(text, color, 11);
            chip.style.marginRight = 8;
            chip.style.flexShrink = 0;
            FateTip.Bind(chip, tip);
            return chip;
        }

        private VisualElement ShopRow(ShopService shop, ShopItem item)
        {
            VisualElement row = FateUi.MakePanel();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 5;
            if (item.Sold)
            {
                row.style.opacity = 0.45f;
            }

            string name;
            string description;
            switch (item.Kind)
            {
                case ShopItemKind.FateCard:
                {
                    var force = item.Card != null
                        ? item.Card.GetObject(_catalog.ForceField) as AStergio.OmniCard.Runtime.Cards.MetaData.MetadataEntry
                        : null;
                    Color color = CardElementBuilder.ForceColor(_catalog, force);
                    row.Add(KindBadge("CARD", color,
                        "A fate card. It joins your DISCARD pile and enters your luck at the next reshuffle."));
                    name = item.Card != null ? item.Card.name : "Card";
                    description = (item.Card != null ? item.Card.GetText(_catalog.DescriptionField) : null)
                        + "  (joins your discard pile)";
                    break;
                }

                case ShopItemKind.Relic:
                    row.Add(KindBadge("RELIC", FateUi.GoldLeaf,
                        "A relic: a permanent law, always active for the rest of the run."));
                    name = item.Card != null ? item.Card.name : "Relic";
                    description = item.Card != null ? item.Card.GetText(_catalog.DescriptionField) : null;
                    break;

                case ShopItemKind.Charm:
                    row.Add(KindBadge("CHARM", FateUi.Violet,
                        "A charm: a one-shot consumable. It sits in your tableau - click it there to "
                        + "use it. Free unless it costs your Main Action."));
                    name = item.Card != null ? item.Card.name : "Charm";
                    description = item.Card != null ? item.Card.GetText(_catalog.DescriptionField) : null;
                    break;

                case ShopItemKind.Key:
                    row.Add(KindBadge("SERVICE", FateUi.Verdigris, "Takes effect immediately."));
                    name = "Key";
                    description = "Opens one locked chest politely — no flip, no Flame gamble.";
                    break;

                case ShopItemKind.Tonic:
                    row.Add(KindBadge("SERVICE", FateUi.Verdigris, "Takes effect immediately."));
                    name = "Tonic";
                    description = $"Heal {Session.Rules.TonicHeal}: choose wound cards to return to your deck.";
                    break;

                default:
                    row.Add(KindBadge("SERVICE", FateUi.Verdigris, "Takes effect immediately."));
                    name = "Surgery";
                    description = "Exile any card from your draw pile. The price rises with each surgery; "
                        + "Debt costs double.";
                    break;
            }

            var mid = FateUi.Column(0);
            mid.style.flexGrow = 1;
            mid.style.flexShrink = 1;
            Label nameLabel = FateUi.Text(name, 14, FateUi.Bone);
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            mid.Add(nameLabel);
            if (!string.IsNullOrEmpty(description))
            {
                mid.Add(FateUi.Text(description, 12, FateUi.BoneDim));
            }

            row.Add(mid);

            if (item.Sold)
            {
                Label sold = FateUi.Text("SOLD", 13, FateUi.BoneDim);
                sold.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(sold);
                return row;
            }

            ShopItem captured = item;
            bool affordable = Session.Gold >= captured.Price;
            Button buy = FateUi.MakeButton($"BUY {captured.Price}g", () =>
            {
                if (captured.Kind == ShopItemKind.Surgery)
                {
                    ShowZonePick(Session.Deck.Draw, "Choose a card for surgery (exile forever)",
                        card => shop.BuySurgery(Session.Deck.Draw, card));
                    return;
                }

                if (shop.Buy(captured))
                {
                    _log.Append($"Bought: {captured.Label()}", FateUi.GoldLeaf, bold: true);
                }
                else
                {
                    _log.Append($"Can't buy {captured.Label()} right now.", FateUi.BoneDim);
                }

                RefreshTableau();
                MarkScreenDirty();
            }, affordable ? FateUi.GoldLeaf : FateUi.BoneDim, 13, affordable);
            buy.style.flexShrink = 0;
            if (!affordable)
            {
                FateTip.Bind(buy, "Not enough gold.");
            }

            row.Add(buy);
            return row;
        }

        // ---------------------------------------------------------------- rewards

        private void BuildRewardsScreen()
        {
            VisualElement stage = CenterStage("THE TABLE PAYS OUT", FateUi.GoldLeaf);

            if (_run.RelicChoices.Count > 0)
            {
                stage.Add(FateUi.Text("Choose one relic — a permanent law for the rest of the run:",
                    15, FateUi.Bone));
                var list = FateUi.Column(0);
                list.style.marginTop = 8;
                list.style.width = 660;
                stage.Add(list);
                for (int i = 0; i < _run.RelicChoices.Count; i++)
                {
                    CardDefinition relic = _run.RelicChoices[i];
                    int index = i;
                    string rules = relic.GetText(_catalog.DescriptionField);

                    VisualElement row = FateUi.MakePanel();
                    row.style.flexDirection = FlexDirection.Row;
                    row.style.alignItems = Align.Center;
                    row.style.marginBottom = 5;
                    row.Add(KindBadge("RELIC", FateUi.GoldLeaf,
                        "A relic: a permanent law, always active for the rest of the run."));

                    var mid = FateUi.Column(0);
                    mid.style.flexGrow = 1;
                    mid.style.flexShrink = 1;
                    Label nameLabel = FateUi.Text(relic.name, 14, FateUi.GoldLeaf);
                    nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    mid.Add(nameLabel);
                    mid.Add(FateUi.Text(rules, 12, FateUi.BoneDim));
                    row.Add(mid);

                    Button pick = FateUi.MakeButton("TAKE", () =>
                    {
                        _log.Append($"Relic taken: {relic.name} — {rules}", FateUi.GoldLeaf, bold: true);
                        _run.PickRelic(index);
                    }, FateUi.GoldLeaf, 14);
                    pick.style.flexShrink = 0;
                    row.Add(pick);
                    list.Add(row);
                }

                return;
            }

            if (_run.CharmReward != null)
            {
                string rules = _run.CharmReward.GetText(_catalog.DescriptionField);
                VisualElement row = FateUi.MakePanel();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.width = 620;
                row.Add(KindBadge("CHARM", FateUi.Violet,
                    "A charm: a one-shot consumable. It sits in your tableau - click it there to use it."));
                var mid = FateUi.Column(0);
                mid.style.flexGrow = 1;
                mid.style.flexShrink = 1;
                Label nameLabel = FateUi.Text(_run.CharmReward.name, 14, FateUi.Bone);
                nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                mid.Add(nameLabel);
                mid.Add(FateUi.Text(rules, 12, FateUi.BoneDim));
                row.Add(mid);
                stage.Add(FateUi.Text("A charm falls from the wreckage:", 15, FateUi.Bone));
                stage.Add(row);

                var buttons = new VisualElement();
                buttons.style.flexDirection = FlexDirection.Row;
                buttons.style.marginTop = 8;
                stage.Add(buttons);
                buttons.Add(FateUi.MakeButton("TAKE IT", () =>
                {
                    _log.Append($"Charm taken: {_run.CharmReward.name}", FateUi.Violet, bold: true);
                    _run.TakeCharmReward();
                    RefreshTableau();
                }, FateUi.Verdigris, 15));
                buttons.Add(FateUi.MakeButton("LEAVE IT", () => _run.CompleteRoom(), FateUi.BoneDim, 15));
                return;
            }

            stage.Add(FateUi.MakeButton("CONTINUE", () => _run.CompleteRoom(), FateUi.GoldLeaf, 16));
        }

        // ---------------------------------------------------------------- death & victory

        private void BuildDeathScreen()
        {
            FateSession session = Session;
            VisualElement stage = CenterStage("THE HONEST LEDGER", FateUi.Ember);
            stage.Add(FateUi.Text("Your last card is spent. The House keeps the lien.", 15, FateUi.BoneDim));

            VisualElement ledger = FateUi.MakePanel();
            ledger.style.marginTop = 12;
            ledger.style.width = 560;
            stage.Add(ledger);

            FateAction fatal = session.LastFatalAction;
            if (fatal != null)
            {
                string forces = fatal.AppliedForces.Count > 0
                    ? string.Join(", ", ForceNames(fatal))
                    : "no flip";
                ledger.Add(FateUi.Text($"Fatal moment: {fatal.Name} — fate said {forces}.", 14, FateUi.Bone));
            }

            double doomRate = session.TotalFlipsThisRun > 0
                ? (double)session.DoomFlipsThisRun / session.TotalFlipsThisRun * 100.0
                : 0.0;
            ledger.Add(FateUi.Text(
                $"Debt surfaced {session.DoomFlipsThisRun} times in {session.TotalFlipsThisRun} flips ({doomRate:0.#}%).",
                14, FateUi.Bone));
            ledger.Add(FateUi.Text($"Reshuffles paid: {session.Deck.ReshuffleCount} · "
                + $"{session.Deck.Exile.Count} cards exiled · {session.Gold}g left on the table.", 14, FateUi.BoneDim));
            ledger.Add(FateUi.Text($"Table seed: {_run.OriginalSeed} — enter it on the hero screen "
                + "to face this exact run again.", 13, FateUi.BoneDim));

            stage.Add(FateUi.MakeButton("THE DECK RE-FORMS", ShowHeroSelect, FateUi.Ember, 17));
        }

        private System.Collections.Generic.IEnumerable<string> ForceNames(FateAction action)
        {
            foreach (var force in action.AppliedForces)
            {
                yield return force != null ? force.name : "?";
            }
        }

        private void BuildVictoryScreen()
        {
            FateSession session = Session;
            VisualElement stage = CenterStage("THE COLLECTOR IS COLLECTED", FateUi.GoldLeaf);
            stage.Add(FateUi.Text("Biome 1 falls. The Mire and the Vault are still being dealt in.",
                15, FateUi.BoneDim));
            stage.Add(FateUi.Text(
                $"{session.TotalFlipsThisRun} flips · {session.DoomFlipsThisRun} Debt · {session.Gold}g banked · "
                + $"{session.Deck.Draw.Count + session.Deck.Discard.Count} cards of soul recovered.", 14, FateUi.Bone));
            stage.Add(FateUi.Text($"Table seed: {_run.OriginalSeed}", 13, FateUi.BoneDim));
            stage.Add(FateUi.MakeButton("PLAY AGAIN", ShowHeroSelect, FateUi.GoldLeaf, 17));
        }
    }
}
