using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Web.Configuration;
using Geonorge.AuthLib.Common;
using Kartverket.Geonorge.Download.Models;
using log4net;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Kartverket.Geonorge.Download.Services.Auth
{
    public class GeoIdAuthentication : IGeoIdAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public GeoIdAuthentication(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public AuthenticatedUser GetAuthenticatedUser()
        {
            var username = ClaimsPrincipal.Current.GetUsername();

            if (string.IsNullOrEmpty(username))
                return null;

            IEnumerable<Claim> roles = ClaimsPrincipal.Current.FindAll(GeonorgeAuthorizationService.ClaimIdentifierRole);
            var rolesAsList = roles.Select(r => r.Value).ToList();
            UserInfo userInfo = new UserInfo();
            userInfo._roles = rolesAsList;

            return new AuthenticatedUser(username, AuthenticationMethod.GeoId, userInfo);
        }

        public AuthenticatedUser GetAuthenticatedUser(HttpRequestMessage requestMessage)
        {

            string accessToken = GetAccessTokenFromHeader(requestMessage);
            if (accessToken != null)
            {
                var geoIdIntrospectionUrl = WebConfigurationManager.AppSettings["GeoID:IntrospectionUrl"];
                var geoIdIntrospectionCredentials = WebConfigurationManager.AppSettings["GeoID:IntrospectionCredentials"];
                
                Log.Debug("Token validation - requestUrl: " + geoIdIntrospectionUrl);
                
                var byteArray = Encoding.ASCII.GetBytes(geoIdIntrospectionCredentials);
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var formUrlEncodedContent = new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("token", accessToken) });
                HttpResponseMessage result = _httpClient.PostAsync(geoIdIntrospectionUrl, formUrlEncodedContent).Result;

                Log.Debug("Token validation status code: " + result.StatusCode);

                if (result.IsSuccessStatusCode)
                {
                    var rawResponse = result.Content.ReadAsStringAsync().Result;
                    
                    Log.Debug("Response from introspection api: " + rawResponse);

                    var jsonResponse = JObject.Parse(rawResponse);
                    if (jsonResponse.ContainsKey("active"))
                    {
                        var isActiveToken = jsonResponse["active"].Value<bool>();

                        Log.Info($"Token [{accessToken}] is active: " + isActiveToken);

                        if (isActiveToken)
                        {
                            if (jsonResponse.ContainsKey("username"))
                            {
                                var username = jsonResponse["username"].Value<string>();
                                if (!string.IsNullOrWhiteSpace(username))
                                {
                                    if(username.Contains('@'))
                                        username = username.Split('@')[0];

                                    var roles = GetRolesForUser(username, accessToken);

                                    return new AuthenticatedUser(username, AuthenticationMethod.GeoId, roles);
                                }
                            }
                               
                        }
                    }
                    else
                    {
                        Log.Info("Response from introspection api: " + rawResponse);
                    }

                }
                else
                {
                    Log.Error("Error while validating user access token at url=" + geoIdIntrospectionUrl);
                }
            }

            string userName = "";
            if (ClaimsPrincipal.Current != null)
                userName = ClaimsPrincipal.Current.GetUsername();

            if (!string.IsNullOrEmpty(userName))
                return new AuthenticatedUser(userName, AuthenticationMethod.GeoId);

            return null;
        }

        private UserInfo GetRolesForUser(string username, string accessToken)
        {
            UserInfo userInfo = new UserInfo();

            var geoIdUserInfoUrl = WebConfigurationManager.AppSettings["GeoID:BaatAuthzApiUrl"] + "info/" + username;

            Log.Debug("User role info - requestUrl: " + geoIdUserInfoUrl);

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage result = _httpClient.GetAsync(geoIdUserInfoUrl).Result;
            Log.Debug("User role info status code: " + result.StatusCode);

            if (result.IsSuccessStatusCode)
            {
                var rawResponse = result.Content.ReadAsStringAsync().Result;

                Log.Debug("Response from baat info api: " + rawResponse);

                var jsonResponse = JObject.Parse(rawResponse);
                if (jsonResponse.ContainsKey("baat_services") && jsonResponse.ContainsKey("baat_organization"))
                {
                    var organizationInfo = jsonResponse["baat_organization"];
                    var orgnr = organizationInfo["orgnr"];
                    if (orgnr != null)
                        userInfo.OrganizationNumber = orgnr.ToString();

                    userInfo._roles = jsonResponse["baat_services"].ToObject<List<string>>();

                    return userInfo;
                }
                else
                {
                    Log.Info("Response from baat info api: " + rawResponse);
                }

            }

            return null;
        }

        private static string GetAccessTokenFromHeader(HttpRequestMessage requestMessage)
        {
            var authorizationHeader = requestMessage.Headers.Authorization;
            if (authorizationHeader != null)
            {
                if (!string.IsNullOrWhiteSpace(authorizationHeader.Parameter))
                {
                    return authorizationHeader.Parameter;
                }
            }

            var queryStrings = requestMessage.GetQueryNameValuePairs();
            var accessToken = queryStrings.Where(queryString => queryString.Key == "access_token")
                .Select(query => new { query.Key, query.Value }).FirstOrDefault();
            if (accessToken != null)
                return accessToken.Value;

            return null;
        }
    }
}