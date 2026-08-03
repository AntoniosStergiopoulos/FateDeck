using AStergio.OmniCard.Runtime.Cards.MetaData;
using UnityEngine;

namespace FateDeck.Runtime.Run
{
    /// <summary>
    /// A room behind a door. Doors show their contents up front - choosing which randomness
    /// to face is a core decision, so <see cref="DoorLabel"/> must be honest.
    /// </summary>
    public abstract class RoomDefinition : CardAsset
    {
        [TextArea]
        public string Blurb;

        public abstract string DoorLabel();
    }
}
