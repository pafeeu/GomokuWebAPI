using GomokuWebAPI.Common;
using GomokuWebAPI.Model;
using GomokuWebAPI.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace GomokuWebAPI.Services
{
    public class ConditionCheckService
    {
        private readonly AppDbContext _context;
        public ConditionCheckService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> PlayerExists(long playerId)
        {
            return await _context.Players.AnyAsync(x => x.Id == playerId);
        }
        public async Task<bool> GameExists(long gameId)
        {
            return await _context.Games.AnyAsync(x => x.Id == gameId);
        }
        public async Task<bool> PlayerIsInGame(long playerId, long gameId)
        {
            return await _context.GamePlayers
                .AnyAsync(x=>x.PlayerId == playerId && x.GameId == gameId);
        }
        /// <summary>
        /// Checks that the game has fewer than the maximum players
        /// </summary>
        /// <returns>True if there is free space for new player, otherwise false</returns>
        public async Task<bool> FreeSpaceForPlayerInGame(long gameId)
        {
            return Statics.MAX_PLAYERS_IN_GAME > await _context.GamePlayers.Where(x=>x.GameId == gameId).CountAsync();
        }
    }
}
