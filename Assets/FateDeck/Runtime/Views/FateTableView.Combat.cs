using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.Instances;
using FateDeck.Runtime.Combat;
using FateDeck.Runtime.Core;
using FateDeck.Runtime.Run;
using UnityEngine;
using UnityEngine.UIElements;

namespace FateDeck.Runtime.Views
{
    public sealed partial class FateTableView
    {
        private void BuildCombatScreen()
        {
            FateSession session = Session;
            CombatEngine combat = session.Combat;
            if (combat == null)
            {
                return;
            }

            var screen = FateUi.Column();
            screen.style.flexGrow = 1;

            string phase = combat.Phase == CombatPhase.PlayerTurn ? "your move" : "the table moves";
            Label header = FateUi.Text($"ROUND {combat.Round} — {phase}", 16, FateUi.BoneDim);
            header.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.style.marginTop = 4;
            screen.Add(header);

            if (combat.Mantle.Count > 0)
            {
                Label mantle = FateUi.Text(
                    $"THE MANTLE holds {combat.Mantle.Count} of your cards (+1 Force to its attacks per 3). "
                    + $"A single hit of {session.Rules.MantleSpillDamage:0}+ shakes one loose; the rest return on its death.",
                    13, FateUi.Ember);
                mantle.style.unityTextAlign = TextAnchor.MiddleCenter;
                screen.Add(mantle);
            }

            var enemyRow = new VisualElement();
            enemyRow.style.flexDirection = FlexDirection.Row;
            enemyRow.style.flexWrap = Wrap.Wrap;
            enemyRow.style.justifyContent = Justify.Center;
            enemyRow.style.alignItems = Align.FlexStart;
            enemyRow.style.flexGrow = 1;
            enemyRow.style.marginTop = 10;
            foreach (CardInstance enemy in combat.EnemiesSnapshot())
            {
                enemyRow.Add(BuildEnemyPanel(session, combat, enemy));
            }

            screen.Add(enemyRow);
            _screenHost.Add(screen);
        }

        private VisualElement BuildEnemyPanel(FateSession session, CombatEngine combat, CardInstance enemy)
        {
            var panel = new VisualElement();
            panel.style.width = 216;
            panel.style.backgroundColor = FateUi.Panel;
            panel.style.marginLeft = 8;
            panel.style.marginRight = 8;
            FateUi.Pad(panel, 10);

            bool isActing = session.CurrentAction?.SourceEnemy == enemy;
            bool isTarget = combat.SelectedOrFirstEnemy() == enemy;
            Color border = isActing ? FateUi.Ember : isTarget ? FateUi.GoldLeaf : FateUi.Line;
            FateUi.SetBorder(panel, border, isActing || isTarget ? 2 : 1, 8);

            Label name = FateUi.Text(enemy.DisplayName, 16, FateUi.Bone);
            name.style.unityFontStyleAndWeight = FontStyle.Bold;
            name.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(name);

            if (isTarget && combat.Enemies.Count > 1)
            {
                Label target = FateUi.Text("— your target —", 11, FateUi.GoldLeaf);
                target.style.unityTextAlign = TextAnchor.MiddleCenter;
                panel.Add(target);
            }

            double hp = enemy.Fields.GetNumber(_catalog.HpField);
            double maxHp = enemy.Fields.GetNumber(_catalog.MaxHpField);
            VisualElement bar = FateUi.Bar(hp, maxHp, FateUi.Blood, $"{hp:0} / {maxHp:0} HP");
            bar.style.marginTop = 6;
            panel.Add(bar);

            var chips = new VisualElement();
            chips.style.flexDirection = FlexDirection.Row;
            chips.style.flexWrap = Wrap.Wrap;
            chips.style.marginTop = 6;
            double block = enemy.Fields.GetNumber(_catalog.BlockField);
            if (block > 0)
            {
                chips.Add(FateUi.Chip($"Block {block:0}", FateUi.Verdigris));
            }

            int burn = session.GetStatus(enemy, StatusKind.Burn);
            if (burn > 0)
            {
                chips.Add(FateUi.Chip($"Burn {burn}", FateUi.Ember));
            }

            int weak = session.GetStatus(enemy, StatusKind.Weak);
            if (weak > 0)
            {
                chips.Add(FateUi.Chip($"Weak {weak}", FateUi.Violet));
            }

            double pocketed = enemy.Fields.GetNumber(_catalog.PocketedGoldField);
            if (pocketed > 0)
            {
                chips.Add(FateUi.Chip($"Holds {pocketed:0}g", FateUi.GoldLeaf));
            }

            if (chips.childCount > 0)
            {
                panel.Add(chips);
            }

            EnemyActionSpec intent = combat.IntentOf(enemy);
            if (intent != null)
            {
                string label = intent.Kind == EnemyActionKind.Special
                    ? $"NEXT: {intent.Name}"
                    : $"NEXT: {intent.Name} {combat.EffectiveForceOf(enemy, intent):0.#}"
                        + (intent.FlipsFate ? "  (flips fate)" : string.Empty);
                Label intentLabel = FateUi.Text(label, 14, isActing ? FateUi.Ember : FateUi.GoldLeaf);
                intentLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                intentLabel.style.marginTop = 6;
                panel.Add(intentLabel);
            }

            if (isActing)
            {
                Label acting = FateUi.Text("acting…", 12, FateUi.Ember);
                panel.Add(acting);
            }

            string gimmick = enemy.Definition.GetText(_catalog.GimmickField);
            if (!string.IsNullOrEmpty(gimmick))
            {
                Label gimmickLabel = FateUi.Text(gimmick, 11, FateUi.BoneDim);
                gimmickLabel.style.marginTop = 6;
                panel.Add(gimmickLabel);
            }

            if (session.Phase == FateResolutionPhase.Idle && combat.Enemies.Count > 1)
            {
                FateUi.MakeClickable(panel, () =>
                {
                    combat.SelectedEnemy = enemy;
                    _log.Append($"Target: {enemy.DisplayName}.", FateUi.BoneDim);
                    MarkScreenDirty();
                });
            }

            return panel;
        }

