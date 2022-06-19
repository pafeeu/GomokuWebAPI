using GomokuWebAPI.Model.Enums;

namespace GomokuWebAPI.DTO
{
    public record GameDetailsResponse
    {
        public long Id { get; init; }
        public short StatusId { get; init; }
        public GameStatus Status { get; init; }
        public List<Player> Players { get; init; }
        public List<Move> Moves { get; init; }
        public record Move
        {
            public long PlayerId { get; init; }
            public short X { get; init; }
            public short Y { get; init; }
            public DateTime Time { get; init; }
        }
        public record Player
        {
            public long Id { get; init; }
            public long PlayerId { get; init; }
            public string Username { get; init; }
            public short RoleId { get; init; }
            public PlayerRole Role { get; init; }
        }
    }
}
