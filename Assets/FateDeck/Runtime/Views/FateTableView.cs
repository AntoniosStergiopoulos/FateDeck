using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Core;
using FateDeck.Runtime.Run;
using UnityEngine;
using UnityEngine.UIElements;

namespace FateDeck.Runtime.Views
{
    /// <summary>
    /// The single screen of Fate Deck, rendered entirely with runtime UI Toolkit:
    /// top status bar, left odds column, center stage, right event log, bottom deck tableau.
    /// The deck is the HUD; the log tells the run back as a story.
    /// </summary>
    [AddComponentMenu("Fate Deck/Fate Table")]
    [RequireComponent(typeof(UIDocument))]
    public sealed partial class FateTableView : MonoBehaviour
    {
        [SerializeField] private FateContentCatalog _catalog;
        [SerializeField] private int _seed;

        [Tooltip("Seconds the enemy pre-flip window stays open for pocket plays. 0 or less waits for a click.")]
        [SerializeField] private float _preFlipWindowSeconds = 2.2f;

        [Tooltip("Delay between automatic combat beats (enemy actions, phase hand-offs).")]
        [SerializeField] private float _beatSeconds = 0.55f;

        private RunController _run;
        private VisualElement _root;
        private VisualElement _screenHost;
        private VisualElement _promptHost;
        private VisualElement _overlayHost;
        private VisualElement _bannerHost;
        private VisualElement _fxHost;
        private VisualElement _tableauBar;
        private VisualElement _leftColumn;
        private VisualElement _rightColumn;
        private Label _runInfoLabel;
        private VisualElement _statusChips;
        private Label _barkLabel;
        private GameLogPanel _log;
        private float _barkTimer;
        private float _bannerTimer;
        private float _beatTimer;
        private float _windowTimer;
        private bool _screenDirty;

        public FateContentCatalog Catalog
        {
            get => _catalog;
            set => _catalog = value;
        }

        private FateSession Session => _run?.Session;

        private bool IsOverlayOpen => _overlayHost != null && _overlayHost.childCount > 0;

        private void Start()
        {
            if (_catalog == null)
            {
                Debug.LogError("[FateDeck] Fate Table has no content catalog. Run Tools/Fate Deck/Create Game Content first.");
                enabled = false;
                return;
            }

            var document = GetComponent<UIDocument>();
            if (document.panelSettings == null)
            {
                document.panelSettings = _catalog.Panel;
            }

            _root = document.rootVisualElement;
            _run = new RunController(_catalog);
            _run.Changed += OnRunChanged;
            _run.SessionStarted = _ => HookSessionEvents();
            BuildChrome();
            ShowBootMenu();
        }

        private void OnDestroy()
        {
            UiFx.Clear();
            if (_run != null)
            {
                _run.Changed -= OnRunChanged;
                _run.Session?.Dispose();
            }
        }

        // ---------------------------------------------------------------- chrome