        // ---------------------------------------------------------------- prompt area

        /// <summary>Rebuilds the always-present prompt strip under the stage: what is happening, what you can do.</summary>
        private void BuildPromptArea()
        {
            _promptHost.Clear();
            FateSession session = Session;
            if (session == null || session.IsPlayerDead)
            {
                return;
            }

            switch (session.Phase)
            {
                case FateResolutionPhase.AwaitPreFlip:
                    BuildPreFlipPrompt(session);
                    return;

                case FateResolutionPhase.AwaitBank:
                    BuildBankPrompt(session);
                    return;

                case FateResolutionPhase.AwaitDoubleDrawChoice:
                    BuildDoubleDrawPrompt(session);
                    return;
            }

            // Pending wound picks survive every screen rebuild until spent.
            if (_woundPicksRemaining > 0 && session.Deck.Wound.Count > 0)
            {
                PromptPanel($"Mend {_woundPicksRemaining} wound{(_woundPicksRemaining == 1 ? "" : "s")} — "
                    + "click the highlighted cards in the Wound Row below.", FateUi.Verdigris);
            }

            if (_run.Screen == RunScreen.Combat && session.Combat != null)
            {
                BuildCombatIdlePrompt(session, session.Combat);
            }
        }

        private VisualElement PromptPanel(string text, Color? accent = null)
        {
            VisualElement panel = FateUi.MakePanel();
            panel.style.marginTop = 6;
            FateUi.SetBorder(panel, accent ?? FateUi.Line, 1, 6);
            if (!string.IsNullOrEmpty(text))
            {
                Label label = FateUi.Text(text, 15, FateUi.Bone);
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                panel.Add(label);
            }

            _promptHost.Add(panel);
            return panel;
        }

        private void SetPrompt(string text)
        {
            _promptHost.Clear();
            PromptPanel(text, FateUi.Verdigris);
        }

        private VisualElement PromptButtons(VisualElement panel)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.Center;
            row.style.marginTop = 4;
            panel.Add(row);
            return row;
        }

