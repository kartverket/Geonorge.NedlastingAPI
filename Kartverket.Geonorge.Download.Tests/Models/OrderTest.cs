using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Kartverket.Geonorge.Download.Models;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Models
{
    public class OrderTest
    {
        [Fact]
        public void OrderWithRestrictedDatasetsCanBeDownloadedByTheUserWhoCreatedTheOrder()
        {
            var username = "username";
            var order = CreateOrder(username, CreateRestrictedOrderItem(), CreateOpenOrderItem());

            order.CanBeDownloadedByUser(username).Should().BeTrue();
        }
        [Fact]
        public void OrderWithRestrictedDatasetsCanNotBeDownloadedByAnonymousUser()
        {
            var order = CreateOrder("username", CreateRestrictedOrderItem(), CreateOpenOrderItem());

            order.CanBeDownloadedByUser(null).Should().BeFalse();
        }

        [Fact]
        public void OrderWithOnlyOpenDataCanBeDownloadedByAnonymousUser()
        {
            var order = CreateOrder(null, CreateOpenOrderItem());

            order.CanBeDownloadedByUser(null).Should().BeTrue();
        }

        [Fact]
        public void OrderWithOnlyOpenDataCanBeDownloadedByLoggedInUser()
        {
            var order = CreateOrder(null, CreateOpenOrderItem(), CreateOpenOrderItem());

            order.CanBeDownloadedByUser("username").Should().BeTrue();
        }

        private static Order CreateOrder(string username, params OrderItem[] orderItems)
        {
            var order = new Order {username = username};
            order.AddOrderItems(orderItems.ToList());
            return order;
        }

        private static OrderItem CreateRestrictedOrderItem()
        {
            return new OrderItem() { AccessConstraint = new AccessConstraint() { Constraint = "restricted" } };
        }

        private static OrderItem CreateOpenOrderItem()
        {
            return new OrderItem() { AccessConstraint = new AccessConstraint() };
        }

    }
}