        private void BuildChrome()
        {
            _root.Clear();
            _root.style.flexGrow = 1;
            _root.style.backgroundColor = FateUi.Ink;
            _root.style.flexDirection = FlexDirection.Column;

            _root.Add(BuildTopBar());

            var main = new VisualElement();
            main.style.flexDirection = FlexDirection.Row;
            main.style.flexGrow = 1;
            FateUi.Pad(main, 8);
            _root.Add(main);

            _leftColumn = new VisualElement();
            _leftColumn.style.width = 268;
            _leftColumn.style.marginRight = 8;
            main.Add(_leftColumn);

            var center = new VisualElement();
            center.style.flexGrow = 1;
            center.style.flexDirection = FlexDirection.Column;
            main.Add(center);

            _screenHost = new VisualElement();
            _screenHost.style.flexGrow = 1;
            center.Add(_screenHost);

            _promptHost = new VisualElement();
            _promptHost.style.minHeight = 6;
            center.Add(_promptHost);

            _rightColumn = new VisualElement();
            _rightColumn.style.width = 330;
            _rightColumn.style.marginLeft = 8;
            main.Add(_rightColumn);

            _log = new GameLogPanel();
            _rightColumn.Add(_log.Root);

            _tableauBar = new VisualElement();
            _tableauBar.style.flexDirection = FlexDirection.Row;
            _tableauBar.style.alignItems = Align.FlexStart;
            _tableauBar.style.backgroundColor = FateUi.Panel;
            _tableauBar.style.borderTopWidth = 1;
            _tableauBar.style.borderTopColor = FateUi.Line;
            _tableauBar.style.minHeight = 132;
            FateUi.Pad(_tableauBar, 8);
            _root.Add(_tableauBar);

            _bannerHost = new VisualElement();
            _bannerHost.style.position = Position.Absolute;
            _bannerHost.style.left = 0;
            _bannerHost.style.right = 0;
            _bannerHost.style.top = 64;
            _bannerHost.style.alignItems = Align.Center;
            _bannerHost.pickingMode = PickingMode.Ignore;
            _root.Add(_bannerHost);

            _fxHost = new VisualElement();
            _fxHost.style.position = Position.Absolute;
            _fxHost.style.left = 0;
            _fxHost.style.right = 0;
            _fxHost.style.top = 0;
            _fxHost.style.bottom = 0;
            _fxHost.pickingMode = PickingMode.Ignore;
            _root.Add(_fxHost);

            _overlayHost = new VisualElement();
            _overlayHost.style.position = Position.Absolute;
            _overlayHost.style.left = 0;
            _overlayHost.style.right = 0;
            _overlayHost.style.top = 0;
            _overlayHost.style.bottom = 0;
            // An empty full-screen element with default picking would swallow every click on
            // the table. The host stays click-transparent; the dimmer a real overlay adds is
            // what blocks the background (children pick independently of their parent).
            _overlayHost.pickingMode = PickingMode.Ignore;
            _root.Add(_overlayHost);
        }

        private VisualElement BuildTopBar()
        {
            var bar = new VisualElement();
            bar.style.flexDirection = FlexDirection.Row;
            bar.style.alignItems = Align.Center;
            bar.style.backgroundColor = FateUi.Panel;
            bar.style.borderBottomWidth = 1;
            bar.style.borderBottomColor = FateUi.Line;
            bar.style.height = 40;
            bar.style.paddingLeft = 12;
            bar.style.paddingRight = 12;

            Label logo = FateUi.Text("FATE DECK", 17, FateUi.GoldLeaf);
            logo.style.unityFontStyleAndWeight = FontStyle.Bold;
            logo.style.marginRight = 16;
            bar.Add(logo);

            _runInfoLabel = FateUi.Text(string.Empty, 14, FateUi.Bone);
            _runInfoLabel.style.whiteSpace = WhiteSpace.NoWrap;
            bar.Add(_runInfoLabel);

            _statusChips = new VisualElement();
            _statusChips.style.flexDirection = FlexDirection.Row;
            _statusChips.style.alignItems = Align.Center;
            _statusChips.style.marginLeft = 10;
            bar.Add(_statusChips);

            bar.Add(FateUi.Spacer());

            _barkLabel = FateUi.Text(string.Empty, 13, FateUi.BoneDim);
            _barkLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            _barkLabel.style.whiteSpace = WhiteSpace.NoWrap;
            bar.Add(_barkLabel);

            Button help = FateUi.MakeButton("? RULES", ShowHelpOverlay, FateUi.BoneDim, 12);
            help.style.marginLeft = 10;
            bar.Add(help);
            return bar;
        }

        // ---------------------------------------------------------------- boot

