using System.Reflection;
using Kartverket.Geonorge.Download.Models;
using log4net;
using Microsoft.AspNet.Identity;
using ScottBrady91.AspNet.Identity.ConfigurablePasswordHasher;

namespace Kartverket.Geonorge.Download.Services.Auth
{
    public class BasicAuthenticationCredentialValidator : IBasicAuthenticationCredentialValidator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DownloadContext _dbContext;

        public BasicAuthenticationCredentialValidator(DownloadContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool ValidCredentials(Credentials credentials)
        {
            var account = _dbContext.MachineAccounts.Find(credentials.Username);

            if (account != null)
            {
                var passwordHasher = new ConfigurablePasswordHasher();
                var passwordVerificationResult = passwordHasher.VerifyHashedPassword(account.Passsword, credentials.Password);
                if (passwordVerificationResult == PasswordVerificationResult.Success) 
                    return true;

                Log.Info($"Basic authentication failed, bad password for username [{credentials.Username}].");
            }
            else
            {
                Log.Info($"Basic authentication failed: username [{credentials.Username}] does not exist.");
            }

            return false;
        }
    }
}