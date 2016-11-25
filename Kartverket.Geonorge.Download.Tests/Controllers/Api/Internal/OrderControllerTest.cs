using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using FluentAssertions;
using Kartverket.Geonorge.Download.Controllers.Api.Internal;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Models.Api.Internal;
using Kartverket.Geonorge.Download.Services;
using Moq;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Controllers.Api.Internal
{
    public class OrderControllerTest
    {
        private IHttpActionResult ExecuteUpdateFileStatus(IUpdateFileStatusService updateService, UpdateFileStatusRequest request)
        {
            var orderController = CreateController(updateService);
            var response = orderController.UpdateFileStatus(request);
            return response;
        }

        private ManageOrderController CreateController(IUpdateFileStatusService updateFileStatusService)
        {
            var controller = new ManageOrderController(updateFileStatusService);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            return controller;
        }

        [Fact]
        public void ShouldReturInternalServerErrorWhenExceptionIsThrown()
        {
            var updateServiceMock = new Mock<IUpdateFileStatusService>();
            updateServiceMock.Setup(m => m.UpdateFileStatus(It.IsAny<UpdateFileStatusInformation>())).Throws(new Exception());
            var response = ExecuteUpdateFileStatus(updateServiceMock.Object, new UpdateFileStatusRequest
            {
                FileId = "123",
                DownloadUrl = "http://blabla.com/testfile.zip",
                Status = "ReadyForDownload"
            });

            response.GetType().Should().Be(typeof(ExceptionResult));
        }

        [Fact]
        public void ShouldReturnBadRequestWhenInvalidStatus()
        {
            var updateServiceMock = new Mock<IUpdateFileStatusService>();
            updateServiceMock.Setup(m => m.UpdateFileStatus(It.IsAny<UpdateFileStatusInformation>()));
            var response = ExecuteUpdateFileStatus(updateServiceMock.Object, new UpdateFileStatusRequest
            {
                FileId = "123",
                DownloadUrl = "http://blabla.com/testfile.zip",
                Status = "ThisIsNotAValidStatus"
            });

            updateServiceMock.Verify(m => m.UpdateFileStatus(It.IsAny<UpdateFileStatusInformation>()), Times.Never);

            response.GetType().Should().Be(typeof(BadRequestErrorMessageResult));
        }

        [Fact]
        public void ShouldUpdateFileStatus()
        {
            var updateServiceMock = new Mock<IUpdateFileStatusService>();
            updateServiceMock.Setup(m => m.UpdateFileStatus(It.IsAny<UpdateFileStatusInformation>()));
            var response = ExecuteUpdateFileStatus(updateServiceMock.Object, new UpdateFileStatusRequest
            {
                FileId = "123",
                DownloadUrl = "http://blabla.com/testfile.zip",
                Status = "ReadyForDownload"
            });

            updateServiceMock.Verify(m => m.UpdateFileStatus(It.IsAny<UpdateFileStatusInformation>()));

            response.GetType().Should().Be(typeof(OkResult));
        }
    }
}