        private void ShowBootMenu()
        {
            _screenHost.Clear();
            _promptHost.Clear();
            var menu = FateUi.Column();
            menu.style.flexGrow = 1;
            menu.style.alignItems = Align.Center;
            menu.style.justifyContent = Justify.Center;

            menu.Add(FateUi.Heading("F A T E   D E C K", 44, FateUi.Bone));
            Label tagline = FateUi.Text("Your deck is your health, your luck, and your build.", 16, FateUi.BoneDim);
            tagline.style.marginBottom = 22;
            menu.Add(tagline);

            if (FateRunSave.Exists)
            {
                menu.Add(FateUi.MakeButton("CONTINUE THE CLIMB", () =>
                {
                    if (!_run.TryContinueRun())
                    {
                        ShowHeroSelect();
                    }
                }, FateUi.Verdigris, 17));
                menu.Add(FateUi.MakeButton("NEW RUN", ShowHeroSelect, FateUi.GoldLeaf, 17));
            }
            else
            {
                menu.Add(FateUi.MakeButton("SIT DOWN AT THE TABLE", ShowHeroSelect, FateUi.GoldLeaf, 17));
            }

            _screenHost.Add(menu);
            UiFx.FadeSlideIn(menu, -20f, 0.4f);
        }

        private void ShowHeroSelect()
        {
            if (_catalog.Heroes.Count <= 1)
            {
                StartNewRun(_catalog.Heroes.Count > 0 ? _catalog.Heroes[0] : null);
                return;
            }

            BuildHeroSelectScreen();
        }

        private void StartNewRun(AStergio.OmniCard.Runtime.Cards.Data.CardDefinition hero)
        {
            FateRunSave.Delete();
            _run.StartNewRun(hero, _seed);
        }

        // ---------------------------------------------------------------- session wiring

        private void HookSessionEvents()
        {
            FateSession session = Session;
            if (session == null)
            {
                return;
            }

            _log.Clear();
            _log.Divider("a new hand is dealt");

            session.Events.Subscribe<DealerBarkEvent>(OnBark);
            session.Events.Subscribe<FateFlipEvent>(OnFlip);
            session.Events.Subscribe<ActionResolvedEvent>(OnActionResolvedView);
            session.Events.Subscribe<ResolutionPhaseChangedEvent>(_ => MarkScreenDirty());
            session.Events.Subscribe<GoldChangedEvent>(OnGoldChanged);
            session.Events.Subscribe<ReshuffleEvent>(OnReshuffle);
            session.Events.Subscribe<CardMilledEvent>(OnMilled);
            session.Events.Subscribe<PocketBankedEvent>(OnPocketBanked);
            session.Events.Subscribe<PocketPlayedEvent>(OnPocketPlayed);
            session.Events.Subscribe<WoundHealedEvent>(OnWoundHealed);
            session.Events.Subscribe<CardExiledEvent>(OnExiled);
            session.Events.Subscribe<StatusChangedEvent>(OnStatusChanged);
            session.Events.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            session.Events.Subscribe<PlayerHitEvent>(OnPlayerHit);
            session.Events.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            session.Events.Subscribe<CombatStartedEvent>(OnCombatStarted);
            session.Events.Subscribe<CombatEndedEvent>(OnCombatEndedView);
            session.Events.Subscribe<PlayerTurnStartedEvent>(OnPlayerTurnStarted);
            session.Events.Subscribe<PlayerDiedEvent>(OnPlayerDiedView);
            session.Events.Subscribe<ScryEvent>(OnScry);
            session.Events.Subscribe<MantleTakenEvent>(OnMantleTaken);
            session.Events.Subscribe<MantleSpilledEvent>(OnMantleSpilled);
            session.Events.Subscribe<KeysChangedEvent>(OnKeysChanged);
            session.ChoiceHandler = OnZoneChoice;

            UiFx.Clear();
            _fxHost.Clear();
            _overlayHost.Clear();
            RefreshAll();
        }

        // ---------------------------------------------------------------- frame pump

        private void Update()
        {
            UiFx.Update(Time.deltaTime);
            if (Session == null)
            {
                return;
            }

            if (_barkTimer > 0f)
            {
                _barkTimer -= Time.deltaTime;
                if (_barkTimer <= 0f)
                {
                    _barkLabel.text = string.Empty;
                }
            }

            if (_bannerTimer > 0f)
            {
                _bannerTimer -= Time.deltaTime;
                if (_bannerTimer <= 0f)
                {
                    _bannerHost.Clear();
                }
            }

            if (_screenDirty)
            {
                _screenDirty = false;
                RebuildScreen();
            }

            PumpCombat();
        }

