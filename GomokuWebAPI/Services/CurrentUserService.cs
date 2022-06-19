using GomokuWebAPI.Authentication;
using GomokuWebAPI.Common;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GomokuWebAPI.Services
{
    public class CurrentUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<AppUser> _userManager;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager)
        {
            _contextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        public async Task<AppUser> GetUser()
        {
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext == null) throw new NotFoundException("http context is null");
            return await _userManager.GetUserAsync(httpContext.User);
        }
        public async Task<long> UserId()
        {
            return (await GetUser()).Id;
        }
        public async Task<long> PlayerId()
        {
            var user = await GetUser();
            return user.PlayerId is null ? -1 : (long)user.PlayerId;
        }
    }
}
