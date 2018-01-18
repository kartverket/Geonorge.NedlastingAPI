using System.Net.Http;
using FluentAssertions;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services.Auth;
using Moq;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Services
{
    public class AuthenticationServiceTest
    {
        [Fact]
        public void ShouldFetchAuthenticatedUserFromBasicAuthIfNoSamlAuthIsPresent()
        {
            var exampleUser = new AuthenticatedUser("exampleUser", AuthenticationMethod.Baat);

            var samlAuthMock = new Mock<IBaatAuthenticationService>();
            var basicAuthMock = new Mock<IBasicAuthenticationService>();
            var httpRequestMessage = new HttpRequestMessage();
            basicAuthMock.Setup(b => b.GetAuthenticatedUsername(httpRequestMessage)).Returns(exampleUser);

            var authenticatedUser =
                new AuthenticationService(samlAuthMock.Object, basicAuthMock.Object).GetAuthenticatedUser(
                    httpRequestMessage);

            authenticatedUser.Should().Be(exampleUser);
        }


        [Fact]
        public void ShouldFetchAuthenticatedUserFromSamlIfPresent()
        {
            var exampleUser = new AuthenticatedUser("exampleUser", AuthenticationMethod.Baat);
            var samlAuthMock = new Mock<IBaatAuthenticationService>();
            samlAuthMock.Setup(s => s.GetAuthenticatedUser()).Returns(exampleUser);
            var basicAuthMock = new Mock<IBasicAuthenticationService>();

            var authenticatedUser =
                new AuthenticationService(samlAuthMock.Object, basicAuthMock.Object).GetAuthenticatedUser(
                    new HttpRequestMessage());

            authenticatedUser.Should().Be(exampleUser);
        }
    }
}