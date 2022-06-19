﻿namespace GomokuWebAPI.Authentication
{
    public class TokenResponse
    {
        public string AccessToken { get; init; }
        public DateTime Expires { get; init; }
        public long UserId { get; init; }
        public List<string> Roles { get; init; }
    }
}
