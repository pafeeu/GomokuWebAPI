namespace GomokuWebAPI.DTO
{
    public record PlayerResponse
    {
        public PlayerResponse()
        {
            Name = null!;
        }
        public long Id { get; init; }
        public string Name { get; init; }
        public char Symbol { get; init; }
    }
}
