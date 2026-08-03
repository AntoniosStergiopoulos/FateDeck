namespace FateDeck.Runtime.Run
{
    public enum ShrineKind
    {
        /// <summary>Exile 1 card free; exiling Doom costs gold.</summary>
        Ash,

        /// <summary>Gifts Flame cards (Biome 1's scripted Flame tutorial).</summary>
        Forge,

        /// <summary>Heal wounds back into the draw pile.</summary>
        Stitches
    }

    public class ShrineRoomDefinition : RoomDefinition
    {
        public ShrineKind Kind = ShrineKind.Ash;

        public override string DoorLabel()
        {
            switch (Kind)
            {
                case ShrineKind.Forge: return "SHRINE of the Forge";
                case ShrineKind.Stitches: return "SHRINE of Stitches";
                default: return "SHRINE of Ash";
            }
        }
    }
}
