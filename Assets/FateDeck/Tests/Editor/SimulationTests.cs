using AStergio.OmniCard.Runtime.Cards.Game.Decks;
using FateDeck.Runtime.Run;
using FateDeck.Runtime.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FateDeck.Tests
{
    /// <summary>
    /// The balance lab's own safety net: the AutoPlayer must finish every seed (no stalls,
    /// no exceptions), never touch the real run save, and the aggregator must account for
    /// every run it was given.
    /// </summary>
    public sealed class SimulationTests
    {
        private TestContent _content;

        [SetUp]
        public void SetUp()
        {
            _content = TestContent.Create();
            _content.Catalog.Biome1Rooms.Add(_content.FightRoom);
            _content.Catalog.Biome1Rooms.Add(_content.PairRoom);

            var boss = ScriptableObject.CreateInstance<BossRoomDefinition>();
            boss.name = "Boss";
            _content.Owned.Add(boss);
            var encounter = ScriptableObject.CreateInstance<DeckDefinition>();
            encounter.name = "Boss Encounter";
            _content.Owned.Add(encounter);
            encounter.Cards.Add(new DeckEntry { Card = _content.Collector, Count = 1 });
            boss.Encounter = encounter;
            _content.Catalog.Biome1Boss = boss;
        }

        [TearDown]
        public void TearDown()
        {
            _content.Destroy();
        }

        [Test]
        public void AutoPlayerFinishesEverySeedWithoutStalling()
        {
            var player = new AutoPlayer(_content.Catalog);
            for (int seed = 1; seed <= 8; seed++)
            {
                RunResult result = player.Play(_content.Hero, seed);
                Assert.IsFalse(result.Stalled, $"seed {seed} stalled");
                Assert.IsTrue(result.Victory || result.DeathStep > 0, $"seed {seed} ended nowhere");
                Assert.Greater(result.Flips, 0, $"seed {seed} never flipped fate");
            }
        }

        [Test]
        public void AutoPlayerNeverTouchesTheRealRunSave()
        {
            FateRunSave.Delete();
            new AutoPlayer(_content.Catalog).Play(_content.Hero, 3);
            Assert.IsFalse(FateRunSave.Exists);
            Assert.IsFalse(FateRunSave.Suppressed, "suppression flag leaked");
        }

        [Test]
        public void SimulatorAccountsForEveryRun()
        {
            HeroReport report = RunSimulator.Simulate(_content.Catalog, _content.Hero, runs: 6);
            int deaths = 0;
            foreach (int count in report.DeathsByStep)
            {
                deaths += count;
            }

            Assert.AreEqual(6, report.Runs);
            Assert.AreEqual(6, report.Victories + report.Stalls + deaths);
            Assert.Greater(report.AverageFlips, 0);

            string markdown = RunSimulator.ToMarkdown(new[] { report }, _content.Catalog.Rules);
            StringAssert.Contains(report.HeroName, markdown);
            StringAssert.Contains("Deaths by step", markdown);
        }
    }
}
