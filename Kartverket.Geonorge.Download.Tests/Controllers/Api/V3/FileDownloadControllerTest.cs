using System.Web.Http.Results;
using FluentAssertions;
using Kartverket.Geonorge.Download.Controllers.Api.V3;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Controllers.Api.V3
{
    public class FileDownloadControllerTest
    {
        private readonly string _datasetUuid = "";
        private readonly string _fileUuid = "";

        [Fact]
        public void ShouldReturnBadRequestWhenFileIsNotAnUuid()
        {
            var downloadController = new FileDownloadController();
            var result = downloadController.GetFile(_datasetUuid, "") as BadRequestErrorMessageResult;
            result.Should().NotBeNull();
            result?.Message.Should().Contain("datasetUuid");
        }

        [Fact]
        public void ShouldReturnBadRequestWhenOrderUuidIsNotAnUuid()
        {
            var downloadController = new FileDownloadController();
            var result = downloadController.GetFile("", _fileUuid) as BadRequestErrorMessageResult;
            result.Should().NotBeNull();
            result?.Message.Should().Contain("fileUuid");
        }
    }
}