using Geonorge.Download.Models;

namespace Geonorge.Download.Tests.Models
{
    public class OrderTest
    {
        [Fact]
        public void OrderWithRestrictedDatasetsCanBeDownloadedByTheUserWhoCreatedTheOrder()
        {
            var order = CreateOrder("testuser", CreateRestrictedOrderItem(), CreateOpenOrderItem());

            Assert.True(order.CanBeDownloadedByUser(TestPrincipals.User("testuser")));
        }

        [Fact]
        public void UsernameComparisonIsCaseInsensitive()
        {
            var order = CreateOrder("TestUser", CreateRestrictedOrderItem());

            Assert.True(order.CanBeDownloadedByUser(TestPrincipals.User("testuser")));
        }

        [Fact]
        public void OrderWithRestrictedDatasetsCanNotBeDownloadedByAnonymousUser()
        {
            var order = CreateOrder("testuser", CreateRestrictedOrderItem(), CreateOpenOrderItem());

            Assert.False(order.CanBeDownloadedByUser(TestPrincipals.Anonymous()));
        }

        [Fact]
        public void OrderWithRestrictedDatasetsCanNotBeDownloadedByNullPrincipal()
        {
            var order = CreateOrder("testuser", CreateRestrictedOrderItem());

            Assert.False(order.CanBeDownloadedByUser(null!));
        }

        [Fact]
        public void OrderWithRestrictedDatasetsCanNotBeDownloadedByAnotherUser()
        {
            var order = CreateOrder("testuser", CreateRestrictedOrderItem());

            Assert.False(order.CanBeDownloadedByUser(TestPrincipals.User("anotheruser")));
        }

        [Fact]
        public void AnonymousOrderWithRestrictedDatasetsCanNotBeDownloadedByAnyone()
        {
            var order = CreateOrder(null, CreateRestrictedOrderItem());

            Assert.False(order.CanBeDownloadedByUser(TestPrincipals.Anonymous()));
            Assert.False(order.CanBeDownloadedByUser(TestPrincipals.User("testuser")));
        }

        [Fact]
        public void OrderWithOnlyOpenDatasetsCanBeDownloadedByAnyone()
        {
            var order = CreateOrder("testuser", CreateOpenOrderItem(), CreateOpenOrderItem());

            Assert.True(order.CanBeDownloadedByUser(TestPrincipals.Anonymous()));
            Assert.True(order.CanBeDownloadedByUser(TestPrincipals.User("anotheruser")));
        }

        [Fact]
        public void AddAccessConstraintsAppliesConstraintToMatchingOrderItems()
        {
            var item = CreateOpenOrderItem();
            item.MetadataUuid = "dataset-1";
            var order = CreateOrder("testuser", item);

            order.AddAccessConstraints(
            [
                new DatasetAccessConstraint
                {
                    MetadataUuid = "dataset-1",
                    AccessConstraint = new AccessConstraint(AccessConstraint.Restricted)
                }
            ]);

            Assert.True(order.ContainsRestrictedDatasets());
        }

        [Fact]
        public void AddAccessConstraintsIgnoresConstraintsForOtherDatasets()
        {
            var item = CreateOpenOrderItem();
            item.MetadataUuid = "dataset-1";
            var order = CreateOrder("testuser", item);

            order.AddAccessConstraints(
            [
                new DatasetAccessConstraint
                {
                    MetadataUuid = "dataset-2",
                    AccessConstraint = new AccessConstraint(AccessConstraint.Restricted)
                }
            ]);

            Assert.False(order.ContainsRestrictedDatasets());
        }

        [Fact]
        public void GetItemWithFileIdReturnsMatchingItem()
        {
            var item = CreateOpenOrderItem();
            var order = CreateOrder("testuser", CreateOpenOrderItem(), item);

            Assert.Same(item, order.GetItemWithFileId(item.Uuid.ToString()));
        }

        [Theory]
        [InlineData("not-a-guid")]
        [InlineData("00000000-0000-0000-0000-000000000001")]
        public void GetItemWithFileIdReturnsNullForInvalidOrUnknownId(string fileId)
        {
            var order = CreateOrder("testuser", CreateOpenOrderItem());

            Assert.Null(order.GetItemWithFileId(fileId));
        }

        private static Order CreateOrder(string? username, params OrderItem[] items)
        {
            var order = new Order { username = username };
            order.AddOrderItems(items.ToList());
            return order;
        }

        private static OrderItem CreateOpenOrderItem() => new();

        private static OrderItem CreateRestrictedOrderItem() => new()
        {
            AccessConstraint = new AccessConstraint(AccessConstraint.NorgeDigitalRestricted)
        };
    }
}
