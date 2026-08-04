using System.Collections.Generic;
using System.IO;
using FateDeck.Runtime.Core;
using FateDeck.Runtime.Simulation;
using UnityEditor;
using UnityEngine;

namespace FateDeck.Editor
{
    /// <summary>
    /// Runs the AutoPlayer balance lab against the real generated content and writes
    /// Documentation/BalanceReport.md. Quick = fast sanity pass; Full = tuning data.
    /// </summary>
    public static class FateDeckBalanceLab
    {
        private const string CatalogPath = "Assets/FateDeck/Generated/Fate Content Catalog.asset";

        [MenuItem("Tools/Fate Deck/Run Balance Simulation (Quick)", false, 60)]
        public static void RunQuick()
        {
            Run(50);
        }

        [MenuItem("Tools/Fate Deck/Run Balance Simulation (Full)", false, 61)]
        public static void RunFull()
        {
            Run(300);
        }

        private static void Run(int runsPerHero)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<FateContentCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogWarning("[FateDeck] No content catalog. Run Tools/Fate Deck/Create Game Content first.");
                return;
            }

            bool previousSuppression = Runtime.Run.FateRunSave.Suppressed;
            Runtime.Run.FateRunSave.Suppressed = true;
            try
            {
                var reports = new List<HeroReport>();
                for (int i = 0; i < catalog.Heroes.Count; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Fate Deck balance lab",
                            $"Simulating {catalog.Heroes[i].name} ({runsPerHero} runs)...",
                            (float)i / catalog.Heroes.Count))
                    {
                        return;
                    }

                    reports.Add(RunSimulator.Simulate(catalog, catalog.Heroes[i], runsPerHero));
                }

                string report = RunSimulator.ToMarkdown(reports, catalog.Rules,
                    $"Content: the project's generated catalog. Runs per hero: {runsPerHero}.");
                string folder = Path.Combine(Application.dataPath, "FateDeck", "Documentation");
                Directory.CreateDirectory(folder);
                string path = Path.GetFullPath(Path.Combine(folder, "BalanceReport.md"));
                File.WriteAllText(path, report);
                AssetDatabase.Refresh();

                foreach (HeroReport hero in reports)
                {
                    Debug.Log($"[FateDeck] {hero.HeroName}: {hero.WinRate:P0} win rate, "
                        + $"avg death step {hero.AverageDeathStep:0.0}, stalls {hero.Stalls}.");
                }

                Debug.Log($"[FateDeck] Balance report written to {path}");
            }
            finally
            {
                Runtime.Run.FateRunSave.Suppressed = previousSuppression;
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