        private void PumpCombat()
        {
            FateSession session = Session;
            if (session.IsPlayerDead || IsOverlayOpen)
            {
                return;
            }

            if (session.Phase == FateResolutionPhase.AwaitPreFlip
                && session.CurrentAction?.SourceEnemy != null)
            {
                if (_preFlipWindowSeconds > 0f)
                {
                    _windowTimer += Time.deltaTime;
                    if (_windowTimer >= _preFlipWindowSeconds)
                    {
                        _windowTimer = 0f;
                        session.ContinueFlip();
                    }
                }

                return;
            }

            _windowTimer = 0f;
            if (_run.Screen != RunScreen.Combat || session.Combat == null
                || session.Phase != FateResolutionPhase.Idle)
            {
                return;
            }

            _beatTimer += Time.deltaTime;
            if (_beatTimer < _beatSeconds)
            {
                return;
            }

            _beatTimer = 0f;
            if (session.Combat.TryAdvance())
            {
                MarkScreenDirty();
            }
        }

        private void OnRunChanged()
        {
            MarkScreenDirty();
            RefreshHud();
        }

        private void MarkScreenDirty()
        {
            _screenDirty = true;
        }

        // ---------------------------------------------------------------- shared refresh

        private void RefreshAll()
        {
            RefreshHud();
            RefreshTableau();
            MarkScreenDirty();
        }

        private void RefreshHud()
        {
            FateSession session = Session;
            if (session == null || _runInfoLabel == null)
            {
                return;
            }

            _runInfoLabel.text = $"Biome {_run.Biome} · Step {Mathf.Max(1, _run.Step)}/{session.Rules.TrackSteps}";

            _statusChips.Clear();
            _statusChips.Add(FateUi.Chip($"Gold {session.Gold}g", FateUi.GoldLeaf, 13));
            if (session.Keys > 0)
            {
                _statusChips.Add(FateUi.Chip($"Keys {session.Keys}", FateUi.Verdigris, 13));
            }

            if (session.PlayerBlock > 0)
            {
                _statusChips.Add(FateUi.Chip($"Block {session.PlayerBlock:0}", FateUi.Verdigris, 13));
            }

            if (session.PlayerRetaliateBurn > 0)
            {
                _statusChips.Add(FateUi.Chip($"Retaliate {session.PlayerRetaliateBurn}", FateUi.Ember, 13));
            }

            if (session.PlayerBurn > 0)
            {
                _statusChips.Add(FateUi.Chip($"Burn {session.PlayerBurn}", FateUi.Ember, 13));
            }

            if (session.PlayerWeak > 0)
            {
                _statusChips.Add(FateUi.Chip($"Weak {session.PlayerWeak}", FateUi.Violet, 13));
            }
        }

        private Color ForceColor(MetadataEntry force)
        {
            return CardElementBuilder.ForceColor(_catalog, force);
        }

        // ---------------------------------------------------------------- event log + feedback

        private void OnBark(DealerBarkEvent bark)
        {
            _barkLabel.text = bark.Line;
            _barkTimer = 7f;
            _log.Append(bark.Line, FateUi.BoneDim);
        }

        private void OnFlip(FateFlipEvent flip)
        {
            MetadataEntry force = flip.Force;
            Color color = ForceColor(force);
            string forceName = force != null ? force.name.ToUpperInvariant() : flip.Card.DisplayName;
            ShowBanner(flip.FromPocket ? $"{forceName} — from the pocket" : forceName, color);

            string owner = OwnerLabel(flip.Action);
            string source = flip.FromPocket ? "is played from the pocket onto" : "surfaces on";
            _log.Append($"{forceName} {source} {owner} {flip.Action.Name}.", color, bold: true);

            if (force == _catalog.Doom && Session.DoomFlipsThisRun == 1)
            {
                Session.Bark("\"That one's on the house.\"");
            }

            RefreshTableau();
        }

