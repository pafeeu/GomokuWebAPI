using GomokuWebAPI.Authentication;
using GomokuWebAPI.Common;
using GomokuWebAPI.DTO;
using GomokuWebAPI.Model.Entities;
using GomokuWebAPI.Model.Enums;
using GomokuWebAPI.Repositories;
using GomokuWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GomokuWebAPI.Controllers
{
    [Route("api/games")]
    [ApiController]
    [Authorize(Roles = UserRoles.User)]
    public class GameController : ControllerBase
    {
        private readonly CurrentUserService _currentUser;
        private readonly GameRepository _gameRepo;
        private readonly GameHelper _gameHelper;
        private readonly ConditionCheckService _conditionCheck;
        public GameController(CurrentUserService currentUser, GameRepository gameRepo, ConditionCheckService conditionCheck, GameHelper gameHelper) : base()
        {
            _currentUser = currentUser;
            _gameRepo = gameRepo;
            _conditionCheck = conditionCheck;
            _gameHelper = gameHelper;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ObjectCreatedResponse), 200)]
        public async Task<IActionResult> CreateGame([FromQuery] long? invitedPlayerId)
        {
            if (invitedPlayerId is not null && await _currentUser.PlayerId() == invitedPlayerId)
                throw new NotFoundException("You cannot invite yourself");
            return Ok(new ObjectCreatedResponse(await _gameRepo.Create(invitedPlayerId)));
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<GameResponse>), 200)]
        public async Task<IActionResult> SearchGame([FromQuery] long? playerId, [FromQuery] short? status)
        {
            var games = (IEnumerable<Game>) await _gameRepo.GetAll();

            if (playerId.HasValue)
                games = games.Where(x => x.Players.Any(p => p.PlayerId == playerId));
            if (status.HasValue)
                games = games.Where(x => x.GameStatus == (GameStatus) status);

            return Ok(games.OrderByDescending(x => x.LastModifiedDate).Select(x => new GameResponse()
                {
                    Id = x.Id,
                    StatusId = (short)x.GameStatus,
                    Status = x.GameStatus,
                    MovesNumber = x.Moves.Count,
                    //MovesNumber = (x.Moves ?? new List<Move>()).Count,
                    PlayersNumber = x.Players.Count
                    //PlayersNumber = (x.Players ?? new List<GamePlayer>()).DistinctBy(p=>p.PlayerId).Count()
                }).ToList()
            );
        }

        [HttpGet("{gameId:long}")]
        [ProducesResponseType(typeof(GameDetailsResponse), 200)]
        public async Task<IActionResult> GetGame([FromRoute] long gameId)
        {
            var game = await _gameRepo.GetFull(gameId);
            if (game is null)
                throw new NotFoundException("Game not found");

            return Ok(new GameDetailsResponse()
            {
                Id = game.Id,
                StatusId = (short) game.GameStatus,
                Status = game.GameStatus,
                Players = game.Players.Select(p => new GameDetailsResponse.Player()
                {
                    Id = p.Id,
                    PlayerId = p.PlayerId,
                    Username = p.Player?.Name??"",
                    RoleId = (short) p.Roles,
                    Role = p.Roles
                }).ToList(),
                Moves = game.Moves.Select(x => new GameDetailsResponse.Move() 
                {
                    PlayerId = x.PlayerId,
                    X = x.X,
                    Y = x.Y,
                    Time = x.CreatedDate.DateTime
                }).ToList()
            });
        }

        //join to the game ?
        //only change status to confirmed, and maybe add yourself to players
        [HttpPatch("{gameId:long}/join")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> JoinGame([FromRoute] long gameId)
        {
            var game = await _gameRepo.Get(gameId);
            if (game is null || game.GameStatus != GameStatus.New)
                throw new NotFoundException("Game not found");

            var playerId = await _currentUser.PlayerId();
            if (await _conditionCheck.PlayerIsInGame(playerId, gameId))
            {
                //youre invited, just confirm
                game.GameStatus = GameStatus.Confirmed;
                if (0 < await _gameRepo.Update())
                    return Ok();
                else
                    throw new NotFoundException("There was an error while joining");
            }
            else if (await _conditionCheck.FreeSpaceForPlayerInGame(gameId))
            {
                //there is free place, join, and confirm
                await _gameRepo.AddGamePlayer(gameId, playerId);
                game.GameStatus = GameStatus.Confirmed;
                if (0 < await _gameRepo.Update())
                    return Ok();
                else
                    throw new NotFoundException("There was an error while joining");
            }
            else 
                throw new NotFoundException("You're not in game");

        }

        [HttpPatch("{gameId:long}/end")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CloseGame([FromRoute] long gameId)
        {
            var game = await _gameRepo.Get(gameId);
            if (game is null)
                throw new NotFoundException("Game not found");

            var playerId = await _currentUser.PlayerId();
            if (!await _conditionCheck.PlayerIsInGame(playerId, gameId))
                throw new NotFoundException("You don't have access to this game");

            if(game.GameStatus == GameStatus.Ended)
                throw new NotFoundException("This game is already closed");

            var winnerId = await _gameHelper.CheckWinning(game.Id);
            game.GameStatus = GameStatus.Ended;
            if (winnerId is null)
            {
                var playerClosing = game.Players?.FirstOrDefault(x => x.PlayerId == playerId);
                if (playerClosing is not null)
                    playerClosing.Roles |= PlayerRole.Close;
            } 
            else
            {
                var playerWinning = game.Players?.FirstOrDefault(x => x.PlayerId == winnerId);
                if (playerWinning is not null)
                    playerWinning.Roles |= PlayerRole.Win;
            }

            await _gameRepo.Update();
            return Ok();
        }

    }
}
