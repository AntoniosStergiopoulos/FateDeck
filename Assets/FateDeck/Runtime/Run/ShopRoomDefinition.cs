namespace FateDeck.Runtime.Run
{
    public class ShopRoomDefinition : RoomDefinition
    {
        public bool MiniShop;

        public override string DoorLabel() => MiniShop ? "MINI-SHOP" : "SHOP";
    }
}
