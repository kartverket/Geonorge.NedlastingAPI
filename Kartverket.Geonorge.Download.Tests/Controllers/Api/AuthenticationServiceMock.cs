using System.Net.Http;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services.Auth;
using Moq;

namespace Kartverket.Geonorge.Download.Tests.Controllers.Api
{
    public class AuthenticationServiceMock
    {
        public static IAuthenticationService GetServiceWithAuthenticatedUser(string username)
        {
            var mock = new Mock<IAuthenticationService>();
            SetupMockToReturnAuthenticatedUser(mock, username);
            return mock.Object;
        }

        public static Mock<IAuthenticationService> SetupMockToReturnAuthenticatedUser(Mock<IAuthenticationService> mock, string username)
        {
            if (!string.IsNullOrEmpty(username))
                mock.Setup(m => m.GetAuthenticatedUser(It.IsAny<HttpRequestMessage>()))
                    .Returns(new AuthenticatedUser(username, AuthenticationMethod.Baat));

            return mock;
        }
    }
}