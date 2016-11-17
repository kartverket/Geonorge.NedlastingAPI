using System;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Models
{
    public class OrderTest
    {
        [Fact]
        public void OrdersWithRestrictedDatasetsCanBeDownloadedByTheUserWhoCreatedTheOrder()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void OrdersWithRestrictedDatasetsCanNotBeDownloadedByAnonymousUser()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void OrdersWithOnlyOpenDataCanBeDownloadedByAnonymousUser()
        {
            throw new NotImplementedException();

        }

        [Fact]
        public void OrdersWithOnlyOpenDataCanBeDownloadedByLoggedInUser()
        {
            throw new NotImplementedException();
        }
    }
}
