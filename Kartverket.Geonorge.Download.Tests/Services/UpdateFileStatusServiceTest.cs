using System;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Moq;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Services
{
    public class UpdateFileStatusServiceTest
    {
        [Fact]
        public void ShouldUpdateStatusWhenChanged()
        {
            var fileId = "8910c2bb-6323-42d0-8230-fda622ce6f43";
            var orderItem = new OrderItem {Status = OrderItemStatus.WaitingForProcessing};

            var orderServiceMock = new Mock<IOrderService>();
            orderServiceMock.Setup(o => o.FindOrderItem(fileId)).Returns(orderItem);

            var notificationServiceMock = new Mock<INotificationService>();

            var service = new UpdateFileStatusService(orderServiceMock.Object, notificationServiceMock.Object);

            var updateFileStatusInformation = new UpdateFileStatusInformation() { FileId = fileId, Status = OrderItemStatus.ReadyForDownload};
            service.UpdateFileStatus(updateFileStatusInformation);


            orderServiceMock.Verify(o => o.UpdateFileStatus(updateFileStatusInformation));
            notificationServiceMock.Verify(n => n.SendReadyForDownloadNotification(It.IsAny<string>()));

        }

        [Fact]
        public void ShouldNotUpdateWhenStatusIsSame()
        {
            var fileId = "8910c2bb-6323-42d0-8230-fda622ce6f43";
            var orderItem = new OrderItem { Status = OrderItemStatus.ReadyForDownload };

            var orderServiceMock = new Mock<IOrderService>();
            orderServiceMock.Setup(o => o.FindOrderItem(fileId)).Returns(orderItem);
            var notificationServiceMock = new Mock<INotificationService>();

            var service = new UpdateFileStatusService(orderServiceMock.Object, notificationServiceMock.Object);

            var updateFileStatusInformation = new UpdateFileStatusInformation() { FileId = fileId, Status = OrderItemStatus.ReadyForDownload };
            service.UpdateFileStatus(updateFileStatusInformation);

            orderServiceMock.Verify(o => o.UpdateFileStatus(updateFileStatusInformation), Times.Never);
            notificationServiceMock.Verify(n => n.SendReadyForDownloadNotification(It.IsAny<string>()), Times.Never);
        }
    }
}
