using System;
using AStergio.OmniCard.Runtime.Cards.Gameplay;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.Triggers;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Triggers
{
    /// <summary>Fires at the start of every player turn in combat (round-based passives).</summary>
    [Serializable]
    public class OnPlayerTurnStartTrigger : CardTrigger
    {
        /// <summary>When true the trigger skips round 1 (the combat-start round).</summary>
        public bool SkipFirstRound;

        public override IDisposable Activate(CardInstance owner, IGameContext game)
        {
            void Handler(PlayerTurnStartedEvent turn)
            {
                if (SkipFirstRound && turn.Round <= 1)
                {
                    return;
                }

                Fire(owner, game);
            }

            game.Events.Subscribe<PlayerTurnStartedEvent>(Handler);
            return new Subscription(() => game.Events.Unsubscribe<PlayerTurnStartedEvent>(Handler));
        }
    }
}
