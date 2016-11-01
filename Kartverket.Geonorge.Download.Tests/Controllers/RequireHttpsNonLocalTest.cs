using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using FluentAssertions;
using Kartverket.Geonorge.Download.Controllers;
using Xunit;

namespace Kartverket.Geonorge.Download.Tests.Controllers
{
    public class RequireHttpsNonLocalTest
    {
        [Fact]
        public void ShouldRequireHttpsWhenRemoteHttpRequest()
        {
            HttpActionContext httpActionContext = CreateHttpActionContext("http://localhost/api/test", false);
            new RequireHttpsNonLocal().OnAuthorization(httpActionContext);

            httpActionContext.Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public void ShouldNotRequireHttpsForLocalRequest()
        {
            HttpActionContext httpActionContext = CreateHttpActionContext("http://localhost/api/test", true);
            new RequireHttpsNonLocal().OnAuthorization(httpActionContext);

            httpActionContext.Response.Should().BeNull();
        }

        [Fact]
        public void ShouldDoNothingWhenRemoteHttpsRequest()
        {
            HttpActionContext httpActionContext = CreateHttpActionContext("https://localhost/api/test", false);
            new RequireHttpsNonLocal().OnAuthorization(httpActionContext);

            httpActionContext.Response.Should().BeNull();
        }

        private static HttpActionContext CreateHttpActionContext(string uri, bool isLocal)
        {
            return new HttpActionContext
            {
                ControllerContext = new HttpControllerContext
                {
                    Request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(uri)
                    },
                    RequestContext = new HttpRequestContext()
                    {
                        IsLocal = isLocal
                    }
                }
            };
        }

     

    }
}
