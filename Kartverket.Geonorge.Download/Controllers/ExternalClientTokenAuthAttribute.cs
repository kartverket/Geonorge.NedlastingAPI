using log4net;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Filters;

namespace Kartverket.Geonorge.Download.Controllers.Api
{
    public class ExternalClientTokenAuthAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            AuthenticationHeaderValue authorization = context.Request.Headers.Authorization;
            
            var externalClientToken = WebConfigurationManager.AppSettings["ExternalClientToken"];

            if (authorization == null || authorization.Scheme != "Bearer" || authorization.Parameter != externalClientToken)
            {
                Log.Info("Authorization failed for client: invalid credentials");
                context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", context.Request);
            }

            await Task.FromResult(0);
        }

        public async Task ChallengeAsync(HttpAuthenticationChallengeContext context,
            CancellationToken cancellationToken)
        {
            var challenge = new AuthenticationHeaderValue("Bearer");
            context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
            await Task.FromResult(0);
        }
    }

    public class AuthenticationFailureResult : IHttpActionResult
    {
        public string ReasonPhrase { get; }

        public HttpRequestMessage Request { get; }

        public AuthenticationFailureResult(string reasonPhrase, HttpRequestMessage request)
        {
            ReasonPhrase = reasonPhrase;
            Request = request;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.RequestMessage = Request;
            response.ReasonPhrase = ReasonPhrase;
            return response;
        }
    }

    public class AddChallengeOnUnauthorizedResult : IHttpActionResult
    {
        public AuthenticationHeaderValue Challenge { get; }

        public IHttpActionResult InnerResult { get; }

        public AddChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, IHttpActionResult innerResult)
        {
            Challenge = challenge;
            InnerResult = innerResult;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await InnerResult.ExecuteAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                response.Headers.WwwAuthenticate.Clear();
                if (!response.Headers.WwwAuthenticate.Any(h => h.Scheme == Challenge.Scheme))
                    response.Headers.WwwAuthenticate.Add(Challenge);

            return response;
        }
    }
}