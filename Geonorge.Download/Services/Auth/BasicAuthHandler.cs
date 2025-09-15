using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Geonorge.Download.Services.Auth
{
    public sealed class BasicAuthHandler(IOptionsMonitor<BasicAuthOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ISystemClock clock)
        : AuthenticationHandler<BasicAuthOptions>(options, loggerFactory, encoder, clock)
    {

        public const string SchemeName = "Basic";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check for Authorization header
            if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
                return Task.FromResult(AuthenticateResult.NoResult());

            var authHeader = authHeaderValues.ToString();
            if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(AuthenticateResult.NoResult());

            // Decode "Basic base64(user:pass)"
            string userPass;
            try
            {
                var base64 = authHeader.Substring("Basic ".Length).Trim();
                var bytes = Convert.FromBase64String(base64);
                userPass = Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Basic header"));
            }

            var idx = userPass.IndexOf(':');
            if (idx <= 0)
                return Task.FromResult(AuthenticateResult.Fail("Invalid Basic credential format"));

            var username = userPass.Substring(0, idx);
            var password = userPass[(idx + 1)..];

            // Look up user from configuration/appsettings.json
            var user = Options.Users
                .FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.Ordinal));

            if (user is null || user.Password is null)
                return Task.FromResult(AuthenticateResult.Fail("Invalid username or password"));

            if (!FixedTimeEquals(password, user.Password))
                return Task.FromResult(AuthenticateResult.Fail("Invalid username or password"));

            // Build claims principal with configured roles
            var claims = new List<Claim> { new(ClaimTypes.Name, user.Username) };
            foreach (var r in user.Roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            var ba = Encoding.UTF8.GetBytes(a);
            var bb = Encoding.UTF8.GetBytes(b);
            if (ba.Length != bb.Length)
                return false;
            return CryptographicOperations.FixedTimeEquals(ba, bb);
        }
    }
}
