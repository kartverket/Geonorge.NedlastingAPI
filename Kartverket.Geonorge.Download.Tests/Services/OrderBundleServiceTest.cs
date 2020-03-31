using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Moq;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Services
{
    public class OrderBundleServiceTest
    {
        private const string OrderUuid = "f00912c0-4342-436b-a905-f7468bf36943";
        private const string FileId1 = "8a2ef3d5-5e51-451d-b29a-5d78ba763ed2";
        private const string FileId2 = "10e39bcf-4b7e-47bd-abd7-7503a1c00668";
        private const string FileId3 = "8d33e1aa-6293-494b-95e8-5e20702f126e";

        private readonly string _expectedUrl =
            "http://example.com/api?UUIDFILE=api%2forder%2fuuidfile%2f"+ OrderUuid + "&ORDERID="+OrderUuid+"&opt_servicemode=async";

        private readonly Order _order = new Order
        {
            Uuid = Guid.Parse(OrderUuid),
            orderItem = new List<OrderItem>
            {
                new OrderItem
                {
                    Uuid = Guid.Parse(FileId1)
                },
                new OrderItem
                {
                    Uuid = Guid.Parse(FileId2)
                },
                new OrderItem {Uuid = Guid.Parse(FileId3)}
            }
        };

        private Mock<IExternalRequestService> CreateMock(HttpStatusCode statusCode, string content)
        {
            var externalRequestServiceMock = new Mock<IExternalRequestService>();

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            };

            externalRequestServiceMock.Setup(s => s.RunRequestAsync(_expectedUrl)).ReturnsAsync(httpResponseMessage);

            return externalRequestServiceMock;
        }

        [Fact]
        public void ShouldSendBundleRequest()
        {
            var externalRequestServiceMock = CreateMock(HttpStatusCode.NoContent, "");
            var orderBundleService = new OrderBundleService(externalRequestServiceMock.Object);

            orderBundleService.SendToBundling(_order);

            externalRequestServiceMock.Verify(s => s.RunRequestAsync(_expectedUrl));
        }

        [Fact]
        public void SendBundleRequestShouldThrowExceptionWhenErrorCodeReturnedFromService()
        {
            var orderBundleService =
                new OrderBundleService(CreateMock(HttpStatusCode.BadRequest, "bad request").Object);

            Assert.Throws<ExternalRequestException>(() => orderBundleService.SendToBundling(_order));
            
        }

        [Fact]
        public void SendBundleRequestShouldThrowExceptionWhenErrorOccursWhileCommunicatingWithService()
        {
            var externalRequestServiceMock = new Mock<IExternalRequestService>();
            externalRequestServiceMock.Setup(s => s.RunRequestAsync(_expectedUrl)).Throws(new HttpException());
            var orderBundleService = new OrderBundleService(externalRequestServiceMock.Object);

            Assert.Throws<ExternalRequestException>(() => orderBundleService.SendToBundling(_order));
        }
    }
}