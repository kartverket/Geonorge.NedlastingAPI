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

        [Fact]
        public void ShouldUpdateFileStatus()
        {
            var orderServiceMock = CreateOrderServiceMock();
            var response = ExecuteUpdateFileStatus(orderServiceMock, new UpdateFileStatusRequest
            {
                FileId = "123",
                DownloadUrl = "http://blabla.com/testfile.zip",
                Status = "ReadyForDownload"
            });

            orderServiceMock.Verify(m => m.UpdateFileStatus(It.IsAny<UpdateFileStatusInformation>()));

            response.GetType().Should().Be(typeof(OkResult));
        }

        [Fact]
        public void ShouldReturnBadRequestWhenFileIdIsNotANumber()
        {
            var orderServiceMock = CreateOrderServiceMock();
            var response = ExecuteUpdateFileStatus(orderServiceMock, new UpdateFileStatusRequest
            {
                FileId = "aaaaaaaa123",
                DownloadUrl = "http://blabla.com/testfile.zip",
                Status = "ReadyForDownload"
            });

            orderServiceMock.Verify(m => m.UpdateFileStatus(It.IsAny<UpdateFileStatusInformation>()), Times.Never);

            response.GetType().Should().Be(typeof(BadRequestErrorMessageResult));
        }

        [Fact]
        public void ShouldReturnBadRequestWhenInvalidStatus()
        {
            var orderServiceMock = CreateOrderServiceMock();
            var response = ExecuteUpdateFileStatus(orderServiceMock, new UpdateFileStatusRequest
            {
                FileId = "123",
                DownloadUrl = "http://blabla.com/testfile.zip",
                Status = "ThisIsNotAValidStatus"
            });

            orderServiceMock.Verify(m => m.UpdateFileStatus(It.IsAny<UpdateFileStatusInformation>()), Times.Never);

            response.GetType().Should().Be(typeof(BadRequestErrorMessageResult));
        }

        [Fact]
        public void ShouldReturInternalServerErrorWhenExceptionIsThrown()
        {
            var orderServiceMock = new Mock<IOrderService>();
            orderServiceMock.Setup(m => m.UpdateFileStatus(It.IsAny<UpdateFileStatusInformation>())).Throws(new ArgumentException());
            var response = ExecuteUpdateFileStatus(orderServiceMock, new UpdateFileStatusRequest
            {
                FileId = "123",
                DownloadUrl = "http://blabla.com/testfile.zip",
                Status = "ReadyForDownload"
            });

            response.GetType().Should().Be(typeof(ExceptionResult));
        }

        private IHttpActionResult ExecuteUpdateFileStatus(Mock<IOrderService> orderServiceMock, UpdateFileStatusRequest request)
        {
            OrderController orderController = CreateController(orderServiceMock);
            IHttpActionResult response = orderController.UpdateFileStatus(request);
            return response;
        }

        private static Mock<IOrderService> CreateOrderServiceMock()
        {
            var orderServiceMock = new Mock<IOrderService>();
            orderServiceMock.Setup(m => m.UpdateFileStatus(It.IsAny<UpdateFileStatusInformation>()));
            return orderServiceMock;
        }
        
        private OrderController CreateController(Mock<IOrderService> orderServiceMock)
        {
            OrderController controller = new OrderController(orderServiceMock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            return controller;
        }

    }
}