        private void BuildCombatIdlePrompt(FateSession session, CombatEngine combat)
        {
            if (combat.Phase != CombatPhase.PlayerTurn)
            {
                PromptPanel("The enemies act…", FateUi.Ember);
                return;
            }

            if (combat.MainActionTaken)
            {
                PromptPanel("Main Action spent — the enemy phase begins.", FateUi.Ember);
                return;
            }

            VisualElement panel = PromptPanel(
                "Choose your Main Action. Charms below are free — click one to spend it first.");
            VisualElement row = PromptButtons(panel);
            CardInstance target = combat.SelectedOrFirstEnemy();
            string targetName = target != null ? target.DisplayName : "the enemy";
            row.Add(FateUi.MakeButton($"STRIKE {session.Rules.StrikeBaseForce:0} → {targetName}",
                () => combat.PlayerStrike(combat.SelectedOrFirstEnemy()), FateUi.Ember, 16));
            row.Add(FateUi.MakeButton($"GUARD {session.Rules.GuardBaseForce:0}",
                () => combat.PlayerGuard(), FateUi.Verdigris, 16));
            if (combat.CanFlee)
            {
                row.Add(FateUi.MakeButton($"FLEE (mill {session.Rules.FleeMill})", () =>
                {
                    if (combat.PlayerFlee())
                    {
                        _log.Append("You slip out the door. The room and its rewards are gone.", FateUi.BoneDim);
                        MarkScreenDirty();
                    }
                }, FateUi.BoneDim, 14));
            }
        }

        private void BuildPreFlipPrompt(FateSession session)
        {
            FateAction action = session.CurrentAction;
            bool enemyOwned = action.SourceEnemy != null;
            string owner = enemyOwned ? $"{action.SourceEnemy.DisplayName}'s" : "Your";
            string pocketHint = session.Deck.Pocket.Count > 0
                ? " Play a highlighted Pocket card below to replace the flip entirely."
                : string.Empty;

            VisualElement panel = PromptPanel(
                $"{owner} {action.Name} ({action.Force:0.#}) hangs over the deck…{pocketHint}",
                enemyOwned ? FateUi.Ember : FateUi.GoldLeaf);

            if (!enemyOwned || _preFlipWindowSeconds <= 0f)
            {
                VisualElement row = PromptButtons(panel);
                row.Add(FateUi.MakeButton("FLIP FATE", () => session.ContinueFlip(), FateUi.Ember, 17));
            }
            else
            {
                Label window = FateUi.Text("…the shimmer slows — the window is open…", 13, FateUi.Violet);
                window.style.unityTextAlign = TextAnchor.MiddleCenter;
                panel.Add(window);
            }
        }

        private void BuildBankPrompt(FateSession session)
        {
            CardInstance revealed = session.RevealedCard;
            if (revealed == null)
            {
                return;
            }

            VisualElement panel = PromptPanel(null, FateUi.Verdigris);
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.justifyContent = Justify.Center;
            panel.Add(row);

            row.Add(CardElementBuilder.ScaledCard(_catalog.FateCardLayout, revealed, 0.22f));

            var side = FateUi.Column(4);
            side.style.marginLeft = 14;
            side.style.maxWidth = 420;
            Label title = FateUi.Text($"{revealed.DisplayName} surfaces on your {session.CurrentAction?.Name}.",
                16, FateUi.Bone);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            side.Add(title);
            side.Add(FateUi.Text(
                "SLEEVE IT: bank the card in your Pocket; the action resolves at base value.\n"
                + "LET IT RIDE: apply its law now.", 13, FateUi.BoneDim));
            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.Add(FateUi.MakeButton("SLEEVE IT", () => session.BankRevealed(), FateUi.Verdigris, 16));
            buttons.Add(FateUi.MakeButton("LET IT RIDE", () => session.DeclineBank(), FateUi.Ember, 16));
            side.Add(buttons);
            row.Add(side);
        }

