using System;
using AStergio.OmniCard.Runtime.Cards.Gameplay;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.Triggers;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Triggers
{
    /// <summary>Fires when the discard shuffles back into the draw pile (after the Doom tax).</summary>
    [Serializable]
    public class OnReshuffleTrigger : CardTrigger
    {
        public override IDisposable Activate(CardInstance owner, IGameContext game)
        {
            void Handler(ReshuffleEvent _)
            {
                Fire(owner, game);
            }

            game.Events.Subscribe<ReshuffleEvent>(Handler);
            return new Subscription(() => game.Events.Unsubscribe<ReshuffleEvent>(Handler));
        }
    }
}
