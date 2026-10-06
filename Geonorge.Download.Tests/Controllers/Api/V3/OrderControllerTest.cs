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
    public class OrderControllerTest
    {
        private const string OrderUuid = "3f854e9c-5345-4428-b2fd-1ea3db5d2f7a";

        private readonly IOrderService _orderService = Substitute.For<IOrderService>();

        [Fact]
        public void ShouldReturnNotFoundWhenOrderDoesNotExist()
        {
            var result = CreateController(TestPrincipals.User("testuser")).GetOrder(OrderUuid);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void ShouldReturnUnauthorizedWhenRestrictedOrderAndUserIsNotLoggedIn()
        {
            _orderService.Find(OrderUuid).Returns(CreateRestrictedOrder("testuser"));

            var result = CreateController(TestPrincipals.Anonymous()).GetOrder(OrderUuid);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void ShouldReturnUnauthorizedWhenRestrictedOrderDoesNotBelongToLoggedInUser()
        {
            _orderService.Find(OrderUuid).Returns(CreateRestrictedOrder("anotheruser"));

            var result = CreateController(TestPrincipals.User("testuser")).GetOrder(OrderUuid);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void ShouldReturnOkWhenRestrictedOrderBelongsToLoggedInUser()
        {
            _orderService.Find(OrderUuid).Returns(CreateRestrictedOrder("testuser"));

            var result = CreateController(TestPrincipals.User("testuser")).GetOrder(OrderUuid);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void ShouldRedirectBrowserToOrderDetailsPage()
        {
            var result = CreateController(TestPrincipals.Anonymous(), accept: "text/html").GetOrder(OrderUuid);

            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.EndsWith($"/order/details/{OrderUuid}", redirect.Url);
            _orderService.DidNotReceive().Find(Arg.Any<string>());
        }

        private static Order CreateRestrictedOrder(string username)
        {
            var order = new Order { username = username, Uuid = Guid.Parse(OrderUuid) };
            order.AddOrderItems(
            [
                new OrderItem { AccessConstraint = new AccessConstraint(AccessConstraint.NorgeDigitalRestricted) }
            ]);
            return order;
        }

        private OrderController CreateController(ClaimsPrincipal user, string? accept = null)
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["DownloadUrl"] = "https://download.example.com/" })
                .Build();

            var httpContext = new DefaultHttpContext { User = user };
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("download.example.com");
            if (accept != null)
                httpContext.Request.Headers.Accept = accept;

            return new OrderController(NullLogger<OrderController>.Instance, config, _orderService)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };
        }
    }
}
