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
            return _roles.Contains(role);
        }
    }

    public enum AuthenticationMethod
    {
        Baat, Basic
    }
}