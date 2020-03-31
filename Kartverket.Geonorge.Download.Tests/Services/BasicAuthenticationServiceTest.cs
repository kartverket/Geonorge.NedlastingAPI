using System.Collections.Generic;
using System.Data.Entity;
using System.Net.Http;
using System.Net.Http.Headers;
using EntityFramework.MoqHelper;
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

            Mock<DownloadContext> mockDbContext = EntityFrameworkMoqHelper
            .CreateMockForDbContext<DownloadContext>();
            Mock<DbSet<MachineAccount>> mockMachineAccounts = EntityFrameworkMoqHelper
            .CreateMockForDbSet<MachineAccount>().SetupForQueryOn(new List<MachineAccount>());
            mockDbContext.Setup(m => m.MachineAccounts).Returns(mockMachineAccounts.Object);

            var credentialValidatorMock = new Mock<IBasicAuthenticationCredentialValidator>();
            credentialValidatorMock.Setup(c => c.ValidCredentials(It.IsAny<Credentials>())).Returns(true);
            AuthenticatedUser authenticatedUser = new BasicAuthenticationService(credentialValidatorMock.Object, mockDbContext.Object).GetAuthenticatedUsername(httpRequestMessage);
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