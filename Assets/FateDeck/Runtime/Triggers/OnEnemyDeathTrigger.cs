using System;
using AStergio.OmniCard.Runtime.Cards.Gameplay;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.Triggers;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Triggers
{
    /// <summary>Fires when an enemy dies; with SelfOnly, only for the card carrying this trigger.</summary>
    [Serializable]
    public class OnEnemyDeathTrigger : CardTrigger
    {
        public bool SelfOnly = true;

        public override IDisposable Activate(CardInstance owner, IGameContext game)
        {
            void Handler(EnemyDiedEvent died)
            {
                if (SelfOnly && died.Enemy != owner)
                {
                    return;
                }

                Fire(owner, game);
            }

            game.Events.Subscribe<EnemyDiedEvent>(Handler);
            return new Subscription(() => game.Events.Unsubscribe<EnemyDiedEvent>(Handler));
        }
    }
}
