using GomokuWebAPI.Model;
using GomokuWebAPI.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace GomokuWebAPI.Repositories
{
    public class MoveRepository
    {
        private readonly AppDbContext _context;
        public MoveRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<long> Create(Move move)
        {
            await _context.Moves.AddAsync(move);
            await Update();
            return move.Id;
        }
        public async Task<Move?> GetLast(long gameId)
        {
            return await _context.Moves
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(x => x.GameId == gameId);
        }
        public async Task<List<Move>?> GetAll(long gameId)
        {
            var game = await _context.Games.SingleOrDefaultAsync(x => x.Id == gameId);
            return game?.Moves?.ToList();
        }
        public async Task Update()
        {
            await _context.SaveChangesAsync();
        }
    }
}
