using System.Web.Http.Results;
using FluentAssertions;
using Kartverket.Geonorge.Download.Controllers.Api.V3;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Controllers.Api.V3
{
    public class FileDownloadControllerTest
    {
        private readonly string _datasetUuid = "f428194c-ade6-461b-836f-5a21b8e8ec27";
        private readonly string _fileUuid = "f54b9e85-4c67-4fa7-9cae-d25258e320c1";

        [Fact]
        public void ShouldReturnBadRequestWhenFileIsNotAnUuid()
        {
            var downloadController = new FileDownloadController();
            var result = downloadController.GetFile(_datasetUuid, "") as BadRequestErrorMessageResult;
            result.Should().NotBeNull();
            result?.Message.Should().Contain("fileUuid");
        }

        [Fact]
        public void ShouldReturnBadRequestWhenOrderUuidIsNotAnUuid()
        {
            var downloadController = new FileDownloadController();
            var result = downloadController.GetFile("", _fileUuid) as BadRequestErrorMessageResult;
            result.Should().NotBeNull();
            result?.Message.Should().Contain("datasetUuid");
        }
    }
}