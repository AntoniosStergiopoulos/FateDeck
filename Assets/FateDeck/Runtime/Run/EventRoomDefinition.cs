namespace FateDeck.Runtime.Run
{
    public class EventRoomDefinition : RoomDefinition
    {
        public EventDefinition Event;

        public override string DoorLabel() => "? EVENT";
    }
}
