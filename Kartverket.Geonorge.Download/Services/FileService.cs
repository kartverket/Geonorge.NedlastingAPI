using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IFileService
    {
        Task<Dataset> GetDatasetAsync(string metadataUuid);
        Task<File> GetFileAsync(string fileUuid);
    }

    public class FileService : IFileService
    {
        private readonly DownloadContext _context;

        public FileService(DownloadContext context)
        {
            _context = context;
        }

        public Task<Dataset> GetDatasetAsync(string metadataUuid)
        {
            return _context.Capabilities.FirstOrDefaultAsync(x => x.MetadataUuid == metadataUuid);
        }

        public Task<File> GetFileAsync(string fileUuid)
        {
            return _context.FileList.FirstOrDefaultAsync(x => x.Id == Guid.Parse(fileUuid));
        }
    }
}