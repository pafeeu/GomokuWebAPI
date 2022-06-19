using GomokuWebAPI.Model.Enums;

namespace GomokuWebAPI.DTO
{
    public record GameResponse
    {
        public long Id { get; init; }
        public short StatusId { get; init; }
        public GameStatus Status { get; init; }
        public int MovesNumber { get; init; }
        public int PlayersNumber { get; init; }
    }
}