        private static string OwnerLabel(FateAction action)
        {
            return action.SourceEnemy != null ? $"{action.SourceEnemy.DisplayName}'s" : "your";
        }

        private void OnActionResolvedView(ActionResolvedEvent resolved)
        {
            FateAction action = resolved.Action;
            switch (action.Kind)
            {
                case FateActionKind.Strike:
                    if (action.Negated)
                    {
                        _log.Append("Your Strike is negated — a total whiff.", FateUi.BoneDim);
                    }

                    break;

                case FateActionKind.Guard:
                    _log.Append(action.Negated
                        ? "Your Guard is negated — no Block gained."
                        : $"Your Guard settles at {System.Math.Max(0, action.Force):0.#} Block.", FateUi.Verdigris);
                    break;

                case FateActionKind.EnemyAttack when action.Negated:
                    _log.Append($"{action.SourceEnemy?.DisplayName}'s attack is negated entirely.", FateUi.Verdigris);
                    break;

                case FateActionKind.EnemyBrace when !action.Negated:
                    _log.Append($"{action.SourceEnemy?.DisplayName} braces for {System.Math.Max(0, action.Force):0.#}.",
                        FateUi.BoneDim);
                    break;

                case FateActionKind.Loot:
                    LogLootOutcome(action);
                    break;
            }

            RefreshTableau();
        }

        private void LogLootOutcome(FateAction action)
        {
            if (action.Negated)
            {
                _log.Append("The chest crumbles to dust. Nothing.", FateUi.BoneDim);
            }
            else if (action.NoLoot)
            {
                _log.Append("The chest was trapped!", FateUi.Ember, bold: true);
            }
            else if (action.LockedChest && !action.KeyUsed && !action.OpensLock)
            {
                _log.Append("The lock holds. The card is spent; the chest keeps its gold.", FateUi.BoneDim);
            }
            else
            {
                _log.Append($"The chest pays out {System.Math.Max(0, action.Force):0}g.", FateUi.GoldLeaf, bold: true);
            }
        }

        private void OnGoldChanged(GoldChangedEvent gold)
        {
            int delta = gold.NewValue - gold.OldValue;
            _log.Append(delta >= 0 ? $"+{delta}g (now {gold.NewValue}g)." : $"{delta}g (now {gold.NewValue}g).",
                FateUi.GoldLeaf);
            RefreshHud();
            UiFx.Pulse(_statusChips, 1.1f, 0.28f);
            if (delta > 0)
            {
                SpawnFloater($"+{delta}g", FateUi.GoldLeaf, 0f, 12, 20);
            }
        }

        private void OnReshuffle(ReshuffleEvent reshuffle)
        {
            string teeth = reshuffle.TaxAdded == 1 ? "tooth" : "teeth";
            _log.Append($"RESHUFFLE #{reshuffle.ReshuffleCount} — the discard returns and the House adds "
                + $"{reshuffle.TaxAdded} Doom ({reshuffle.TaxAdded} {teeth}).", FateUi.Violet, bold: true);
            Session.Bark($"\"Shuffling up. The rake is {reshuffle.TaxAdded} {teeth}.\"");
            RefreshTableau();
        }

        private void OnMilled(CardMilledEvent milled)
        {
            string name = milled.Card.DisplayName;
            if (milled.Exiled)
            {
                _log.Append($"Milled {name} burns away to Exile — Doom laundered, gone forever.",
                    FateUi.Violet, bold: true);
            }
            else
            {
                _log.Append($"Milled: {name} is torn into the Wound Row.", FateUi.Blood);
            }

            RefreshTableau();
        }

        private void OnPocketBanked(PocketBankedEvent banked)
        {
            _log.Append($"You sleeve {banked.Card.DisplayName} into the Pocket — the action resolves at base value.",
                FateUi.Verdigris, bold: true);
            RefreshTableau();
        }

        private void OnPocketPlayed(PocketPlayedEvent played)
        {
            _log.Append($"Pocket play! {played.Card.DisplayName} replaces the flip on {OwnerLabel(played.Action)} "
                + $"{played.Action.Name}. No card leaves the deck.", FateUi.Verdigris, bold: true);
            RefreshTableau();
        }

