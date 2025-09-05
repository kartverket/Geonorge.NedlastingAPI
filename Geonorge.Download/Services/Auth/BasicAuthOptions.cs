using Microsoft.AspNetCore.Authentication;

namespace Geonorge.Download.Services.Auth
{
    public sealed class BasicAuthOptions : AuthenticationSchemeOptions
    {
        public List<BasicAuthUser> Users { get; set; } = [];
    }

    public sealed class BasicAuthUser
    {
        public string Username { get; set; } = "";
        public string? Password { get; set; }
        public string[] Roles { get; set; } = [];
    }

}
