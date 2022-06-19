using GomokuWebAPI.Authentication;
using GomokuWebAPI.Common;
using GomokuWebAPI.DTO;
using GomokuWebAPI.Model;
using GomokuWebAPI.Model.Entities;
using GomokuWebAPI.Repositories;
using GomokuWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GomokuWebAPI.Controllers
{
    [Route("api/players")]
    [ApiController]
    [Authorize(Roles = UserRoles.User)]
    public class PlayerController : ControllerBase
    {
        private readonly PlayerRepository _playerRepo;
        private readonly CurrentUserService _currentUser;
        public PlayerController(CurrentUserService currentUser, PlayerRepository playerRepository) : base()
        {
            _currentUser = currentUser;
            _playerRepo = playerRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PlayerResponse>), 200)]
        public async Task<IActionResult> SearchPlayers([FromQuery] string? name)
        {
            var players = await _playerRepo.GetAll();
            if(!string.IsNullOrEmpty(name))
                players = players.Where(x => x.Name.ToLower().Contains(name.ToLower())).ToList();
            return Ok(players.Select(x=>new PlayerResponse() { Id=x.Id, Symbol=x.Symbol, Name=x.Name}).ToList());
        }
    }
}
