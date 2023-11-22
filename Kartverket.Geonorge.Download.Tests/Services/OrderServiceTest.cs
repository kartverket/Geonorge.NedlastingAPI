using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EntityFramework.MoqHelper;
using FluentAssertions;
using Geonorge.NedlastingApi.V3;
using Kartverket.Geonorge.Download.Migrations;
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
            return new OrderService(dbContext, null, null, _orderBundlingServiceMock.Object, notificationServiceMock.Object, null, null);
        }

        private DownloadContext CreateDbContextMock(List<Order> listOfOrders)
        {
            Mock<DbSet<Order>> mockOrders = EntityFrameworkMoqHelper
                .CreateMockForDbSet<Order>().SetupForQueryOn(listOfOrders);

            var listOfOrderItems = new List<OrderItem>();
            foreach (var order in listOfOrders)
                listOfOrderItems.AddRange(order.orderItem);

            Mock<DbSet<OrderItem>> mockOrderItems = EntityFrameworkMoqHelper
                .CreateMockForDbSet<OrderItem>().SetupForQueryOn(listOfOrderItems);

            Mock<DbSet<Dataset>> mockCapabilities = EntityFrameworkMoqHelper
                .CreateMockForDbSet<Dataset>().SetupForQueryOn(new List<Dataset>());

            Mock<DownloadContext> mockDbContext = EntityFrameworkMoqHelper
                .CreateMockForDbContext<DownloadContext>();

            

            // these lines should have been part of EntityFrameworkMoqHelper - consider patching the library
            mockDbContext.Setup(m => m.OrderDownloads).Returns(mockOrders.Object);
            mockDbContext.Setup(m => m.OrderItems).Returns(mockOrderItems.Object);
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
        public void ShouldInitiateBundlingIfOrderUpdatedWithDownloadAsBundleAndOrderIsReadyToDownload()
        {
            var originalOrder = new Order
            {
                Uuid = Guid.Parse("d42efa92-0226-4345-be61-eeb34d70419c"),
                orderItem = new List<OrderItem>()
                {
                    new OrderItem() {Status = OrderItemStatus.ReadyForDownload},
                    new OrderItem() {Status = OrderItemStatus.ReadyForDownload},
                }
            };
          
            var orderService = CreateOrderService(originalOrder);

            var incomingOrder = new OrderType() {downloadAsBundle = true, email = "test@example.com"};
            orderService.UpdateOrder(originalOrder, incomingOrder);

            _orderBundlingServiceMock.Verify(b => b.SendToBundling(originalOrder));
        }

        [Fact]
        public void ShouldNotInitiateBundlingIfOrderHasItemsWaitingForProcessing()
        {
            var originalOrder = new Order
            {
                Uuid = Guid.Parse("d42efa92-0226-4345-be61-eeb34d70419c"),
                orderItem = new List<OrderItem>()
                {
                    new OrderItem() {Status = OrderItemStatus.WaitingForProcessing},
                    new OrderItem() {Status = OrderItemStatus.ReadyForDownload},
                }
            };
          
            var orderService = CreateOrderService(originalOrder);

            var incomingOrder = new OrderType() {downloadAsBundle = true, email = "test@example.com"};
            orderService.UpdateOrder(originalOrder, incomingOrder);

            _orderBundlingServiceMock.Verify(b => b.SendToBundling(originalOrder), Times.Never);
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

        [Fact]
        public void UpdateFileStatusShouldInitiateBundlingIfOrderIsReadyForDownload()
        {

            var uuidItem1 = Guid.Parse("27c29ab2-8ea2-4016-93d0-f6c54fbedb25");
            var uuidItem2 = Guid.Parse("86d1ff02-379a-470f-968e-504ec61cb660");

            var originalOrder = new Order
            {
                Uuid = Guid.Parse("d42efa92-0226-4345-be61-eeb34d70419c"),
                DownloadAsBundle = true,
                orderItem = new List<OrderItem>()
                {
                    new OrderItem() {Id = 1, Uuid = uuidItem1, Status = OrderItemStatus.WaitingForProcessing},
                    new OrderItem() {Id = 2, Uuid = uuidItem2, Status = OrderItemStatus.ReadyForDownload},
                }
            };

            foreach (var item in originalOrder.orderItem) // need to create link back to order
                item.Order = originalOrder;
           
            var orderService = CreateOrderService(originalOrder);
            orderService.UpdateFileStatus(new UpdateFileStatusInformation()
            {
                FileId = uuidItem1.ToString(), Status = OrderItemStatus.ReadyForDownload
            });

            _orderBundlingServiceMock.Verify(b => b.SendToBundling(originalOrder));
        }
        
        [Fact]
        public void UpdateFileStatusShouldNotInitiateBundlingIfOrderHasItemsWaitingForProcessing()
        {

            var uuidItem1 = Guid.Parse("27c29ab2-8ea2-4016-93d0-f6c54fbedb25");
            var uuidItem2 = Guid.Parse("86d1ff02-379a-470f-968e-504ec61cb660");

            var originalOrder = new Order
            {
                Uuid = Guid.Parse("d42efa92-0226-4345-be61-eeb34d70419c"),
                DownloadAsBundle = true,
                orderItem = new List<OrderItem>()
                {
                    new OrderItem() {Id = 1, Uuid = uuidItem1, Status = OrderItemStatus.WaitingForProcessing},
                    new OrderItem() {Id = 2, Uuid = uuidItem2, Status = OrderItemStatus.WaitingForProcessing},
                }
            };

            foreach (var item in originalOrder.orderItem) // need to create link back to order
                item.Order = originalOrder;
           
            var orderService = CreateOrderService(originalOrder);
            orderService.UpdateFileStatus(new UpdateFileStatusInformation()
            {
                FileId = uuidItem1.ToString(), Status = OrderItemStatus.ReadyForDownload
            });

            _orderBundlingServiceMock.Verify(b => b.SendToBundling(originalOrder), Times.Never);
        }

        [Fact]
        public void UpdateFileStatusShouldNotInitiateBundlingIfOrderIsNotBeeingBundled()
        {

            var uuidItem1 = Guid.Parse("27c29ab2-8ea2-4016-93d0-f6c54fbedb25");
            var uuidItem2 = Guid.Parse("86d1ff02-379a-470f-968e-504ec61cb660");

            var originalOrder = new Order
            {
                Uuid = Guid.Parse("d42efa92-0226-4345-be61-eeb34d70419c"),
                DownloadAsBundle = false,
                orderItem = new List<OrderItem>()
                {
                    new OrderItem() {Id = 1, Uuid = uuidItem1, Status = OrderItemStatus.ReadyForDownload},
                    new OrderItem() {Id = 2, Uuid = uuidItem2, Status = OrderItemStatus.WaitingForProcessing},
                }
            };

            foreach (var item in originalOrder.orderItem) // need to create link back to order
                item.Order = originalOrder;
           
            var orderService = CreateOrderService(originalOrder);
            orderService.UpdateFileStatus(new UpdateFileStatusInformation()
            {
                FileId = uuidItem1.ToString(), Status = OrderItemStatus.ReadyForDownload
            });

            _orderBundlingServiceMock.Verify(b => b.SendToBundling(originalOrder), Times.Never);
        }

    }
}