        private void OnWoundHealed(WoundHealedEvent healed)
        {
            _log.Append($"{healed.Card.DisplayName} is stitched back into the draw pile.", FateUi.Verdigris);
            RefreshTableau();
        }

        private void OnExiled(CardExiledEvent exiled)
        {
            _log.Append($"{exiled.Card.DisplayName} is exiled from the run.", FateUi.Violet);
            RefreshTableau();
        }

        private void OnStatusChanged(StatusChangedEvent status)
        {
            string target = status.Enemy != null ? status.Enemy.DisplayName : "You";
            if (status.Stacks > 0)
            {
                Color color = status.Status == StatusKind.Burn ? FateUi.Ember : FateUi.Violet;
                _log.Append($"{target}: {status.Status} {status.Stacks}.", color);
            }

            RefreshHud();
            MarkScreenDirty();
        }

        private void OnEnemyDamaged(EnemyDamagedEvent damaged)
        {
            string blocked = damaged.Absorbed > 0 ? $" ({damaged.Absorbed:0.#} blocked)" : string.Empty;
            if (damaged.Dealt > 0)
            {
                _log.Append($"{damaged.Enemy.DisplayName} takes {damaged.Dealt:0.#} damage{blocked} — "
                    + $"{damaged.RemainingHp:0} HP left.", FateUi.Ember);
                SpawnFloater($"-{damaged.Dealt:0.#}", FateUi.Ember, EnemyFloaterOffset(damaged.Enemy), 26);
            }
            else if (damaged.Absorbed > 0)
            {
                _log.Append($"{damaged.Enemy.DisplayName}'s Block absorbs everything{blocked}.", FateUi.BoneDim);
                SpawnFloater("BLOCKED", FateUi.Verdigris, EnemyFloaterOffset(damaged.Enemy), 30);
            }

            MarkScreenDirty();
        }

        private void OnPlayerHit(PlayerHitEvent hit)
        {
            string attacker = hit.Attacker != null ? hit.Attacker.DisplayName : "The enemy";
            string blocked = hit.Absorbed > 0 ? $"Block absorbs {hit.Absorbed:0.#}; " : string.Empty;
            _log.Append(hit.Milled > 0
                    ? $"{attacker} hits for {hit.Incoming:0.#} — {blocked}you mill {hit.Milled}."
                    : $"{attacker} hits for {hit.Incoming:0.#} — {blocked}nothing gets through.",
                hit.Milled > 0 ? FateUi.Blood : FateUi.Verdigris, bold: hit.Milled > 0);

            if (hit.Milled > 0)
            {
                SpawnFloater($"-{hit.Milled} card{(hit.Milled == 1 ? "" : "s")}", FateUi.Blood, 0f, 62, 24);
                UiFx.Shake(_tableauBar, 8f, 0.45f);
            }
            else
            {
                SpawnFloater("HELD", FateUi.Verdigris, 0f, 62, 20);
            }
        }

        private void OnMantleTaken(MantleTakenEvent taken)
        {
            _log.Append($"CONFISCATED — {taken.Count}x {taken.Force.name} vanish into the Mantle. "
                + "Heavy hits (6+) shake cards loose; the rest return when it dies.",
                FateUi.Violet, bold: true);
            ShowBanner($"CONFISCATED: {taken.Count}x {taken.Force.name.ToUpperInvariant()}", FateUi.Violet);
            RefreshTableau();
        }

        private void OnMantleSpilled(MantleSpilledEvent spilled)
        {
            _log.Append($"A heavy blow shakes {spilled.Card.DisplayName} loose from the Mantle — "
                + "it returns to your discard.", FateUi.Verdigris, bold: true);
            SpawnFloater($"+{spilled.Card.DisplayName} freed", FateUi.Verdigris, 0f, 34, 20);
            RefreshTableau();
        }

