using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Results;
using FluentAssertions;
using Kartverket.Geonorge.Download.Controllers.Api.V2;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Kartverket.Geonorge.Download.Services.Auth;
using Moq;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Controllers.Api.V2
{
    public class DownloadControllerTest
    {
        public DownloadControllerTest()
        {
            ConfigurationManager.AppSettings["DownloadUrl"] = "https://nedlasting.geonorge.no/";
        }

        private readonly string NonExistingFileId = "ac7b07a9-80cf-4260-ad32-f1966f25f885";
        private readonly string NonExistingOrderUuid = "a2158025-4a25-4309-86d1-5026d047bc9b";
        private const string OrderUuid = "c7a7427e-99c8-4edd-a705-6cd24f38e646";
        private const string FileId = "61786107-565d-4e95-8f01-d3dacadc376b";
        private const string OrderUsername = "johndoe";
        private const string DownloadUrl = "http://nedlasting.geonorge.no/dummyfile.zip";

        private static Order CreateOrder(string username, string accessConstraint, string orderUuid, string fileId,
            OrderItemStatus orderItemStatus)
        {
            var order = new Order
            {
                Uuid = Guid.Parse(orderUuid),
                username = username,
                orderItem = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Uuid = Guid.Parse(fileId),
                        AccessConstraint = new AccessConstraint(accessConstraint),
                        Status = orderItemStatus,
                        DownloadUrl = DownloadUrl
                    }
                }
            };
            return order;
        }


        private static Mock<IOrderService> CreateOrderServiceMock(string accessConstraint = null,
            OrderItemStatus orderItemStatus = OrderItemStatus.ReadyForDownload)
        {
            var orderServiceMock = new Mock<IOrderService>();
            var order = CreateOrder(OrderUsername, accessConstraint, OrderUuid, FileId, orderItemStatus);
            orderServiceMock.Setup(o => o.Find(OrderUuid)).Returns(order);
            return orderServiceMock;
        }

        private IAuthenticationService AuthenticatedUserIs(string username)
        {
            var mock = new Mock<IAuthenticationService>();
            if (!string.IsNullOrEmpty(username))
                mock.Setup(m => m.GetAuthenticatedUser(It.IsAny<HttpRequestMessage>()))
                    .Returns(new AuthenticatedUser(username, AuthenticationMethod.Baat));

            return mock.Object;
        }

        private static DownloadController CreateDownloadController(IOrderService orderServiceMock,
            IDownloadService downloadServiceMock, IAuthenticationService authenticationMock)
        {
            var downloadController = new DownloadController(orderServiceMock, downloadServiceMock, authenticationMock);
            // these lines are needed to use the Content-method inside the controller
            downloadController.Request = new HttpRequestMessage();
            downloadController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            return downloadController;
        }

        [Fact]
        public void ShouldRedirectToDownloadUrlWhenDatasetIsOpen()
        {
            var downloadController =
                CreateDownloadController(CreateOrderServiceMock().Object, null, AuthenticatedUserIs(null));
            var result = downloadController.GetFile(OrderUuid, FileId) as RedirectResult;
            result.Should().NotBeNull();
            result?.Location.ToString().Should().Be(DownloadUrl);
        }

        [Fact]
        public void ShouldRedirectToLoginPageWhenUserIsAccessingRestrictedDatasetAndNotLoggedIn()
        {
            var orderServiceMock = CreateOrderServiceMock(AccessConstraint.NorgeDigitalRestricted).Object;
            var downloadController = CreateDownloadController(orderServiceMock, null, AuthenticatedUserIs(null));
            var result = downloadController.GetFile(OrderUuid, FileId) as RedirectResult;
            result.Should().NotBeNull();
            result?.Location.ToString().Should()
                .Contain("SignIn"); // kentor authservices uses SignIn in url for login page
        }


        [Fact]
        public void ShouldReturnBadRequestWhenFileIsNotAnUuid()
        {
            var downloadController =
                CreateDownloadController(CreateOrderServiceMock().Object, null, AuthenticatedUserIs(null));
            var result = downloadController.GetFile(OrderUuid, "") as BadRequestErrorMessageResult;
            result.Should().NotBeNull();
            result?.Message.Should().Contain("fileId");
        }

        [Fact]
        public void ShouldReturnBadRequestWhenOrderUuidIsNotAnUuid()
        {
            var downloadController =
                CreateDownloadController(CreateOrderServiceMock().Object, null, AuthenticatedUserIs(null));
            var result = downloadController.GetFile("", FileId) as BadRequestErrorMessageResult;
            result.Should().NotBeNull();
            result?.Message.Should().Contain("orderUuid");
        }

        [Fact]
        public void ShouldReturnForbiddenWhenUserTriesToAccessOtherUsersOrder()
        {
            var orderServiceMock = CreateOrderServiceMock(AccessConstraint.NorgeDigitalRestricted).Object;
            var downloadController =
                CreateDownloadController(orderServiceMock, null, AuthenticatedUserIs("anotherUsername"));
            var result = downloadController.GetFile(OrderUuid, FileId);

            var response = result.ExecuteAsync(CancellationToken.None).Result;
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public void ShouldReturnNotFoundWhenFileIdDoesNotExist()
        {
            var downloadController = CreateDownloadController(CreateOrderServiceMock().Object, null,
                AuthenticatedUserIs(OrderUsername));
            var result = downloadController.GetFile(OrderUuid, NonExistingFileId) as NotFoundResult;
            result.Should().NotBeNull();
        }

        [Fact]
        public void ShouldReturnNotFoundWhenOrderDoesNotExist()
        {
            var downloadController =
                new DownloadController(new Mock<IOrderService>().Object, null, AuthenticatedUserIs(null));
            var result = downloadController.GetFile(NonExistingOrderUuid, NonExistingFileId) as NotFoundResult;
            result.Should().NotBeNull();
        }

        [Fact]
        public void ShouldReturnNotFoundWhenOrderItemIsNotReadyToBeDownloaded()
        {
            var orderService = CreateOrderServiceMock(orderItemStatus: OrderItemStatus.WaitingForProcessing).Object;
            var downloadController = new DownloadController(orderService, null, AuthenticatedUserIs(OrderUsername));
            var result = downloadController.GetFile(OrderUuid, FileId) as NotFoundResult;
            result.Should().NotBeNull();
        }

        [Fact]
        public void ShouldStreamFileResponseWhenUserHasAccessToRestrictedDataset()
        {
            var orderService = CreateOrderServiceMock(AccessConstraint.NorgeDigitalRestricted).Object;

            var downloadServiceMock = new Mock<IDownloadService>();
            var stringWriter = new StringWriter();
            stringWriter.WriteLine("This is the content of the delivered file.");
            var httpResponse = new HttpResponse(stringWriter);
            downloadServiceMock.Setup(d => d.CreateResponseFromRemoteFile(It.IsAny<string>())).Returns(httpResponse);

            var downloadController = CreateDownloadController(orderService, downloadServiceMock.Object,
                AuthenticatedUserIs(OrderUsername));

            var result = downloadController.GetFile(OrderUuid, FileId);

            var response = result.ExecuteAsync(CancellationToken.None).Result;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}