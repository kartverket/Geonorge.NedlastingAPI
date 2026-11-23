using Geonorge.Download.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;

namespace Geonorge.Download.Tests.Services
{
    public class OidcNonceCookieCleanupTest
    {
        private static readonly AuthenticationScheme Scheme =
            new(OpenIdConnectDefaults.AuthenticationScheme, null, typeof(OpenIdConnectHandler));

        [Theory]
        [InlineData("navigate", null, true)]
        [InlineData("cors", null, false)]
        [InlineData("cors", "text/html; blazor-enhanced-nav=on", true)]
        [InlineData(null, "application/json", false)]
        [InlineData(null, "text/html,application/xhtml+xml", true)]
        [InlineData(null, null, true)]
        public void DetectsBrowserNavigation(string? secFetchMode, string? accept, bool expected)
        {
            var ctx = new DefaultHttpContext();
            if (secFetchMode != null) ctx.Request.Headers["Sec-Fetch-Mode"] = secFetchMode;
            if (accept != null) ctx.Request.Headers.Accept = accept;

            Assert.Equal(expected, OidcNonceCookieCleanup.IsBrowserNavigation(ctx.Request));
        }

        [Fact]
        public void XhrIsNotNavigation()
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.XRequestedWith = "XMLHttpRequest";
            ctx.Request.Headers.Accept = "text/html";

            Assert.False(OidcNonceCookieCleanup.IsBrowserNavigation(ctx.Request));
        }

        [Fact]
        public void FetchChallengeGets401AndNoNonceOrCorrelationCookies()
        {
            var options = new OpenIdConnectOptions();
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers["Sec-Fetch-Mode"] = "cors";
            ctx.Response.Cookies.Append(options.NonceCookie.Name + "abc", "N");
            ctx.Response.Cookies.Append(options.CorrelationCookie.Name + "def", "N");
            ctx.Response.Cookies.Append("other", "1");

            var redirect = new RedirectContext(ctx, Scheme, options, new AuthenticationProperties());

            Assert.True(OidcNonceCookieCleanup.SuppressChallengeForNonNavigation(redirect));
            Assert.Equal(StatusCodes.Status401Unauthorized, ctx.Response.StatusCode);
            var setCookie = Assert.Single(ctx.Response.Headers.SetCookie);
            Assert.StartsWith("other=", setCookie);
        }

        [Fact]
        public void NavigationChallengeIsLeftAlone()
        {
            var options = new OpenIdConnectOptions();
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers["Sec-Fetch-Mode"] = "navigate";
            ctx.Response.Cookies.Append(options.NonceCookie.Name + "abc", "N");

            var redirect = new RedirectContext(ctx, Scheme, options, new AuthenticationProperties());

            Assert.False(OidcNonceCookieCleanup.SuppressChallengeForNonNavigation(redirect));
            Assert.Single(ctx.Response.Headers.SetCookie);
        }

        [Theory]
        [InlineData(3, 0)]
        [InlineData(4, 4)]
        public void RemovesNonceCookiesWhenTooMany(int existing, int expectedDeleted)
        {
            var options = new OpenIdConnectOptions();
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.Cookie = string.Join("; ",
                Enumerable.Range(0, existing).Select(i => $"{options.NonceCookie.Name}{i}=N").Append("other=1"));

            OidcNonceCookieCleanup.RemoveStaleCookies(
                new MessageReceivedContext(ctx, Scheme, options, new AuthenticationProperties()));

            var deleted = ctx.Response.Headers.SetCookie.Where(c => c!.StartsWith(options.NonceCookie.Name!)).ToList();
            Assert.Equal(expectedDeleted, deleted.Count);
            Assert.All(deleted, c => Assert.Contains("expires=Thu, 01 Jan 1970", c));
            Assert.All(deleted, c => Assert.Contains("path=/signin-oidc", c));
        }
    }
}