        private void OnKeysChanged(KeysChangedEvent keys)
        {
            int delta = keys.NewValue - keys.OldValue;
            _log.Append(delta > 0
                    ? $"+{delta} Key{(delta == 1 ? "" : "s")} (now {keys.NewValue})."
                    : $"A Key turns. {keys.NewValue} left.",
                FateUi.Verdigris);
            RefreshHud();
        }

        /// <summary>Horizontal floater offset for an enemy, matching the centered enemy row.</summary>
        private float EnemyFloaterOffset(CardInstance enemy)
        {
            var combat = Session?.Combat;
            if (combat == null)
            {
                return 0f;
            }

            var cards = combat.Enemies.Cards;
            int index = 0;
            int count = cards.Count > 0 ? cards.Count : 1;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] == enemy)
                {
                    index = i;
                    break;
                }
            }

            return (index - (count - 1) * 0.5f) * 232f;
        }

        /// <summary>Spawns a floating feedback label at a horizontal offset from screen center.</summary>
        private void SpawnFloater(string text, Color color, float offsetX, float topPercent, float size = 26)
        {
            Label floater = FateUi.Text(text, size, color);
            floater.style.unityFontStyleAndWeight = FontStyle.Bold;
            floater.style.position = Position.Absolute;
            floater.style.left = 0;
            floater.style.right = 0;
            floater.style.top = Length.Percent(topPercent);
            floater.style.unityTextAlign = TextAnchor.MiddleCenter;
            floater.pickingMode = PickingMode.Ignore;
            _fxHost.Add(floater);
            UiFx.FloatAway(floater, offsetX, -48f, 0.95f);
        }

        private void OnEnemyDied(EnemyDiedEvent died)
        {
            _log.Append($"{died.Enemy.DisplayName} is settled. Bounty: {died.Bounty}g.", FateUi.GoldLeaf, bold: true);
            Session.Bark($"\"{died.Enemy.DisplayName}, settled.\"");
            MarkScreenDirty();
        }

        private void OnCombatStarted(CombatStartedEvent _)
        {
            var combat = Session.Combat;
            if (combat == null)
            {
                return;
            }

            var names = new System.Text.StringBuilder();
            foreach (CardInstance enemy in combat.Enemies.Cards)
            {
                if (names.Length > 0)
                {
                    names.Append(", ");
                }

                names.Append(enemy.DisplayName);
            }

            _log.Divider($"combat — {names}");
            foreach (CardInstance enemy in combat.Enemies.Cards)
            {
                string gimmick = enemy.Definition.GetText(_catalog.GimmickField);
                if (!string.IsNullOrEmpty(gimmick))
                {
                    _log.Append($"{enemy.DisplayName} — {gimmick}", FateUi.BoneDim);
                }
            }
        }

        private void OnCombatEndedView(CombatEndedEvent ended)
        {
            _log.Divider(ended.Victory ? "victory" : "combat over");
        }

        private void OnPlayerTurnStarted(PlayerTurnStartedEvent turn)
        {
            if (turn.Round > 1)
            {
                _log.Append($"— Round {turn.Round}: your move. Block and retaliation reset. —", FateUi.BoneDim);
            }

            MarkScreenDirty();
        }

        private void OnPlayerDiedView(PlayerDiedEvent _)
        {
            _log.Divider("the last card is spent");
        }

        private void ShowBanner(string text, Color color)
        {
            _bannerHost.Clear();
            var banner = new VisualElement();
            banner.pickingMode = PickingMode.Ignore;
            banner.style.backgroundColor = new Color(0f, 0f, 0f, 0.72f);
            FateUi.SetBorder(banner, color, 1, 8);
            banner.style.paddingLeft = 26;
            banner.style.paddingRight = 26;
            banner.style.paddingTop = 8;
            banner.style.paddingBottom = 8;

            Label label = FateUi.Text(text, 30, color);
            label.pickingMode = PickingMode.Ignore;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            banner.Add(label);
            _bannerHost.Add(banner);
            UiFx.FadeSlideIn(banner, -18f, 0.22f);
            UiFx.Pop(label, 0.8f, 0.24f);
            _bannerTimer = 1.05f;
        }
    }
}
