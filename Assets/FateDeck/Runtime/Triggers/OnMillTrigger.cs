using System;
using AStergio.OmniCard.Runtime.Cards.Gameplay;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using AStergio.OmniCard.Runtime.Cards.Triggers;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Triggers
{
    /// <summary>Fires when the player is milled; optionally only for a given force or Doom-launders.</summary>
    [Serializable]
    public class OnMillTrigger : CardTrigger
    {
        public MetadataEntry ForceFilter;
        public bool OnlyWhenExiled;

        public override IDisposable Activate(CardInstance owner, IGameContext game)
        {
            void Handler(CardMilledEvent milled)
            {
                if (ForceFilter != null && milled.Force != ForceFilter)
                {
                    return;
                }

                if (OnlyWhenExiled && !milled.Exiled)
                {
                    return;
                }

                Fire(owner, game);
            }

            game.Events.Subscribe<CardMilledEvent>(Handler);
            return new Subscription(() => game.Events.Unsubscribe<CardMilledEvent>(Handler));
        }
    }
}
