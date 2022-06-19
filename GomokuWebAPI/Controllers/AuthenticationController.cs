using GomokuWebAPI.Authentication;
using GomokuWebAPI.Model.Entities;
using GomokuWebAPI.Repositories;
using GomokuWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GomokuWebAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole<long>> roleManager;
        private readonly IConfiguration configuration;
        private readonly PlayerRepository playerRepository;
        private readonly CurrentUserService currentUser;

        public AuthenticationController(UserManager<AppUser> userManager, RoleManager<IdentityRole<long>> roleManager, IConfiguration configuration, PlayerRepository playerRepo, CurrentUserService currentUser)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
            this.playerRepository = playerRepo;
            this.currentUser = currentUser;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);
            if (user == null)
                return Unauthorized(new CustomResponse { Status = "Error", Message = "User not found" });
            if (!await userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new CustomResponse { Status = "Error", Message = "Bad password" });


            var roles = await userManager.GetRolesAsync(user);
            var claims = await userManager.GetClaimsAsync(user);

            var jwtClaims = new List<Claim> {
                    new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new(JwtRegisteredClaimNames.UniqueName, user.Id.ToString()),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            jwtClaims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            jwtClaims.AddRange(claims.Select(c => new Claim(c.Type, c.Value)));

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:ValidIssuer"],
                audience: configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(12),
                claims: jwtClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return Ok(new TokenResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                Expires = token.ValidTo,
                UserId = user.Id,
                Roles = (List<string>) roles ?? new List<string>()
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CustomResponse), 200)]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return BadRequest(new CustomResponse { Status = "Error", Message = "User already exists!" });
            AppUser user = new AppUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(
                    new CustomResponse { Status = "Error", Message = "User creation failed! Please check user details and try again. "+result.ToString() });

            await playerRepository.Create(user);

            if (!await roleManager.RoleExistsAsync(UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole<long>(UserRoles.User));

            if (await roleManager.RoleExistsAsync(UserRoles.User))
            {
                await userManager.AddToRoleAsync(user, UserRoles.User);
            }

            return Ok(new CustomResponse { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPost("register-admin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CustomResponse), 200)]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return BadRequest(new CustomResponse { Status = "Error", Message = "User already exists!" });

            AppUser user = new AppUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(
                    new CustomResponse { Status = "Error", Message = "User creation failed! Please check user details and try again." + result.ToString() });

            await playerRepository.Create(user);

            if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole<long>(UserRoles.Admin));
            if (!await roleManager.RoleExistsAsync(UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole<long>(UserRoles.User));

            await userManager.AddToRoleAsync(user, UserRoles.Admin);
            await userManager.AddToRoleAsync(user, UserRoles.User);

            return Ok(new CustomResponse { Status = "Success", Message = "User created successfully!" });
        }


        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(AccountResponse), 200)]
        public async Task<IActionResult> GetAccountDetails()
        {
            var user = await currentUser.GetUser();
            var player = await playerRepository.Get((long)user.PlayerId) ?? new Player();
            var roles = await userManager.GetRolesAsync(user);
            return Ok( new AccountResponse(){ 
                UserId = user.Id,
                PlayerId = player.Id,
                Email = user.Email,
                Username = user.UserName,
                Roles = (List<string>)roles,
                Symbol = player.Symbol
            });
        }

    }
}
