using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public sealed class ExternalTokenHandler(
        IOptionsMonitor<ExternalTokenOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ISystemClock clock)
        : AuthenticationHandler<ExternalTokenOptions>(options, loggerFactory, encoder, clock)
{
    // ↓↓↓ paste the method here ↓↓↓
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var raw))
        {
            //Logger.LogWarning("No Authorization header");
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!AuthenticationHeaderValue.TryParse(raw, out var hdr))
        {
            //Logger.LogWarning("Malformed Authorization header: {Raw}", raw.ToString());
            return Task.FromResult(AuthenticateResult.Fail("Malformed header"));
        }

        //Logger.LogInformation("Bearer token received: {Token}", hdr.Parameter);

        if (hdr.Scheme != "Bearer" || hdr.Parameter != Options.Token)
            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));

        // success → create a principal (claims optional)
        var identity = new ClaimsIdentity(Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    // optional: add the WWW-Authenticate header when we return 401
    protected override Task HandleChallengeAsync(AuthenticationProperties props)
    {
        Response.Headers.WWWAuthenticate = "Bearer";
        return base.HandleChallengeAsync(props);
    }
}