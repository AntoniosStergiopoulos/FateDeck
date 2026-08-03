using System;
using AStergio.OmniCard.Runtime.Cards.Gameplay;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.Triggers;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Triggers
{
    /// <summary>Fires when a pocketed card replaces a flip; optionally only on enemy actions.</summary>
    [Serializable]
    public class OnPocketPlayTrigger : CardTrigger
    {
        public bool EnemyActionsOnly;

        public override IDisposable Activate(CardInstance owner, IGameContext game)
        {
            void Handler(PocketPlayedEvent played)
            {
                if (EnemyActionsOnly && played.Action?.SourceEnemy == null)
                {
                    return;
                }

                Fire(owner, game);
            }

            game.Events.Subscribe<PocketPlayedEvent>(Handler);
            return new Subscription(() => game.Events.Unsubscribe<PocketPlayedEvent>(Handler));
        }
    }
}
