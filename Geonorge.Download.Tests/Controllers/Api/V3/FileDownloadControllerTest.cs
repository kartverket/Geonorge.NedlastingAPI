using Geonorge.Download.Controllers.Api.V3;
using Geonorge.Download.Models;
using Geonorge.Download.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Security.Claims;

namespace Geonorge.Download.Tests.Controllers.Api.V3
{
    public class FileDownloadControllerTest
    {
        private const string DatasetUuid = "7a0c6a5c-3e0e-4b8a-9a43-1d3f4b0c2a11";
        private const string FileUuid = "c1d2e3f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f";
        private const string FileUrl = "https://example.com/files/test.zip";

        private readonly IFileService _fileService = Substitute.For<IFileService>();
        private readonly IDownloadService _downloadService = Substitute.For<IDownloadService>();

        [Fact]
        public async Task ShouldReturnBadRequestWhenUuidIsInvalid()
        {
            var result = await CreateController(TestPrincipals.Anonymous()).GetFile("not-a-uuid", FileUuid);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ShouldReturnNotFoundWhenDatasetDoesNotExist()
        {
            var result = await CreateController(TestPrincipals.Anonymous()).GetFile(DatasetUuid, FileUuid);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ShouldReturnNotFoundWhenFileDoesNotExist()
        {
            _fileService.GetDatasetAsync(DatasetUuid).Returns(CreateDataset(restricted: false));

            var result = await CreateController(TestPrincipals.Anonymous()).GetFile(DatasetUuid, FileUuid);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ShouldStreamFileWhenDatasetIsOpenAndUserIsNotLoggedIn()
        {
            SetupDatasetAndFile(restricted: false);
            var controller = CreateController(TestPrincipals.Anonymous());

            var result = await controller.GetFile(DatasetUuid, FileUuid);

            Assert.IsType<EmptyResult>(result);
            await _downloadService.Received(1).StreamRemoteFileToResponseAsync(controller.HttpContext, FileUrl);
        }

        [Fact]
        public async Task ShouldReturnForbiddenWhenUserIsNotLoggedInAndDatasetIsRestricted()
        {
            SetupDatasetAndFile(restricted: true);

            var result = await CreateController(TestPrincipals.Anonymous()).GetFile(DatasetUuid, FileUuid);

            Assert.IsType<ForbidResult>(result);
            await AssertFileNotStreamed();
        }

        [Fact]
        public async Task ShouldRedirectBrowserToLoginWhenUserIsNotLoggedInAndDatasetIsRestricted()
        {
            SetupDatasetAndFile(restricted: true);

            var result = await CreateController(TestPrincipals.Anonymous(), accept: "text/html").GetFile(DatasetUuid, FileUuid);

            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.StartsWith("https://download.example.com/account/login?ReturnUrl=", redirect.Url);
            await AssertFileNotStreamed();
        }

        [Fact]
        public async Task ShouldReturnForbiddenWhenUserIsLoggedInWithoutAccessToRestrictedDataset()
        {
            SetupDatasetAndFile(restricted: true);
            _fileService.HasAccess(Arg.Any<Download.Models.File>(), Arg.Any<ClaimsPrincipal>()).Returns(false);

            var result = await CreateController(TestPrincipals.User("testuser")).GetFile(DatasetUuid, FileUuid);

            Assert.IsType<ForbidResult>(result);
            await AssertFileNotStreamed();
        }

        [Fact]
        public async Task ShouldStreamFileWhenUserIsLoggedInWithAccessToRestrictedDataset()
        {
            SetupDatasetAndFile(restricted: true);
            _fileService.HasAccess(Arg.Any<Download.Models.File>(), Arg.Any<ClaimsPrincipal>()).Returns(true);
            var controller = CreateController(TestPrincipals.User("testuser"));

            var result = await controller.GetFile(DatasetUuid, FileUuid);

            Assert.IsType<EmptyResult>(result);
            await _downloadService.Received(1).StreamRemoteFileToResponseAsync(controller.HttpContext, FileUrl);
        }

        private void SetupDatasetAndFile(bool restricted)
        {
            var dataset = CreateDataset(restricted);
            _fileService.GetDatasetAsync(DatasetUuid).Returns(dataset);
            _fileService.GetFileAsync(FileUuid, DatasetUuid).Returns(new Download.Models.File
            {
                Filename = "test.zip",
                Url = FileUrl,
                Dataset = dataset
            });
        }

        private static Dataset CreateDataset(bool restricted) => new()
        {
            Title = "Test dataset",
            MetadataUuid = DatasetUuid,
            AccessConstraint = restricted ? AccessConstraint.NorgeDigitalRestricted : null
        };

        private Task AssertFileNotStreamed() =>
            _downloadService.DidNotReceive().StreamRemoteFileToResponseAsync(Arg.Any<HttpContext>(), Arg.Any<string>());

        private FileDownloadController CreateController(ClaimsPrincipal user, string? accept = null)
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["DownloadUrl"] = "https://download.example.com/" })
                .Build();

            var httpContext = new DefaultHttpContext { User = user };
            if (accept != null)
                httpContext.Request.Headers.Accept = accept;

            return new FileDownloadController(NullLogger<FileDownloadController>.Instance, config, _fileService, _downloadService)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };
        }
    }
}
