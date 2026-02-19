using Geonorge.Download.Models;
using Geonorge.Download.Services.Auth;
using Geonorge.Download.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Geonorge.Download.Services
{
    public class MachineAccountService(ILogger<MachineAccountService> logger, IConfiguration config, IBasicAuthenticationCredentialValidator credentialValidator, DownloadContext context) : IMachineAccountService
    {
        public async Task<List<MachineAccountCreate_VM>> ListAsync()
        {
            List<MachineAccountCreate_VM> accountsView = [];
            var accounts = await context.MachineAccounts
                                    .AsNoTracking()
                                    .OrderBy(a => a.Username)
                                    .ToListAsync();

            foreach (var account in accounts)
            {
                var roles = account.Roles?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                accountsView.Add(new MachineAccountCreate_VM
                {
                    Username = account.Username,
                    Company = account.Company,
                    ContactPerson = account.ContactPerson,
                    ContactEmail = account.ContactEmail,
                    Created = account.Created,
                    Roles = roles ?? []
                });
            }

            return accountsView;
        }

        public async Task CreateAsync(MachineAccountCreate_VM account, CancellationToken ct = default)
        {
            logger.LogInformation("Creating machine account for {User} with roles: {Roles}", account.Username, string.Join(",", account.Roles));
            
            MachineAccount dbAccount = new()
            {
                Username = account.Username,
                Passsword = credentialValidator.HashPassword(account.Password),
                Company = account.Company,
                ContactPerson = account.ContactPerson,
                ContactEmail = account.ContactEmail,
                Created = DateTime.UtcNow,
                Roles = string.Join(',', account.Roles)
            };

            context.MachineAccounts.Add(dbAccount);
            await context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(MachineAccountEdit_VM account, CancellationToken ct = default)
        {
            var dbAccount = await context.MachineAccounts.FindAsync(account.Username, ct);
            if (dbAccount == null)
            {
                logger.LogWarning("Attempted to update non-existing machine account for {User}", account.Username);
                return;
            }
            logger.LogInformation("Updating machine account for {User} with roles: {Roles}", account.Username, string.Join(",", account.Roles));
            if (!string.IsNullOrEmpty(account.Password))
            {
                dbAccount.Passsword = credentialValidator.HashPassword(account.Password);
            }
            dbAccount.Company = account.Company;
            dbAccount.ContactPerson = account.ContactPerson;
            dbAccount.ContactEmail = account.ContactEmail;
            dbAccount.Roles = string.Join(',', account.Roles);
            context.MachineAccounts.Update(dbAccount);
            await context.SaveChangesAsync(ct);
        }

        public async Task<MachineAccountCreate_VM?> GetByUsernameAsync(string username, CancellationToken ct = default)
        {
            var account = await context.MachineAccounts
                                .AsNoTracking()
                                .FirstOrDefaultAsync(a => a.Username == username);
            if (account == null)
                return null;
            
            var roles = account.Roles?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var accountView = new MachineAccountCreate_VM
            {
                Username = account.Username,
                Company = account.Company,
                ContactPerson = account.ContactPerson,
                ContactEmail = account.ContactEmail,
                Created = account.Created,
                Roles = roles ?? []
            };

            return accountView;
        }

        public async Task DeleteAsync(string username)
        {
            var account = await context.MachineAccounts.FindAsync(username);
            if (account != null)
            {
                context.MachineAccounts.Remove(account);
                await context.SaveChangesAsync();
                logger.LogInformation("Deleted machine account for {User}", username);
            }
            else
            {
                logger.LogWarning("Attempted to delete non-existing machine account for {User}", username);
            }
        }


        public List<string> GetAvailableRoles()
        {
            List<string> roles = new List<string>();
            string rolesCSV = config["auth:BasicAuthRoles"];
            if (!string.IsNullOrEmpty(rolesCSV))
            {
                var rolesArray = rolesCSV.Split(',');
                foreach (var role in rolesArray)
                    roles.Add(role);
            }

            return roles;
        }
    }
}
