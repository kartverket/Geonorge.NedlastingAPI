using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EntityFramework.MoqHelper;
using FluentAssertions;
using Geonorge.NedlastingApi.V3;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Moq;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Services
{
    public class OrderServiceTest
    {
        private readonly Mock<IOrderBundleService> _orderBundlingServiceMock;

        public OrderServiceTest()
        {
            _orderBundlingServiceMock = new Mock<IOrderBundleService>();

        }

        private OrderService CreateOrderService(params Order[] orders)
        {
            return CreateOrderService(orders.ToList());
        }

        private OrderService CreateOrderService(List<Order> listOfOrders)
        {
            var dbContext = CreateDbContextMock(listOfOrders);
            var notificationServiceMock = new Mock<INotificationService>();
            return new OrderService(dbContext, null, null, _orderBundlingServiceMock.Object, notificationServiceMock.Object);
        }

        private DownloadContext CreateDbContextMock(List<Order> listOfOrders)
        {
            Mock<DbSet<Order>> mockOrders = EntityFrameworkMoqHelper
                .CreateMockForDbSet<Order>().SetupForQueryOn(listOfOrders);

            Mock<DbSet<Dataset>> mockCapabilities = EntityFrameworkMoqHelper
                .CreateMockForDbSet<Dataset>().SetupForQueryOn(new List<Dataset>());

            Mock<DownloadContext> mockDbContext = EntityFrameworkMoqHelper
                .CreateMockForDbContext<DownloadContext>();

            // these lines should have been part of EntityFrameworkMoqHelper - consider patching the library
            mockDbContext.Setup(m => m.OrderDownloads).Returns(mockOrders.Object);
            mockDbContext.Setup(m => m.Capabilities).Returns(mockCapabilities.Object);

            return mockDbContext.Object;
        }

        [Fact]
        public void FindOrderShouldReturnNullWhenNotFound()
        {
            var orderService = CreateOrderService(new List<Order>());

            var order = orderService.Find("uuid");
            order.Should().BeNull();
        }

        [Fact]
        public void FindOrderShouldReturnOrder()
        {
            var uuid = "d42efa92-0226-4345-be61-eeb34d70419c";
            var orderService = CreateOrderService(new Order
            {
                Uuid = Guid.Parse(uuid)
            });

            var order = orderService.Find(uuid);
            order.Should().NotBeNull();
        }

        [Fact]
        public void ShouldInitiateBundlingIfOrderUpdatedWithDownloadAsBundle()
        {
            var originalOrder = new Order
            {
                Uuid = Guid.Parse("d42efa92-0226-4345-be61-eeb34d70419c")
            };
          
            var orderService = CreateOrderService(originalOrder);

            var incomingOrder = new OrderType() {downloadAsBundle = true, email = "test@example.com"};
            orderService.UpdateOrder(originalOrder, incomingOrder);

            _orderBundlingServiceMock.Verify(b => b.SendToBundling(originalOrder));
        }

        [Fact]
        public void ShouldNotInitiateBundlingIfOrderUpdatedWithDownloadAsBundleEqualsFalse()
        {
            var originalOrder = new Order
            {
                Uuid = Guid.Parse("d42efa92-0226-4345-be61-eeb34d70419c")
            };
           
            var orderService = CreateOrderService(originalOrder);

            var incomingOrder = new OrderType() { downloadAsBundle = false, email = "test@example.com" };
            orderService.UpdateOrder(originalOrder, incomingOrder);

            _orderBundlingServiceMock.Verify(b => b.SendToBundling(originalOrder), Times.Never);

        }
    }
}