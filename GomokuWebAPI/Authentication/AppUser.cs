using GomokuWebAPI.Model.Entities;
using Microsoft.AspNetCore.Identity;

namespace GomokuWebAPI.Authentication
{
    public class AppUser: IdentityUser<long>
    {
        public long? PlayerId { get; set; }
        public Player? Player { get; set; }
    }
}
