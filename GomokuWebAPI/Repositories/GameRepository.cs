using GomokuWebAPI.Common;
using GomokuWebAPI.Model;
using GomokuWebAPI.Model.Entities;
using GomokuWebAPI.Model.Enums;
using GomokuWebAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace GomokuWebAPI.Repositories
{
    public class GameRepository
    {
        private readonly AppDbContext _context;
        private readonly ConditionCheckService _conditionCheck;
        private readonly CurrentUserService _currentUser;
        public GameRepository(AppDbContext context, CurrentUserService currentUser, ConditionCheckService conditionCheck)
        {
            _context = context;
            _currentUser = currentUser;
            _conditionCheck = conditionCheck;
        }
        public async Task<long> Create(long? invitedPlayerId = null)
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {
                var game = await _context.Games.AddAsync(new Game());
                await Update();
                var gameId = game.Entity.Id;
                var currentPlayerId = await _currentUser.PlayerId();
                var playerCreator = new GamePlayer()
                {
                    PlayerId = currentPlayerId,
                    GameId = gameId,
                    Roles = PlayerRole.Creator
                };
                await _context.GamePlayers.AddAsync(playerCreator);
                if(invitedPlayerId is not null && await _conditionCheck.PlayerExists((long)invitedPlayerId))
                {
                    var playerInvited = new GamePlayer()
                    {
                        PlayerId = (long)invitedPlayerId,
                        GameId = gameId,
                        Roles = PlayerRole.Invited
                    };
                    await _context.GamePlayers.AddAsync(playerInvited);
                }

                await Update();
                await transaction.CommitAsync();
                return gameId;
            } 
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new NotFoundException($"Creating the game failed \n {e.Message}");
            }
        }
        public async Task<long> AddGamePlayer(long gameId, long playerId)
        {
            if (!await _conditionCheck.PlayerExists(playerId))
                throw new NotFoundException("Player not found");

            if(!await _conditionCheck.GameExists(gameId))
                throw new NotFoundException("Game not found");

            if (!await _conditionCheck.FreeSpaceForPlayerInGame(gameId)
                || await _conditionCheck.PlayerIsInGame(playerId, gameId)
                || await GetStatus(gameId)>=GameStatus.Confirmed)
                throw new NotFoundException("You can't join");

            var player = await _context.GamePlayers.AddAsync(
                new GamePlayer()
                {
                    PlayerId = playerId,
                    GameId = gameId,
                    Roles = PlayerRole.Invited
                }
            );
            await Update();
            return player.Entity.Id;
        }
        public async Task<Game?> Get(long id)
        {
            return await _context.Games
                .Include(x => x.Players)
                .SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task<Game?> GetFull(long id)
        {
            return await _context.Games
                .Include(x => x.Moves)
                .Include(x => x.Players).ThenInclude(gp => gp.Player)
                .SingleOrDefaultAsync(x => x.Id == id);
        }
        public async Task<GameStatus> GetStatus(long id)
        {
            return await _context.Games
                .Where(x => x.Id == id)
                .Select(x=>x.GameStatus)
                .SingleOrDefaultAsync();
        }
        public async Task<List<Game>> GetAll()
        {
            return await _context.Games
                .Include(x=>x.Moves)
                .Include(x=>x.Players)
                .ToListAsync();
        }
        public async Task<int> Update()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
