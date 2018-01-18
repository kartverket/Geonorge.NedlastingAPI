namespace Kartverket.Geonorge.Download.Services.Auth
{
    public interface IBasicAuthenticationCredentialValidator
    {
        /// <summary>
        /// Validate credentials received from a http request with basic authentication header
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        bool ValidCredentials(Credentials credentials);

    }
}