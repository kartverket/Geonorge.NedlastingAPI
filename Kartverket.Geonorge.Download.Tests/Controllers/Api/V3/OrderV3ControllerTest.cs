using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Http.Results;
using FluentAssertions;
using Geonorge.NedlastingApi.V3;
using Kartverket.Geonorge.Download.Controllers.Api.V3;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Kartverket.Geonorge.Download.Services.Auth;
using Moq;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Controllers.Api.V3
{
    public class OrderV3ControllerTest
    {
        public OrderV3ControllerTest()
        {
            _orderServiceMock = new Mock<IOrderService>();

            _authenticationServiceMock = new Mock<IAuthenticationService>();
        }

        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IAuthenticationService> _authenticationServiceMock;

        private OrderV3Controller CreateController()
        {
            var controller = new OrderV3Controller(_orderServiceMock.Object, _authenticationServiceMock.Object);
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://tempuri.org", ""),
                new HttpResponse(new StringWriter())
            );

            return controller;
        }

        private static Order CreateRestrictedOrder(string username = null)
        {
            return new Order
            {
                username = username,
                orderItem = new List<OrderItem>
                {
                    new OrderItem
                    {
                        AccessConstraint = new AccessConstraint(AccessConstraint.NorgeDigitalRestricted)
                    }
                }
            };
        }

        private static Order CreateOpenDataOrder(string username = null)
        {
            return new Order
            {
                username = username,
                orderItem = new List<OrderItem>
                {
                    new OrderItem()
                }
            };
        }

        [Fact]
        public void ShouldReturnNotFoundWhenOrderDoesNotExist()
        {
            var controller = new OrderV3Controller(_orderServiceMock.Object, _authenticationServiceMock.Object);

            var updatedOrder = new OrderType {downloadAsBundle = true, email = "dummy@example.com"};
            var result = controller.UpdateOrder("asdasd", updatedOrder) as NotFoundResult;
            result.Should().NotBeNull();
        }

        [Fact]
        public void ShouldReturnOkWhenOpenDataOrderAndUserIsLoggedIn()
        {
            var orderUuid = "orderId";
            var loggedInUsername = "aUserName";

            _orderServiceMock.Setup(o => o.Find(orderUuid)).Returns(CreateOpenDataOrder());

            _authenticationServiceMock.Setup(a => a.GetAuthenticatedUser(It.IsAny<HttpRequestMessage>()))
                .Returns(new AuthenticatedUser(loggedInUsername, AuthenticationMethod.Baat));

            var updatedOrder = new OrderType {downloadAsBundle = true, email = "dummy@example.com"};

            var result = CreateController().UpdateOrder(orderUuid, updatedOrder);

            var okResult = result as OkNegotiatedContentResult<OrderReceiptType>;

            okResult.Should().NotBeNull();
        }

        [Fact]
        public void ShouldReturnOkWhenOpenDataOrderAndUserIsNotLoggedIn()
        {
            var orderUuid = "orderId";

            _orderServiceMock.Setup(o => o.Find(orderUuid)).Returns(CreateOpenDataOrder());

            var updatedOrder = new OrderType {downloadAsBundle = true, email = "dummy@example.com"};

            var result = CreateController().UpdateOrder(orderUuid, updatedOrder);

            var okResult = result as OkNegotiatedContentResult<OrderReceiptType>;

            okResult.Should().NotBeNull();
        }

        [Fact]
        public void ShouldReturnOkWhenRestrictedOrderAndUserIsLoggedIn()
        {
            var orderUuid = "orderId";
            var loggedInUsername = "aUserName";

            _orderServiceMock.Setup(o => o.Find(orderUuid)).Returns(CreateRestrictedOrder(loggedInUsername));

            _authenticationServiceMock.Setup(a => a.GetAuthenticatedUser(It.IsAny<HttpRequestMessage>()))
                .Returns(new AuthenticatedUser(loggedInUsername, AuthenticationMethod.Baat));

            var updatedOrder = new OrderType {downloadAsBundle = true, email = "dummy@example.com"};

            var result = CreateController().UpdateOrder(orderUuid, updatedOrder);

            var okResult = result as OkNegotiatedContentResult<OrderReceiptType>;

            okResult.Should().NotBeNull();
        }


        [Fact]
        public void ShouldReturnUnauthorizedWhenRestrictedOrderAndUserIsNotLoggedIn()
        {
            var orderUuid = "orderId";
            _orderServiceMock.Setup(o => o.Find(orderUuid)).Returns(CreateRestrictedOrder());

            var controller = new OrderV3Controller(_orderServiceMock.Object, _authenticationServiceMock.Object);

            var updatedOrder = new OrderType {downloadAsBundle = true, email = "dummy@example.com"};
            var result = controller.UpdateOrder(orderUuid, updatedOrder) as UnauthorizedResult;
            result.Should().NotBeNull();
        }

        [Fact]
        public void ShouldReturnUnauthorizedWhenRestrictedOrderAndOrderDoesNotBelongToLoggedInUser()
        {
            var orderUuid = "orderId";
            var loggedInUsername = "aUserName";

            _orderServiceMock.Setup(o => o.Find(orderUuid)).Returns(CreateRestrictedOrder("anotherUsername"));

            _authenticationServiceMock.Setup(a => a.GetAuthenticatedUser(It.IsAny<HttpRequestMessage>()))
                .Returns(new AuthenticatedUser(loggedInUsername, AuthenticationMethod.Baat));

            var updatedOrder = new OrderType {downloadAsBundle = true, email = "dummy@example.com"};

            var result = CreateController().UpdateOrder(orderUuid, updatedOrder) as UnauthorizedResult;

            result.Should().NotBeNull();
        }
    }
}