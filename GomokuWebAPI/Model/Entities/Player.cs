using GomokuWebAPI.Authentication;

namespace GomokuWebAPI.Model.Entities
{
    public class Player : AuditableEntity
    {
        public Player()
        {
            Name = "";
            Symbol = 'x';
            User = null!; //it cause problems ?
        }
        public string Name { get; set; }
        public char Symbol { get; set; }
        public long UserId { get; set; }
        public AppUser User { get; set; }
        public ICollection<GamePlayer>? Games { get; set; }
    }
}
