using AStergio.OmniCard.Runtime.Cards.Gameplay;
using AStergio.OmniCard.Runtime.Cards.Instances;
using FateDeck.Runtime.Combat;

namespace FateDeck.Runtime.Core
{
    /// <summary>
    /// The seam Fate Deck effect/trigger/condition atoms resolve against, mirroring how
    /// OmniCard atoms cast <c>context.Game</c> to <c>IGameSession</c>.
    /// </summary>
    public interface IFateSession : IGameContext
    {
        FateContentCatalog Catalog { get; }

        FateRulesDefinition Rules { get; }

        System.Random Rng { get; }

        FateDeckService Deck { get; }

        /// <summary>The active combat, or null between fights.</summary>
        CombatEngine Combat { get; }

        /// <summary>The action currently moving through the fate pipeline, or null.</summary>
        FateAction CurrentAction { get; }

        /// <summary>The force whose law most recently resolved - what a Mirror flip repeats.</summary>
        AStergio.OmniCard.Runtime.Cards.MetaData.MetadataEntry LastFlippedForce { get; }

        /// <summary>Keys open locked chests without gambling on Flame.</summary>
        int Keys { get; }

        void AddKeys(int delta);

        int Gold { get; }

        void AddGold(int delta);

        int PocketSlots { get; }

        void AddPocketSlots(int delta);

        double PlayerBlock { get; }

        void AddPlayerBlock(double delta);

        int GetStatus(CardInstance enemy, StatusKind status);

        void AddStatus(CardInstance enemy, StatusKind status, int stacks);

        /// <summary>Queues one extra fate flip on the current action (the Echo law).</summary>
        void QueueEchoFlip();

        /// <summary>+N force to the player's next declared action (Rustheart-style rewards).</summary>
        void AddNextPlayerActionBonus(double delta);

        /// <summary>Grants draw-2-choose-1 charges consumed by upcoming player flips.</summary>
        void AddDoubleDrawCharges(int count);

        /// <summary>Reveals the top cards to the player (Scry); the view renders the result.</summary>
        void Scry(int count, bool allowReorder);

        void MillPlayer(int count, string reason = null);

        void Bark(string line);

        bool IsPlayerDead { get; }
    }
}
