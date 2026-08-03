using System.Text;
using AStergio.OmniCard.Runtime.Cards.Game.Decks;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using UnityEngine;

namespace FateDeck.Runtime.Run
{
    public class FightRoomDefinition : RoomDefinition
    {
        public DeckDefinition Encounter;
        public bool IsElite;

        [Tooltip("Scripted openings: this force is placed on top of the draw pile at combat start.")]
        public MetadataEntry RiggedTopForce;

        [Range(0f, 1f)]
        public float CharmDropChance = 0.2f;

        public virtual bool IsBoss => false;

        public override string DoorLabel()
        {
            var builder = new StringBuilder(IsElite ? "ELITE: " : "FIGHT: ");
            AppendEnemies(builder);
            return builder.ToString();
        }

        protected void AppendEnemies(StringBuilder builder)
        {
            if (Encounter == null)
            {
                builder.Append("???");
                return;
            }

            bool first = true;
            foreach (DeckEntry entry in Encounter.Cards)
            {
                if (entry?.Card == null)
                {
                    continue;
                }

                for (int i = 0; i < entry.Count; i++)
                {
                    if (!first)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(entry.Card.name);
                    first = false;
                }
            }
        }
    }
}
