using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Instances;
using FateDeck.Runtime.Core;
using FateDeck.Runtime.Run;
using NUnit.Framework;

namespace FateDeck.Tests
{
    public sealed class FateEngineTests
    {
        private TestContent _content;
        private FateSession _session;

        [SetUp]
        public void SetUp()
        {
            _content = TestContent.Create();
            _session = new FateSession(_content.Catalog, seed: 7, log: _ => { });
            _session.SetHero(_content.Hero);
        }

        [TearDown]
        public void TearDown()
        {
            _session.Dispose();
            _content.Destroy();
        }

        [Test]
        public void StartingDeckBuildsFromHero()
        {
            Assert.AreEqual(9, _session.Deck.Draw.Count);
            Assert.AreEqual(2, _session.PocketSlots);
        }

        [Test]
        public void ReshufflePaysTheDoomTax()
        {
            while (_session.Deck.Draw.Count > 0)
            {
                _session.Deck.ToDiscard(_session.Deck.Draw.RemoveTop());
            }

            Assert.IsTrue(_session.Deck.Reshuffle());
            Assert.AreEqual(1, _session.Deck.ReshuffleCount);
            Assert.AreEqual(10, _session.Deck.Draw.Count);
            Assert.AreEqual(2, _session.Deck.CountForceIn(_session.Deck.Draw, _content.Catalog.Doom));
        }

        [Test]
        public void MilledDoomIsExiledForever()
        {
            _session.Deck.MoveForceToTop(_content.Catalog.Doom);
            _session.MillPlayer(1);
            Assert.AreEqual(1, _session.Deck.Exile.Count);
            Assert.AreEqual(0, _session.Deck.Wound.Count);
            Assert.AreEqual(0, _session.Deck.CountForceIn(_session.Deck.Draw, _content.Catalog.Doom));
        }

        [Test]
        public void MilledIronLandsInTheWoundRow()
        {
            _session.Deck.MoveForceToTop(_content.Catalog.Iron);
            _session.MillPlayer(1);
            Assert.AreEqual(1, _session.Deck.Wound.Count);
            Assert.AreEqual(0, _session.Deck.Exile.Count);
        }

        [Test]
        public void StrikeWithRiggedIronDealsFivePlusLaw()
        {
            var combat = _session.StartCombat(_content.FightRoom);
            CardInstance enemy = combat.Enemies.Cards[0];
            _session.Deck.MoveForceToTop(_content.Catalog.Iron);

            combat.PlayerStrike(enemy);
            Assert.AreEqual(FateResolutionPhase.AwaitPreFlip, _session.Phase);
            _session.ContinueFlip();
            Assert.AreEqual(FateResolutionPhase.AwaitBank, _session.Phase);
            _session.DeclineBank();

            Assert.AreEqual(FateResolutionPhase.Idle, _session.Phase);
            Assert.AreEqual(3, enemy.Fields.GetNumber(_content.Catalog.HpField));
        }

        [Test]
        public void BankedCardResolvesAtBaseValueAndFillsThePocket()
        {
            var combat = _session.StartCombat(_content.FightRoom);
            CardInstance enemy = combat.Enemies.Cards[0];
            _session.Deck.MoveForceToTop(_content.Catalog.Iron);

            combat.PlayerStrike(enemy);
            _session.ContinueFlip();
            _session.BankRevealed();

            Assert.AreEqual(1, _session.Deck.Pocket.Count);
            Assert.AreEqual(5, enemy.Fields.GetNumber(_content.Catalog.HpField));
        }

        [Test]
        public void PocketPlayReplacesAnEnemyFlipEntirely()
        {
            var combat = _session.StartCombat(_content.FightRoom);
            _session.Deck.MoveForceToTop(_content.Catalog.Iron);
            combat.PlayerStrike(combat.Enemies.Cards[0]);
            _session.ContinueFlip();
            _session.BankRevealed();

            int drawBefore = _session.Deck.Draw.Count;
            Assert.IsTrue(combat.TryAdvance());
            Assert.AreEqual(FateResolutionPhase.AwaitPreFlip, _session.Phase);
            CardInstance pocketed = _session.Deck.Pocket.Cards[0];
            Assert.IsTrue(_session.PlayPocket(pocketed));

            Assert.AreEqual(drawBefore - 5, _session.Deck.Draw.Count,
                "no card leaves the deck for the flip itself; only the Attack 3 + Iron 2 mill does");
            Assert.AreEqual(0, _session.Deck.Pocket.Count);
            Assert.IsTrue(_session.Deck.Discard.Contains(pocketed));
        }

        [Test]
        public void DoomCannotBePocketed()
        {
            var doom = new CardInstance(_content.DoomCard);
            Assert.IsFalse(_session.Deck.CanPocket(doom, pocketSlots: 2));
        }

        [Test]
        public void OddsTableMatchesCompositionAndLaws()
        {
            List<OddsRow> rows = OddsCalculator.Table(_content.Catalog, _session.Deck,
                LawContext.PlayerOffense, baseForce: 3);

            int total = 0;
            foreach (OddsRow row in rows)
            {
                Assert.AreEqual(9, row.Total);
                total += row.Count;
                if (row.Force == _content.Catalog.Iron)
                {
                    Assert.AreEqual(5, row.Count);
                    Assert.AreEqual(5, row.ResultForce);
                }

                if (row.Force == _content.Catalog.Doom)
                {
                    Assert.AreEqual(0, row.ResultForce);
                }
            }

            Assert.AreEqual(9, total);
        }

        [Test]
        public void DoorDealerGuaranteesTheEliteByStepSeven()
        {
            var pool = new List<RoomDefinition> { _content.FightRoom };
            var elite = UnityEngine.ScriptableObject.CreateInstance<FightRoomDefinition>();
            elite.IsElite = true;
            elite.Encounter = _content.FightRoom.Encounter;
            _content.Owned.Add(elite);

            bool offered = false;
            var rng = new System.Random(3);
            for (int step = 5; step <= 7 && !offered; step++)
            {
                List<RoomDefinition> doors = DoorDealer.Deal(pool, elite, step, 3, rng, ref offered);
                if (step == 7)
                {
                    Assert.IsTrue(doors.Contains(elite));
                }
            }

            Assert.IsTrue(offered);
        }

        [Test]
        public void EnemyDeathPaysBountyAndEndsCombat()
        {
            var combat = _session.StartCombat(_content.FightRoom);
            int goldBefore = _session.Gold;
            CardInstance enemy = combat.Enemies.Cards[0];
            combat.DamageEnemy(enemy, 99);

            Assert.AreEqual(goldBefore + 5, _session.Gold);
            Assert.IsTrue(combat.Victory);
            Assert.AreEqual(0, combat.Enemies.Count);
        }
    }
}
