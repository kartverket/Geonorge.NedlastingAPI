using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Results;
using FluentAssertions;
using Kartverket.Geonorge.Download.Controllers.Api.V3;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Kartverket.Geonorge.Download.Services.Auth;
using Moq;
using Xunit;
using File = Kartverket.Geonorge.Download.Models.File;

namespace Kartverket.Geonorge.Download.Tests.Controllers.Api.V3
{
    public class FileDownloadControllerTest
    {
        private readonly string _datasetUuid = "f428194c-ade6-461b-836f-5a21b8e8ec27";
        private readonly string _fileUuid = "f54b9e85-4c67-4fa7-9cae-d25258e320c1";
        private readonly string _fileContent = "This is the content of the delivered file.";

        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IAuthenticationService> _authenticationServiceMock;
        private readonly Mock<IDownloadService> _downloadServiceMock;

        public FileDownloadControllerTest()
        {
            _fileServiceMock = new Mock<IFileService>();
            _authenticationServiceMock = new Mock<IAuthenticationService>();
            _downloadServiceMock = new Mock<IDownloadService>();
        }

        [Fact]
        public async Task ShouldReturnBadRequestWhenFileIsNotAnUuid()
        {
            var result = await Controller().GetFile(_datasetUuid, "") as BadRequestErrorMessageResult;
            result.Should().NotBeNull();
            result?.Message.Should().Contain("fileUuid");
        }

        [Fact]
        public async Task ShouldReturnBadRequestWhenOrderUuidIsNotAnUuid()
        {
            var result = await Controller().GetFile("", _fileUuid) as BadRequestErrorMessageResult;
            result.Should().NotBeNull();
            result?.Message.Should().Contain("datasetUuid");
        }

        [Fact]
        public async Task ShouldReturnNotFoundWhenDatasetDoesNotExist()
        {
            var result = await Controller().GetFile(_datasetUuid, _fileUuid) as NotFoundResult;
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldReturnNotFoundWhenFileDoesNotExist()
        {
            _fileServiceMock.Setup(f => f.GetDatasetAsync(_datasetUuid)).ReturnsAsync(new Dataset());
            var result = await Controller().GetFile(_datasetUuid, _fileUuid) as NotFoundResult;
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldReturnForbiddenWhenUserIsNotLoggedInAndDatasetIsRestricted()
        {
            SetupFileServiceMockToDeliverRestrictedDatasetAndFile();

            HttpResponseMessage response = await ExecuteRequest();

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        private async Task<HttpResponseMessage> ExecuteRequest()
        {
            IHttpActionResult result = await Controller().GetFile(_datasetUuid, _fileUuid);
            var response = await result.ExecuteAsync(CancellationToken.None);
            return response;
        }


        [Fact]
        public async Task ShouldReturnFileWhenUserIsLoggedInAndDatasetIsRestricted()
        {
            SetupFileServiceMockToDeliverRestrictedDatasetAndFile();
            SetupDownloadServiceMockToDeliverFileStream();
            AuthenticateUser();

            HttpResponseMessage response = await ExecuteRequest();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldReturnFileWhenUserIsNotLoggedInAndDatasetIsOpen()
        {
            SetupFileServiceMockToDeliverOpenDatasetAndFile();
            SetupDownloadServiceMockToDeliverFileStream();

            HttpResponseMessage response = await ExecuteRequest();
             
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNull();
        }

        private void AuthenticateUser()
        {
            AuthenticationServiceMock.SetupMockToReturnAuthenticatedUser(_authenticationServiceMock, "username");
        }

        private void SetupDownloadServiceMockToDeliverFileStream()
        {
            var stringWriter = new StringWriter();
            stringWriter.WriteLine(_fileContent);
            var httpResponse = new HttpResponse(stringWriter);
            _downloadServiceMock.Setup(d => d.CreateResponseFromRemoteFile(It.IsAny<string>())).Returns(httpResponse);
        }

        private void SetupFileServiceMockToDeliverRestrictedDatasetAndFile()
        {
            _fileServiceMock.Setup(f => f.GetDatasetAsync(_datasetUuid)).ReturnsAsync(CreateRestrictedDataset());
            _fileServiceMock.Setup(f => f.GetFileAsync(_fileUuid, _datasetUuid)).ReturnsAsync(new File());
        }

        private void SetupFileServiceMockToDeliverOpenDatasetAndFile()
        {
            _fileServiceMock.Setup(f => f.GetDatasetAsync(_datasetUuid)).ReturnsAsync(new Dataset());
            _fileServiceMock.Setup(f => f.GetFileAsync(_fileUuid, _datasetUuid)).ReturnsAsync(new File());
        }

        private static Dataset CreateRestrictedDataset()
        {
            return new Dataset()
            {
                AccessConstraint = AccessConstraint.NorgeDigitalRestricted
            };
        }

        private FileDownloadController Controller()
        {
            var controller = new FileDownloadController(_fileServiceMock.Object, _authenticationServiceMock.Object, _downloadServiceMock.Object);
            // these lines are needed to use the Context inside the controller
            controller.Request = new HttpRequestMessage();
            controller.Request.Headers.Add("Accept", "text/plain");
            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            controller.Configuration = new HttpConfiguration();
            return controller;
        }
    }
}