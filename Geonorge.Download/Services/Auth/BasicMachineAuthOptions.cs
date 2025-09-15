using Microsoft.AspNetCore.Authentication;

namespace Geonorge.Download.Services.Auth
{
    public sealed class BasicMachineAuthOptions : AuthenticationSchemeOptions
    {
        public string Realm { get; set; } = "App";
    }
}
