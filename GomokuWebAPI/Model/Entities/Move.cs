namespace GomokuWebAPI.Model.Entities
{
    public class Move : AuditableEntity
    {
        public Move()
        {
            Player = null!;
            Game = null!;
        }
        public long PlayerId { get; set; }
        public Player Player { get; set; }
        public long GameId { get; set; }
        public Game Game { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
    }
}
