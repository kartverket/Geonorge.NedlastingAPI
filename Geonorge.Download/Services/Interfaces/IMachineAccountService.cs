using Geonorge.Download.Models;

namespace Geonorge.Download.Services.Interfaces
{
    // Services/IMachineAccountService.cs
    

    public interface IMachineAccountService
    {
        Task<List<MachineAccountCreate_VM>> ListAsync();
        Task CreateAsync(MachineAccountCreate_VM account, CancellationToken ct = default);
        Task UpdateAsync(MachineAccountEdit_VM account, CancellationToken ct = default);
        Task<MachineAccountCreate_VM?> GetByUsernameAsync(string username, CancellationToken ct = default);
        Task DeleteAsync(string username);

        public List<string> GetAvailableRoles();
    }

}
