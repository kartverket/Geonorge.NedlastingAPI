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

        public AuthenticatedUser(string username, AuthenticationMethod method, List<string> roles)
        {
            Username = username;
            AuthenticationMethod = method;
            if(roles != null && roles.Count > 0)
                _roles.AddRange(roles);
        }

        public string Username { get; }

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