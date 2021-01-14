using System.Collections.Generic;

namespace Kartverket.Geonorge.Download.Models
{
    public class AuthenticatedUser
    {
        public AuthenticatedUser(string username, AuthenticationMethod method)
        {
            Username = username;
            AuthenticationMethod = method;
        }

        public AuthenticatedUser(string username, AuthenticationMethod method, UserInfo userInfo)
        {
            Username = username;
            AuthenticationMethod = method;
            if(userInfo._roles != null && userInfo._roles.Count > 0)
                _roles.AddRange(userInfo._roles);
            OrganizationNumber = userInfo.OrganizationNumber;
        }

        public string Username { get; }

        public string OrganizationNumber { get; }

        private AuthenticationMethod AuthenticationMethod { get; }

        private List<string> _roles  = new List<string>();

        public string UsernameForStorage()
        {
            if (AuthenticationMethod == AuthenticationMethod.Basic)
                return "local_" + Username;

            return Username;
        }

        public bool HasRole(string role)
        {
            if (_roles != null && _roles.Count > 0)
                return _roles.Contains(role);
            else
                return false;
        }

        public bool IsAuthorizedWith(AuthenticationMethod method)
        {
            return AuthenticationMethod == method;
        }
    }

    public enum AuthenticationMethod
    {
        GeoId, Basic
    }
}