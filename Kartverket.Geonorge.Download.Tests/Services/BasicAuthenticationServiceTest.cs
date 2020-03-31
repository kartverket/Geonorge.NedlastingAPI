using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services.Auth;
using Moq;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Services
{
    public class BasicAuthenticationServiceTest
    {
        [Fact]
        public void ShouldValidateCredentialsComingFromRequest()
        {
            var httpRequestMessage = new HttpRequestMessage();
            var parameter = "YWRtaW46YWRtaW4="; // admin:admin
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", parameter);

            var credentialValidatorMock = new Mock<IBasicAuthenticationCredentialValidator>();
            credentialValidatorMock.Setup(c => c.ValidCredentials(It.IsAny<Credentials>())).Returns(true);
            AuthenticatedUser authenticatedUser = new BasicAuthenticationService(credentialValidatorMock.Object, new DownloadContext()).GetAuthenticatedUsername(httpRequestMessage);
            authenticatedUser.Username.Should().Be("admin");
        }

        [Fact]
        public void ShouldReturnNullWhenUserIsNotAuthenticated()
        {
            AuthenticatedUser authenticatedUser = new BasicAuthenticationService(null, null).GetAuthenticatedUsername(new HttpRequestMessage());
            authenticatedUser.Should().BeNull();
        }
    }
}