using System;
using AStergio.OmniCard.Runtime.Cards.Gameplay;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.Triggers;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Triggers
{
    [Serializable]
    public class OnRoomEndTrigger : CardTrigger
    {
        public override IDisposable Activate(CardInstance owner, IGameContext game)
        {
            void Handler(RoomEndedEvent _)
            {
                Fire(owner, game);
            }

            game.Events.Subscribe<RoomEndedEvent>(Handler);
            return new Subscription(() => game.Events.Unsubscribe<RoomEndedEvent>(Handler));
        }
    }
}
