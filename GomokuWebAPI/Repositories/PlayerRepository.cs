using GomokuWebAPI.Authentication;
using GomokuWebAPI.Model;
using GomokuWebAPI.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace GomokuWebAPI.Repositories
{
    public class PlayerRepository
    {
        private readonly AppDbContext _context;
        public PlayerRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<long> Create(AppUser user)
        {
            var player = new Player() { Name = user.UserName, User = user, UserId = user.Id};
            await _context.AddAsync(player);
            user.Player = player;
            await _context.SaveChangesAsync();
            return player.Id;
        }
        public async Task<Player?> Get(long id)
        {
            return await _context.Players.SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task<List<Player>> GetAll()
        {
            return await _context.Players.ToListAsync();
        }
        public async Task Update()
        {
            await _context.SaveChangesAsync();
        }
    }
}
