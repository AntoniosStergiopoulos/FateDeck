using System;
using AStergio.OmniCard.Runtime.Cards.Gameplay;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using AStergio.OmniCard.Runtime.Cards.Triggers;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Triggers
{
    public enum ActionOwnerFilter
    {
        Any,
        Player,
        Enemy
    }

    /// <summary>
    /// Fires when a fate card is flipped (GDD atom <c>OnFlip</c>): filter by law context,
    /// force, and which side owns the action. Relic laws are built from this.
    /// </summary>
    [Serializable]
    public class OnFateFlipTrigger : CardTrigger
    {
        public bool AnyContext = true;
        public LawContext Context = LawContext.PlayerOffense;
        public MetadataEntry ForceFilter;
        public ActionOwnerFilter Owner = ActionOwnerFilter.Any;
        public bool IncludePocketPlays = true;

        public override IDisposable Activate(CardInstance owner, IGameContext game)
        {
            void Handler(FateFlipEvent flip)
            {
                if (!Matches(flip))
                {
                    return;
                }

                Fire(owner, game);
            }

            game.Events.Subscribe<FateFlipEvent>(Handler);
            return new Subscription(() => game.Events.Unsubscribe<FateFlipEvent>(Handler));
        }

        private bool Matches(FateFlipEvent flip)
        {
            if (flip?.Action == null)
            {
                return false;
            }

            if (!IncludePocketPlays && flip.FromPocket)
            {
                return false;
            }

            if (!AnyContext && flip.Action.Context != Context)
            {
                return false;
            }

            if (ForceFilter != null && flip.Force != ForceFilter)
            {
                return false;
            }

            if (Owner == ActionOwnerFilter.Player && !flip.Action.IsPlayerAction)
            {
                return false;
            }

            if (Owner == ActionOwnerFilter.Enemy && flip.Action.SourceEnemy == null)
            {
                return false;
            }

            return true;
        }
    }
}