        private void BuildDoubleDrawPrompt(FateSession session)
        {
            VisualElement panel = PromptPanel(
                "Two fates surface — click the one whose law applies. The other is discarded.", FateUi.Violet);
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.Center;
            row.style.marginTop = 6;
            panel.Add(row);

            row.Add(ChoiceCard(session.RevealedCard, () => session.ChooseRevealed(false)));
            row.Add(ChoiceCard(session.AlternateCard, () => session.ChooseRevealed(true)));
        }

        private VisualElement ChoiceCard(CardInstance card, System.Action pick)
        {
            VisualElement face = CardElementBuilder.ScaledCard(_catalog.FateCardLayout, card, 0.26f);
            face.style.marginLeft = 10;
            face.style.marginRight = 10;
            FateUi.MakeClickable(face, pick);
            return face;
        }

        // ---------------------------------------------------------------- odds column

        /// <summary>The Odds Panel: exact fractions for the actions currently on the table.</summary>
        private VisualElement BuildOddsColumn(FateSession session)
        {
            if (_run.Screen == RunScreen.Chest && !_run.ChestOpened
                && _run.CurrentRoom is ChestRoomDefinition chest
                && session.Phase == FateResolutionPhase.Idle)
            {
                double baseGold = chest.Locked ? session.Rules.LockedChestBaseGold : session.Rules.ChestBaseGold;
                return OddsPanel($"CHEST — base {baseGold:0}g",
                    OddsCalculator.Table(_catalog, session.Deck, LawContext.Loot, baseGold), "g");
            }

            if (_run.Screen != RunScreen.Combat || session.Combat == null
                || session.Combat.Phase != CombatPhase.PlayerTurn || session.Combat.MainActionTaken
                || session.Phase != FateResolutionPhase.Idle)
            {
                return null;
            }

            var holder = FateUi.Column(8);
            holder.Add(OddsPanel($"YOUR STRIKE — base {session.Rules.StrikeBaseForce:0}",
                OddsCalculator.Table(_catalog, session.Deck, LawContext.PlayerOffense,
                    session.Rules.StrikeBaseForce), " dmg"));

            CardInstance target = session.Combat.SelectedOrFirstEnemy();
            EnemyActionSpec intent = target != null ? session.Combat.IntentOf(target) : null;
            if (intent != null && intent.FlipsFate && intent.Kind != EnemyActionKind.Special)
            {
                double force = session.Combat.EffectiveForceOf(target, intent);
                VisualElement incoming = OddsPanel($"INCOMING — {target.DisplayName} {intent.Name} {force:0.#}",
                    OddsCalculator.Table(_catalog, session.Deck, LawContext.EnemyAction, force), " dmg");
                incoming.style.marginTop = 8;
                holder.Add(incoming);
            }

            return holder;
        }

        private VisualElement OddsPanel(string title, List<OddsRow> rows, string unit)
        {
            VisualElement panel = FateUi.MakePanel(title);
            foreach (OddsRow row in rows)
            {
                var line = new VisualElement();
                line.style.flexDirection = FlexDirection.Row;
                line.style.marginBottom = 2;

                Color color = ForceColor(row.Force);
                Label name = FateUi.Text(row.Force.name, 13, color);
                name.style.width = 76;
                name.style.unityFontStyleAndWeight = FontStyle.Bold;
                name.style.whiteSpace = WhiteSpace.NoWrap;
                line.Add(name);

                Label odds = FateUi.Text(row.FractionLabel, 13, FateUi.Bone);
                odds.style.width = 86;
                odds.style.whiteSpace = WhiteSpace.NoWrap;
                line.Add(odds);

                string note = string.IsNullOrEmpty(row.Note) ? string.Empty : $"  {row.Note}";
                Label result = FateUi.Text($"→ {row.ResultForce:0.#}{unit}{note}", 13, FateUi.BoneDim);
                result.style.whiteSpace = WhiteSpace.Normal;
                result.style.flexShrink = 1;
                line.Add(result);

                panel.Add(line);
            }

            if (rows.Count == 0)
            {
                panel.Add(FateUi.Text("deck empty — a reshuffle (and its tax) comes first", 12, FateUi.BoneDim));
            }

            return panel;
        }
    }
}
