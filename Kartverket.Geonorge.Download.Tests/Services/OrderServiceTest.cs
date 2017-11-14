using System;
using System.Collections.Generic;
using System.Data.Entity;
using EntityFramework.MoqHelper;
using FluentAssertions;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Moq;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Services
{
    public class OrderServiceTest
    {
        private static OrderService CreateOrderService(List<Order> listOfOrders)
        {
            var dbContext = CreateDbContextMock(listOfOrders);

            return new OrderService(dbContext, null, null);
        }

        private static DownloadContext CreateDbContextMock(List<Order> listOfOrders)
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
            var listOfOrders = new List<Order>();
            var orderService = CreateOrderService(listOfOrders);

            var order = orderService.Find("uuid");
            order.Should().BeNull();
        }

        [Fact]
        public void FindOrderShouldReturnOrder()
        {
            var uuid = "d42efa92-0226-4345-be61-eeb34d70419c";
            var listOfOrders = new List<Order>
            {
                new Order
                {
                    Uuid = Guid.Parse(uuid)
                }
            };
            var orderService = CreateOrderService(listOfOrders);

            var order = orderService.Find(uuid);
            order.Should().NotBeNull();
        }
    }
}