using Microsoft.AspNetCore.Authentication;

public sealed class ExternalTokenOptions : AuthenticationSchemeOptions
{
    public string Token { get; init; } = string.Empty;
}
