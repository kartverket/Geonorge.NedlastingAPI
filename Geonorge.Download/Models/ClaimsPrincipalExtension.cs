using Geonorge.Download.Services.Auth;
using System.Security.Claims;

namespace Geonorge.Download.Models
{
    public static class ClaimsPrincipalExtension
    {
        public static string? UsernameForStorage(this ClaimsPrincipal principal)
        {
            if (principal == null)
                return null;
            var nameClaim = principal.Claims.FirstOrDefault(c => (c.Type == ClaimTypes.Name || c.Type == "preferred_username"));
            var schemeClaim = principal.Claims.FirstOrDefault(c => c.Type == "auth_scheme");
            if (nameClaim == null)
                return null;
            if (schemeClaim != null && schemeClaim.Value == BasicMachineAuthHandler.SchemeName)
                return $"local_{nameClaim?.Value}";
            return nameClaim?.Value;
        }

        public static string? MunicipalityCode(this ClaimsPrincipal principal)
        {
            if (principal == null)
                return null;
            var municipalityClaim = principal.Claims.FirstOrDefault(c => c.Type == "MunicipalityCode");
            if (municipalityClaim == null)
                return null;
            if (!string.IsNullOrWhiteSpace(municipalityClaim.Value))
                return municipalityClaim.Value.PadLeft(4, '0');
            return null;
        }
    }
}
