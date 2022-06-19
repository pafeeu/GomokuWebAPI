using GomokuWebAPI.Authentication;
using GomokuWebAPI.Common;
using GomokuWebAPI.Model.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GomokuWebAPI.Controllers
{
    [Route("api/helper")]
    [ApiController]
    [Authorize(Roles = UserRoles.User)]
    public class HelperController : ControllerBase
    {

        [HttpGet("list-of-game-statuses")]
        [ProducesResponseType(typeof(List<PairStringShort>), 200)]
        public async Task<IActionResult> GetListOfGameStatuses()
        {
            return Ok(typeof(GameStatus).EnumToListPairs());
        }

        [HttpGet("list-of-player-roles")]
        [ProducesResponseType(typeof(List<PairStringShort>), 200)]
        public async Task<IActionResult> GetListOfPlayerRoles()
        {
            return Ok(typeof(PlayerRole).EnumToListPairs());
        }
    }
}
