using FateDeck.Runtime.Core;
using FateDeck.Runtime.Views;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FateDeck.Editor
{
    /// <summary>Builds the one-screen Fate Deck table into the open scene: camera + table controller.</summary>
    public static class FateDeckSceneBuilder
    {
        [MenuItem("Tools/Fate Deck/Create Game Scene", false, 1)]
        public static void CreateScene()
        {
            FateContentCatalog catalog = FateDeckContentGenerator.CreateAssets();

            Camera camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                Undo.RegisterCreatedObjectUndo(cameraObject, "Create Fate Deck Camera");
            }

            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.071f, 0.063f, 0.055f);

            var existing = Object.FindFirstObjectByType<FateTableView>();
            if (existing != null)
            {
                EnsureDocument(existing.gameObject, catalog);
                var existingSerialized = new SerializedObject(existing);
                SerializedProperty catalogProperty = existingSerialized.FindProperty("_catalog");
                if (catalogProperty.objectReferenceValue == null)
                {
                    catalogProperty.objectReferenceValue = catalog;
                    existingSerialized.ApplyModifiedPropertiesWithoutUndo();
                }

                Debug.Log("[FateDeck] A Fate Table already exists in the scene; references refreshed.");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            var tableObject = new GameObject("Fate Table");
            FateTableView table = tableObject.AddComponent<FateTableView>();
            EnsureDocument(tableObject, catalog);
            var serialized = new SerializedObject(table);
            serialized.FindProperty("_catalog").objectReferenceValue = catalog;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Undo.RegisterCreatedObjectUndo(tableObject, "Create Fate Table");
            Selection.activeGameObject = tableObject;
            Debug.Log("[FateDeck] Table ready. Press Play - the Dealer is waiting.");
        }

        /// <summary>The UI Toolkit document that hosts the whole table UI (added by RequireComponent,
        /// but older scenes may predate it) with the generated PanelSettings assigned.</summary>
        private static void EnsureDocument(GameObject host, FateContentCatalog catalog)
        {
            var document = host.GetComponent<UIDocument>();
            if (document == null)
            {
                document = host.AddComponent<UIDocument>();
            }

            if (document.panelSettings == null)
            {
                document.panelSettings = catalog.Panel;
                EditorUtility.SetDirty(document);
            }
        }
    }
}
