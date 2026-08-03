using AStergio.OmniCard.Runtime.Cards.Events;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;

namespace FateDeck.Runtime.Core
{
    /// <summary>A fate card was revealed for an action and its law is about to apply (or was banked).</summary>
    public sealed class FateFlipEvent : IGameEvent
    {
        public FateFlipEvent(CardInstance card, MetadataEntry force, FateAction action, bool fromPocket)
        {
            Card = card;
            Force = force;
            Action = action;
            FromPocket = fromPocket;
        }

        public CardInstance Card { get; }
        public MetadataEntry Force { get; }
        public FateAction Action { get; }
        public bool FromPocket { get; }
    }

    /// <summary>An action finished resolving with its final force committed.</summary>
    public sealed class ActionResolvedEvent : IGameEvent
    {
        public ActionResolvedEvent(FateAction action)
        {
            Action = action;
        }

        public FateAction Action { get; }
    }

    public sealed class CardMilledEvent : IGameEvent
    {
        public CardMilledEvent(CardInstance card, MetadataEntry force, bool exiled)
        {
            Card = card;
            Force = force;
            Exiled = exiled;
        }

        public CardInstance Card { get; }
        public MetadataEntry Force { get; }

        /// <summary>True when the milled card was Doom-laundered straight to exile.</summary>
        public bool Exiled { get; }
    }

    public sealed class ReshuffleEvent : IGameEvent
    {
        public ReshuffleEvent(int taxAdded, int reshuffleCount)
        {
            TaxAdded = taxAdded;
            ReshuffleCount = reshuffleCount;
        }

        public int TaxAdded { get; }
        public int ReshuffleCount { get; }
    }

    public sealed class PocketBankedEvent : IGameEvent
    {
        public PocketBankedEvent(CardInstance card)
        {
            Card = card;
        }

        public CardInstance Card { get; }
    }

    public sealed class PocketPlayedEvent : IGameEvent
    {
        public PocketPlayedEvent(CardInstance card, FateAction action)
        {
            Card = card;
            Action = action;
        }

        public CardInstance Card { get; }
        public FateAction Action { get; }
    }

    public sealed class WoundHealedEvent : IGameEvent
    {
        public WoundHealedEvent(CardInstance card)
        {
            Card = card;
        }

        public CardInstance Card { get; }
    }

    public sealed class CardExiledEvent : IGameEvent
    {
        public CardExiledEvent(CardInstance card)
        {
            Card = card;
        }

        public CardInstance Card { get; }
    }

    public sealed class GoldChangedEvent : IGameEvent
    {
        public GoldChangedEvent(int oldValue, int newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public int OldValue { get; }
        public int NewValue { get; }
    }

    public sealed class StatusChangedEvent : IGameEvent
    {
        public StatusChangedEvent(CardInstance enemy, StatusKind status, int stacks)
        {
            Enemy = enemy;
            Status = status;
            Stacks = stacks;
        }

        /// <summary>The afflicted enemy, or null when the player is afflicted.</summary>
        public CardInstance Enemy { get; }
        public StatusKind Status { get; }
        public int Stacks { get; }
    }

    public sealed class EnemyDiedEvent : IGameEvent
    {
        public EnemyDiedEvent(CardInstance enemy, int bounty)
        {
            Enemy = enemy;
            Bounty = bounty;
        }

        public CardInstance Enemy { get; }
        public int Bounty { get; }
    }

    public sealed class CombatStartedEvent : IGameEvent
    {
    }

    public sealed class CombatEndedEvent : IGameEvent
    {
        public CombatEndedEvent(bool victory)
        {
            Victory = victory;
        }

        public bool Victory { get; }
    }

    public sealed class PlayerTurnStartedEvent : IGameEvent
    {
        public PlayerTurnStartedEvent(int round)
        {
            Round = round;
        }

        public int Round { get; }
    }

    public sealed class RoomEndedEvent : IGameEvent
    {
    }

    public sealed class PlayerDiedEvent : IGameEvent
    {
        public PlayerDiedEvent(FateAction fatalAction)
        {
            FatalAction = fatalAction;
        }

        public FateAction FatalAction { get; }
    }

    public sealed class DealerBarkEvent : IGameEvent
    {
        public DealerBarkEvent(string line)
        {
            Line = line;
        }

        public string Line { get; }
    }

    /// <summary>The top cards of the draw pile were revealed to the player (Scry).</summary>
    public sealed class ScryEvent : IGameEvent
    {
        public ScryEvent(System.Collections.Generic.IReadOnlyList<CardInstance> cards, bool allowReorder)
        {
            Cards = cards;
            AllowReorder = allowReorder;
        }

        public System.Collections.Generic.IReadOnlyList<CardInstance> Cards { get; }
        public bool AllowReorder { get; }
    }

    /// <summary>An enemy took damage (after its Block); precise feedback for the log.</summary>
    public sealed class EnemyDamagedEvent : IGameEvent
    {
        public EnemyDamagedEvent(CardInstance enemy, double dealt, double absorbed, double remainingHp)
        {
            Enemy = enemy;
            Dealt = dealt;
            Absorbed = absorbed;
            RemainingHp = remainingHp;
        }

        public CardInstance Enemy { get; }
        public double Dealt { get; }
        public double Absorbed { get; }
        public double RemainingHp { get; }
    }

    /// <summary>An enemy attack landed on the player: how much Block ate and how much milled.</summary>
    public sealed class PlayerHitEvent : IGameEvent
    {
        public PlayerHitEvent(CardInstance attacker, double incoming, double absorbed, int milled)
        {
            Attacker = attacker;
            Incoming = incoming;
            Absorbed = absorbed;
            Milled = milled;
        }

        public CardInstance Attacker { get; }
        public double Incoming { get; }
        public double Absorbed { get; }
        public int Milled { get; }
    }

    /// <summary>The interactive resolution phase changed; views re-render prompts from this.</summary>
    public sealed class ResolutionPhaseChangedEvent : IGameEvent
    {
        public ResolutionPhaseChangedEvent(FateResolutionPhase phase, FateAction action)
        {
            Phase = phase;
            Action = action;
        }

        public FateResolutionPhase Phase { get; }
        public FateAction Action { get; }
    }
}
