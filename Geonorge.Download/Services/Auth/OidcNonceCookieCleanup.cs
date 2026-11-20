using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Geonorge.Download.Services.Auth
{
    /// <summary>
    /// Prevents OpenID Connect nonce and correlation cookies from piling up ("Request cookie too large").
    ///
    /// The OIDC handler writes a new nonce cookie (lifetime 1 hour) and correlation cookie for every redirect
    /// to the identity provider, and only removes them when the matching callback succeeds. Challenges that
    /// never complete (fetch/XHR calls following a redirect to /account/login, aborted logins) leave cookies behind.
    ///
    /// ASP.NET Core counterpart of NonceCleanupCookieManager in Geonorge.AuthLib.NetFull.
    /// </summary>
    public static class OidcNonceCookieCleanup
    {
        /// <summary>
        /// Number of existing nonce/correlation cookies tolerated before they are all removed. A few are allowed
        /// so that parallel logins in several tabs still work.
        /// </summary>
        public const int MaxNonceCookies = 3;

        /// <summary>
        /// Use in OnRedirectToIdentityProvider. Requests that are not browser navigations cannot complete a login:
        /// answer 401 instead of redirecting and drop the nonce/correlation cookies already written by the handler.
        /// Returns true if the response was handled.
        /// </summary>
        public static bool SuppressChallengeForNonNavigation(RedirectContext context)
        {
            if (IsBrowserNavigation(context.Request))
                return false;

            var response = context.Response;
            var noncePrefix = context.Options.NonceCookie.Name;
            var correlationPrefix = context.Options.CorrelationCookie.Name;

            var kept = response.Headers.SetCookie
                .Where(h => h is not null
                            && !(noncePrefix is not null && h.StartsWith(noncePrefix, StringComparison.Ordinal))
                            && !(correlationPrefix is not null && h.StartsWith(correlationPrefix, StringComparison.Ordinal)))
                .ToArray();
            response.Headers.SetCookie = new StringValues(kept);

            response.StatusCode = StatusCodes.Status401Unauthorized;
            context.HandleResponse();
            return true;
        }

        /// <summary>
        /// Use in OnMessageReceived (runs on the callback path, where the path-scoped nonce/correlation cookies are sent).
        /// Removes them when too many have accumulated. The current callback still validates, since the request
        /// cookies have already been received.
        /// </summary>
        public static void RemoveStaleCookies(MessageReceivedContext context)
        {
            var options = context.Options;
            RemoveIfTooMany(context.HttpContext, options.NonceCookie.Name, options.NonceCookie.Build(context.HttpContext));
            RemoveIfTooMany(context.HttpContext, options.CorrelationCookie.Name, options.CorrelationCookie.Build(context.HttpContext));
        }

        private static void RemoveIfTooMany(HttpContext httpContext, string? prefix, CookieOptions cookieOptions)
        {
            if (string.IsNullOrEmpty(prefix))
                return;

            var keys = httpContext.Request.Cookies.Keys
                .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
                .ToList();

            if (keys.Count <= MaxNonceCookies)
                return;

            foreach (var key in keys)
                httpContext.Response.Cookies.Delete(key, cookieOptions);
        }

        /// <summary>
        /// True if the request is a browser navigation, i.e. a request where redirecting to the identity
        /// provider can actually complete a login. Blazor enhanced navigation (fetch) counts as navigation,
        /// since Blazor retries it as a full page load when redirected to another origin.
        /// </summary>
        public static bool IsBrowserNavigation(HttpRequest request)
        {
            var accept = request.Headers.Accept.ToString();

            if (request.Headers.ContainsKey("blazor-enhanced-nav")
                || accept.Contains("blazor-enhanced-nav", StringComparison.OrdinalIgnoreCase))
                return true;

            var secFetchMode = request.Headers["Sec-Fetch-Mode"].ToString();
            if (!string.IsNullOrEmpty(secFetchMode))
                return string.Equals(secFetchMode, "navigate", StringComparison.OrdinalIgnoreCase);

            if (string.Equals(request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
                return false;

            return string.IsNullOrEmpty(accept) || accept.Contains("text/html", StringComparison.OrdinalIgnoreCase);
        }
    }
}
