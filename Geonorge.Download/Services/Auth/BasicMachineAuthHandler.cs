using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Geonorge.Download.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Geonorge.Download.Services.Auth
{
    public sealed class BasicMachineAuthHandler(
            IOptionsMonitor<BasicMachineAuthOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IBasicAuthenticationCredentialValidator credentialValidator,
            DownloadContext dbContext)
            : AuthenticationHandler<BasicMachineAuthOptions>(options, logger, encoder, clock)
    {
        public const string SchemeName = "BasicMachine";

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{Options.Realm}\", charset=\"UTF-8\"";
            return base.HandleChallengeAsync(properties);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check for Authorization header
            if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
                return AuthenticateResult.NoResult();

            var header = authHeaderValues.ToString();
            if (!header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.NoResult();

            var parameter = header.Substring("Basic ".Length).Trim();
            var creds = TryParseBasicCredentials(parameter);
            if (creds is null)
                return AuthenticateResult.Fail("Invalid Basic credentials header.");

            // Validate
            var isValid = credentialValidator.ValidCredentials(creds);
            if (!isValid)
                return AuthenticateResult.Fail("Invalid username or password.");

            var roles = await GetRolesForUserAsync(creds.Username);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, creds.Username),
            new Claim(ClaimTypes.Name, creds.Username),
            new Claim("auth_scheme", Scheme.Name)
        };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private async Task<List<string>> GetRolesForUserAsync(string userName)
        {
            var account = await dbContext.MachineAccounts.FindAsync(userName);
            if (account?.Roles is string roles && !string.IsNullOrWhiteSpace(roles))
            {
                return roles
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
            }
            return new List<string>();
        }

        private static Credentials? TryParseBasicCredentials(string authorizationParameter)
        {
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(authorizationParameter);
            }
            catch (FormatException)
            {
                return null;
            }

            string decoded;
            if (!TryGetString(bytes, Encoding.UTF8, out decoded) &&
                !TryGetString(bytes, Encoding.Latin1, out decoded))
            {
                return null;
            }

            if (string.IsNullOrEmpty(decoded)) return null;
            var idx = decoded.IndexOf(':');
            if (idx < 0) return null;

            return new Credentials
            {
                Username = decoded[..idx],
                Password = decoded[(idx + 1)..]
            };
        }

        private static bool TryGetString(byte[] data, Encoding encoding, out string result)
        {
            var safe = (Encoding)encoding.Clone();
            safe.DecoderFallback = DecoderFallback.ExceptionFallback;
            try
            {
                result = safe.GetString(data);
                return true;
            }
            catch (DecoderFallbackException)
            {
                result = string.Empty;
                return false;
            }
        }
    }

}
