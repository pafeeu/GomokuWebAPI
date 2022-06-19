using GomokuWebAPI.Authentication;
using GomokuWebAPI.Common;
using GomokuWebAPI.DTO;
using GomokuWebAPI.Model.Enums;
using GomokuWebAPI.Repositories;
using GomokuWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GomokuWebAPI.Controllers
{
    [Route("api/moves/{gameId:long}")]
    [ApiController]
    [Authorize(Roles = UserRoles.User)]
    public class MoveController : ControllerBase
    {
        private readonly CurrentUserService _currentUser;
        private readonly GameRepository _gameRepo;
        private readonly MoveRepository _moveRepo;
        private readonly GameHelper _gameHelper;
        private readonly ConditionCheckService _conditionCheckService;
        public MoveController(CurrentUserService currentUser, GameRepository gameRepo, MoveRepository moveRepo, GameHelper gameHelper, ConditionCheckService conditionCheck) : base()
        {
            _currentUser = currentUser;
            _gameRepo = gameRepo;
            _moveRepo = moveRepo;
            _gameHelper = gameHelper;
            _conditionCheckService = conditionCheck;
        }

        [HttpGet("last")]
        [ProducesResponseType(typeof(GameDetailsResponse.Move), 200)]
        public async Task<IActionResult> GetLastMove([FromRoute] long gameId)
        {
            var move = await _moveRepo.GetLast(gameId);
            if (move is null)
                throw new NotFoundException("Moves not found");

            return Ok(new GameDetailsResponse.Move()
            {
                PlayerId = move.PlayerId,
                X = move.X,
                Y = move.Y,
                Time = move.CreatedDate.DateTime
            });
        }

        [HttpPost("make-at")]
        [ProducesResponseType(typeof(ObjectCreatedResponse), 200)]
        public async Task<IActionResult> MakeMove([FromRoute] long gameId, [FromQuery] short x, [FromQuery] short y)
        {
            var game = await _gameRepo.Get(gameId);
            var playerId = await _currentUser.PlayerId();

            if (game is null || game.GameStatus==GameStatus.Ended)
                throw new NotFoundException("Game not found");

            if(!await _conditionCheckService.PlayerIsInGame(playerId, game.Id))
                throw new NotFoundException("Player isn't in game");

            if (!_gameHelper.IsValidMove(x, y) || !await _gameHelper.IsFreeSlot(game.Id, x, y))
                throw new NotFoundException("Bad move");

            if(await _gameHelper.WhoseMove(game.Id) != playerId)
                throw new NotFoundException("It's not your turn");

            //OK
            if (game.GameStatus <= GameStatus.Confirmed)
                game.GameStatus = GameStatus.InProgress;
            await _gameRepo.Update();

            return Ok(new ObjectCreatedResponse(
                await _moveRepo.Create(new Model.Entities.Move() { 
                    GameId = game.Id,
                    PlayerId = playerId,
                    X = x,
                    Y = y
                })));
        }

    }
}
