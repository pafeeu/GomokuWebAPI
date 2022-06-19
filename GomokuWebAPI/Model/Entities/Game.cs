using GomokuWebAPI.Model.Enums;

namespace GomokuWebAPI.Model.Entities
{
    public class Game : AuditableEntity
    {
        public Game()
        {
            GameStatus = GameStatus.New;
        }
        public GameStatus GameStatus { get; set; }
        public ICollection<Move>? Moves { get; set; }
        public ICollection<GamePlayer>? Players { get; set; }
    }
}
