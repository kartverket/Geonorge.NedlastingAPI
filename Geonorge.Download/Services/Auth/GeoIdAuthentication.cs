using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading;
using Geonorge.AuthLib.Common;
using Geonorge.Download.Models;
using Geonorge.Download.Services.Interfaces;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Geonorge.Download.Services.Auth
{
    public class GeoIdAuthentication(ILogger<GeoIdAuthentication> logger, IConfiguration config, IHttpClientFactory httpClientFactory, IRegisterFetcher registerFetcher) : IGeoIdAuthenticationService
    {
        public AuthenticatedUser GetAuthenticatedUser()
        {
            if (ClaimsPrincipal.Current == null)
            {
                return null;
            }

            var username = ClaimsPrincipal.Current.GetUsername();

            if (string.IsNullOrEmpty(username))
                return null;

            IEnumerable<Claim> roles = ClaimsPrincipal.Current.FindAll(GeonorgeAuthorizationService.ClaimIdentifierRole);
            var rolesAsList = roles.Select(r => r.Value).ToList();
            UserInfo userInfo = new UserInfo();
            userInfo._roles = rolesAsList;

            return new AuthenticatedUser(username, AuthenticationMethod.GeoId, userInfo);
        }

        public AuthenticatedUser GetAuthenticatedUser(HttpRequest request)
        {

            string accessToken = GetAccessTokenFromHeader(request);
            if (accessToken != null)
            {
                var geoIdIntrospectionUrl = config["GeoID:IntrospectionUrl"];
                var geoIdIntrospectionCredentials = config["GeoID:IntrospectionCredentials"];

                var clientId = config["GeoID:ClientId"];
                var clientSecret = config["GeoID:ClientSecret"];

                logger.LogDebug("Token validation - requestUrl: " + geoIdIntrospectionUrl);
                
                var byteArray = Encoding.ASCII.GetBytes(geoIdIntrospectionCredentials);

                var httpClient = httpClientFactory.CreateClient();

                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var formUrlEncodedContent = new FormUrlEncodedContent(new[] 
                {
                    new KeyValuePair<string, string>("token", accessToken),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                });
                HttpResponseMessage result = httpClient.PostAsync(geoIdIntrospectionUrl, formUrlEncodedContent).Result;

                logger.LogDebug("Token validation status code: " + result.StatusCode);

                if (result.IsSuccessStatusCode)
                {
                    var rawResponse = result.Content.ReadAsStringAsync().Result;
                    
                    logger.LogDebug("Response from introspection api: " + rawResponse);

                    var jsonResponse = JObject.Parse(rawResponse);
                    if (jsonResponse.ContainsKey("active"))
                    {
                        var isActiveToken = jsonResponse["active"].Value<bool>();

                        logger.LogInformation($"Token [{accessToken}] is active: " + isActiveToken);

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
                        logger.LogInformation("Response from introspection api: " + rawResponse);
                    }

                }
                else
                {
                    logger.LogError("Error while validating user access token at url=" + geoIdIntrospectionUrl);
                }
            }

            var user = GetAuthenticatedUser();
            if (user != null)
                return user;

            return null;
        }

        private UserInfo GetRolesForUser(string username, string accessToken)
        {
            UserInfo userInfo = new UserInfo();

            var geoIdUserInfoUrl = config["GeoID:BaatAuthzApiUrl"] + "info/" + username;

            logger.LogDebug("User role info - requestUrl: " + geoIdUserInfoUrl);

            var httpClient = httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage result = httpClient.GetAsync(geoIdUserInfoUrl).Result;
            logger.LogDebug("User role info status code: " + result.StatusCode);

            if (result.IsSuccessStatusCode)
            {
                var rawResponse = result.Content.ReadAsStringAsync().Result;

                logger.LogDebug("Response from baat info api: " + rawResponse);

                var jsonResponse = JObject.Parse(rawResponse);
                if (jsonResponse.ContainsKey("baat_services") && jsonResponse.ContainsKey("baat_organization"))
                {
                    var organizationInfo = jsonResponse["baat_organization"];
                    var orgnr = organizationInfo["orgnr"];
                    if (orgnr != null)
                        userInfo.OrganizationNumber = orgnr.ToString();

                    userInfo._roles = jsonResponse["baat_services"].ToObject<List<string>>();

                    if (!string.IsNullOrEmpty(userInfo.OrganizationNumber)) {
                        var organization = registerFetcher.GetOrganization(userInfo.OrganizationNumber);
                        if(organization != null)
                            userInfo.MunicipalityCode = organization.MunicipalityCode;
                    }

                    return userInfo;
                }
                else
                {
                    logger.LogInformation("Response from baat info api: " + rawResponse);
                }

            }

            return null;
        }

        private static string GetAccessTokenFromHeader(HttpRequest request)
        {
            // Try parse Authorization header like in original
            if (request.Headers.TryGetValue("Authorization", out var headerValues))
            {
                var authHeader = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(headerValues.FirstOrDefault());

                if (!string.IsNullOrWhiteSpace(authHeader?.Parameter))
                {
                    return authHeader.Parameter;
                }
            }

            // Match ?access_token=... query parameter
            var accessToken = request.Query
                .Where(q => q.Key == "access_token")
                .Select(q => new { q.Key, q.Value })
                .FirstOrDefault();

            if (accessToken != null)
            {
                return accessToken.Value;
            }

            return null;
        }

    }
}