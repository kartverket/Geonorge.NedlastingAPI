using System.Security.Claims;

namespace Geonorge.Download.Tests
{
    internal static class TestPrincipals
    {
        public static ClaimsPrincipal Anonymous() => new(new ClaimsIdentity());

        public static ClaimsPrincipal User(string username, params string[] roles)
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, username) };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        public static ClaimsPrincipal WithClaims(params Claim[] claims) =>
            new(new ClaimsIdentity(claims, "Test"));
    }
}
