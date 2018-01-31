using System.Web.Mvc;
using System.Web.Mvc.Filters;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services.Auth;
using Kartverket.Geonorge.Utilities;

namespace Kartverket.Geonorge.Download.Controllers
{
    public class BaatAuthorizationAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        public IBaatAuthenticationService BaatAuthenticationService { get; set; }

        public string Role { get; set; }

        public void OnAuthentication(AuthenticationContext filterContext)
        {
            var authenticatedUser = BaatAuthenticationService.GetAuthenticatedUser();
            if (authenticatedUser == null
                || !authenticatedUser.IsAuthorizedWith(AuthenticationMethod.Baat)
                || !authenticatedUser.HasRole(SecurityClaim.Role.MetadataAdmin))
                filterContext.Result = new HttpUnauthorizedResult(); 
            
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
                
        }
    }
}