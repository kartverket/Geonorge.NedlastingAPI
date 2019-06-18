using System.Web.Mvc;
using System.Web.Mvc.Filters;
using Geonorge.AuthLib.Common;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services.Auth;
using Kartverket.Geonorge.Utilities;

namespace Kartverket.Geonorge.Download.Controllers
{
    public class GeoIdAuthorizationAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        public IGeoIdAuthenticationService GeoIdAuthenticationService { get; set; }

        public string Role { get; set; }

        public void OnAuthentication(AuthenticationContext filterContext)
        {
            var authenticatedUser = GeoIdAuthenticationService.GetAuthenticatedUser();
            if (authenticatedUser == null
                || !authenticatedUser.IsAuthorizedWith(AuthenticationMethod.GeoId)
                || !authenticatedUser.HasRole(Role))
                filterContext.Result = new HttpUnauthorizedResult(); 
            
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
                
        }
    }
}