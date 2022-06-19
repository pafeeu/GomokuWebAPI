namespace GomokuWebAPI.Common
{
    public record PairStringShort
    {
        public PairStringShort()
        {
            Name = null!;
        }
        public string Name { get; set; }
        public short Id { get; set; }
    }
}
