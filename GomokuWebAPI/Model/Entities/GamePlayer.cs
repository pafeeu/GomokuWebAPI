using GomokuWebAPI.Model.Enums;

namespace GomokuWebAPI.Model.Entities
{
    public class GamePlayer : AuditableEntity
    {
        public GamePlayer()
        {
            Player = null!;
            Game = null!;
            Roles = PlayerRole.None;
        }
        public long PlayerId { get; set; }
        public Player Player { get; set; }
        public long GameId { get; set; }
        public Game Game { get; set; }
        public PlayerRole Roles { get; set; }
    }
}
