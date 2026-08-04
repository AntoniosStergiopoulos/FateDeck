using System;
using System.Collections.Generic;
using System.IO;
using AStergio.OmniCard.Runtime.Cards.Data;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.Fields.Values;
using AStergio.OmniCard.Runtime.Cards.Game.Decks;
using AStergio.OmniCard.Runtime.Cards.Game.Zones;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using AStergio.OmniCard.Runtime.Cards.Triggers;
using FateDeck.Runtime.Combat;
using FateDeck.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace FateDeck.Editor
{
    /// <summary>
    /// Generates every Fate Deck asset - fields, forces with their per-context laws, fate cards,
    /// Biome 1 enemies, items, rooms, layouts and the content catalog - as OmniCard data.
    /// Idempotent: re-running never overwrites edits to existing assets (the SpireClimb pattern).
    /// </summary>
    public static partial class FateDeckContentGenerator
    {
        private const string Root = "Assets/FateDeck/Generated";

        [MenuItem("Tools/Fate Deck/Create Game Content", false, 0)]
        public static void Create()
        {
            FateContentCatalog catalog = CreateAssets();
            Selection.activeObject = catalog;
            Debug.Log("[FateDeck] Content ready. Next: Tools/Fate Deck/Create Game Scene, then press Play.");
        }

        [MenuItem("Tools/Fate Deck/Rebuild Content From Scratch", false, 20)]
        public static void RebuildFresh()
        {
            if (!EditorUtility.DisplayDialog("Rebuild Fate Deck content",
                    "Deletes Assets/FateDeck/Generated (including any manual edits to generated assets) "
                    + "and regenerates everything - heroes, forces, enemies, rooms, items. "
                    + "The run save is deleted too. Continue?", "Rebuild", "Cancel"))
            {
                return;
            }

            FateDeck.Runtime.Run.FateRunSave.Delete();
            AssetDatabase.DeleteAsset(Root);
            AssetDatabase.Refresh();
            FateContentCatalog catalog = CreateAssets();
            Selection.activeObject = catalog;

            bool relinked = FateDeckSceneBuilder.RelinkExistingTable(catalog, out _);
            Debug.Log(relinked
                ? "[FateDeck] Content rebuilt and the scene's Fate Table was relinked. Press Play."
                : "[FateDeck] Content rebuilt. Run Tools/Fate Deck/Create Game Scene next.");
        }

        [MenuItem("Tools/Fate Deck/Delete Run Save", false, 40)]
        public static void DeleteSave()
        {
            FateDeck.Runtime.Run.FateRunSave.Delete();
            Debug.Log("[FateDeck] Run save deleted.");
        }

        public static FateContentCatalog CreateAssets()
        {
            FateRulesDefinition rules = GetOrCreate<FateRulesDefinition>("Fate Rules", null);
            Fields fields = CreateFields();
            Forces forces = CreateForces(fields);
            List<CardDefinition> fateCards = CreateFateCards(fields, forces);
            Zones zones = CreateZones();
            Layouts layouts = CreateLayouts(fields);
            Items items = CreateItems(fields, forces);
            List<CardDefinition> heroes = CreateHeroes(fields, forces, fateCards);
            Rooms rooms = CreateRooms(fields, forces, fateCards, items);

            FateContentCatalog catalog = GetOrCreate<FateContentCatalog>("Fate Content Catalog", null);
            FillCatalog(catalog, rules, fields, forces, fateCards, zones, layouts, items, heroes, rooms);
            catalog.Panel = CreatePanelSettings();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        /// <summary>
        /// The runtime UI Toolkit panel: a default-theme .tss (required for runtime panels)
        /// plus a PanelSettings asset that scales with screen size around 1920x1080.
        /// </summary>
        private static UnityEngine.UIElements.PanelSettings CreatePanelSettings()
        {
            string themePath = $"{Root}/UnityDefaultRuntimeTheme.tss";
            var theme = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.ThemeStyleSheet>(themePath);
            if (theme == null)
            {
                EnsureFolder(Root);
                File.WriteAllText(themePath, "@import url(\"unity-theme://default\");\n");
                AssetDatabase.ImportAsset(themePath);
                theme = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.ThemeStyleSheet>(themePath);
            }

            return GetOrCreate<UnityEngine.UIElements.PanelSettings>("Fate Panel Settings", panel =>
            {
                panel.themeStyleSheet = theme;
                panel.scaleMode = UnityEngine.UIElements.PanelScaleMode.ScaleWithScreenSize;
                panel.referenceResolution = new Vector2Int(1920, 1080);
                panel.match = 0.5f;
            });
        }

        // ---------------------------------------------------------------- primitives

        private static T GetOrCreate<T>(string name, Action<T> initialize) where T : ScriptableObject
        {
            return GetOrCreate(name, initialize, null);
        }

        private static T GetOrCreate<T>(string name, Action<T> initialize, string subfolder) where T : ScriptableObject
        {
            string folder = string.IsNullOrEmpty(subfolder) ? Root : $"{Root}/{subfolder}";
            string path = $"{folder}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            EnsureFolder(folder);
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            initialize?.Invoke(asset);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        private static CardFieldDefinition Field(string name, CardFieldKind kind)
        {
            return GetOrCreate<CardFieldDefinition>(name, field => field.Kind = kind, "Fields");
        }

        private static CardDefinition Card(CardSchema schema, string name, Action<CardDefinition> initialize,
            string subfolder = "Cards")
        {
            CardDefinition card = GetOrCreate<CardDefinition>(name, created =>
            {
                created.Schema = schema;
                created.SyncValuesWithSchema();
                initialize?.Invoke(created);
            }, subfolder);

            card.SyncValuesWithSchema();
            EditorUtility.SetDirty(card);
            return card;
        }

        private static void SetNumber(ICardFieldOwner owner, CardFieldDefinition field, double value)
        {
            ((NumberFieldValue)owner.GetValue(field)).Value = value;
        }

        private static void SetText(ICardFieldOwner owner, CardFieldDefinition field, string value)
        {
            ((TextFieldValue)owner.GetValue(field)).Value = value;
        }

        private static void SetBoolean(ICardFieldOwner owner, CardFieldDefinition field, bool value)
        {
            ((BooleanFieldValue)owner.GetValue(field)).Value = value;
        }

        private static void SetColor(ICardFieldOwner owner, CardFieldDefinition field, Color value)
        {
            ((ColorFieldValue)owner.GetValue(field)).Value = value;
        }

        private static void SetReference(ICardFieldOwner owner, CardFieldDefinition field, MetadataEntry value)
        {
            ((ReferenceFieldValue)owner.GetValue(field)).Value = value;
        }

        private static void SetObject(ICardFieldOwner owner, CardFieldDefinition field, UnityEngine.Object value)
        {
            ((ObjectFieldValue)owner.GetValue(field)).Value = value;
        }

        private static List<CardEffect> EffectsOf(ICardFieldOwner owner, CardFieldDefinition field)
        {
            return ((EffectListFieldValue)owner.GetValue(field)).Effects;
        }

        private static List<CardTrigger> TriggersOf(ICardFieldOwner owner, CardFieldDefinition field)
        {
            return ((TriggerListFieldValue)owner.GetValue(field)).Triggers;
        }

        private static List<EnemyActionSpec> PatternOf(ICardFieldOwner owner, CardFieldDefinition field)
        {
            return ((EnemyPatternFieldValue)owner.GetValue(field)).Steps;
        }

        private static DeckDefinition Deck(string name, params (CardDefinition card, int count)[] entries)
        {
            return GetOrCreate<DeckDefinition>(name, deck =>
            {
                foreach ((CardDefinition card, int count) in entries)
                {
                    deck.Cards.Add(new DeckEntry { Card = card, Count = count });
                }
            }, "Decks");
        }

        private static ZoneDefinition Zone(string name, ZoneVisibility visibility)
        {
            return GetOrCreate<ZoneDefinition>(name, zone => zone.Visibility = visibility, "Zones");
        }
    }
}
