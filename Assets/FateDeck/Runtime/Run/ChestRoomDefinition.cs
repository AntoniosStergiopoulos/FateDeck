namespace FateDeck.Runtime.Run
{
    public class ChestRoomDefinition : RoomDefinition
    {
        public bool Locked;

        public override string DoorLabel() => Locked ? "LOCKED CHEST (base 14g)" : "CHEST (base 8g)";
    }
}
