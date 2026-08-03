using System.Text;

namespace FateDeck.Runtime.Run
{
    public class BossRoomDefinition : FightRoomDefinition
    {
        public override bool IsBoss => true;

        public override string DoorLabel()
        {
            var builder = new StringBuilder("BOSS: ");
            AppendEnemies(builder);
            return builder.ToString();
        }
    }
}
