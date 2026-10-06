using Geonorge.Download.Models;
using Geonorge.Download.Services.Auth;
using System.Security.Claims;

namespace Geonorge.Download.Tests.Models
{
    public class ClaimsPrincipalExtensionTest
    {
        [Fact]
        public void UsernameForStorageUsesNameClaim()
        {
            var principal = TestPrincipals.WithClaims(new Claim(ClaimTypes.Name, "testuser"));

            Assert.Equal("testuser", principal.UsernameForStorage());
        }

        [Fact]
        public void UsernameForStorageUsesPreferredUsernameClaim()
        {
            var principal = TestPrincipals.WithClaims(new Claim("preferred_username", "testuser"));

            Assert.Equal("testuser", principal.UsernameForStorage());
        }

        [Fact]
        public void UsernameForStoragePrefixesMachineAccounts()
        {
            var principal = TestPrincipals.WithClaims(
                new Claim(ClaimTypes.Name, "machine"),
                new Claim("auth_scheme", BasicMachineAuthHandler.SchemeName));

            Assert.Equal("local_machine", principal.UsernameForStorage());
        }

        [Fact]
        public void UsernameForStorageIsNullWithoutNameClaim()
        {
            Assert.Null(TestPrincipals.Anonymous().UsernameForStorage());
            Assert.Null(((ClaimsPrincipal)null!).UsernameForStorage());
        }

        [Theory]
        [InlineData("301", "0301")]
        [InlineData("4601", "4601")]
        public void MunicipalityCodeIsPaddedToFourDigits(string claimValue, string expected)
        {
            var principal = TestPrincipals.WithClaims(new Claim("MunicipalityCode", claimValue));

            Assert.Equal(expected, principal.MunicipalityCode());
        }

        [Fact]
        public void MunicipalityCodeIsNullWhenMissingOrBlank()
        {
            Assert.Null(TestPrincipals.Anonymous().MunicipalityCode());
            Assert.Null(TestPrincipals.WithClaims(new Claim("MunicipalityCode", " ")).MunicipalityCode());
        }
    }
}
