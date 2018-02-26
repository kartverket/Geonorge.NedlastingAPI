using System;
using System.Data.Entity;
using System.Reflection;
using System.Threading.Tasks;
using Kartverket.Geonorge.Download.Models;
using log4net;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IFileService
    {
        Task<Dataset> GetDatasetAsync(string metadataUuid);
        Task<File> GetFileAsync(string fileUuid);
        Task<File> GetFileAsync(string fileUuid, string metadataUuid);
    }

    /// <summary>
    ///     Basic file services on dataset and file objects.
    /// </summary>
    public class FileService : IFileService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DownloadContext _context;

        public FileService(DownloadContext context)
        {
            _context = context;
        }

        /// <summary>
        ///     Retrieve a dataset based on metadata uuid.
        /// </summary>
        /// <param name="metadataUuid"></param>
        /// <returns></returns>
        public async Task<Dataset> GetDatasetAsync(string metadataUuid)
        {
            return await _context.Capabilities.FirstOrDefaultAsync(x => x.MetadataUuid == metadataUuid);
        }

        /// <summary>
        ///     Retreive a file based on uuid.
        /// </summary>
        /// <param name="fileUuid"></param>
        /// <returns></returns>
        public async Task<File> GetFileAsync(string fileUuid)
        {
            var fileGuid = Guid.Parse(fileUuid);
            return await _context.FileList
                .Include(f => f.Dataset)
                .FirstOrDefaultAsync(x => x.Id == fileGuid);
        }

        /// <summary>
        ///     Retrieve a file based on uuid. Checks that the file belongs to the given dataset referenced by the metadata uuid.
        /// </summary>
        /// <param name="fileUuid"></param>
        /// <param name="metadataUuid"></param>
        /// <returns>Returns null if not found or if file does not belong to the given dataset</returns>
        public async Task<File> GetFileAsync(string fileUuid, string metadataUuid)
        {
            var file = await GetFileAsync(fileUuid);

            if (file.Dataset.MetadataUuid != metadataUuid)
            {
                Log.Info(
                    $"Requested file does not belong to requested dataset. [fileUuid={fileUuid}] [metadataUuid={metadataUuid}]");
                return null;
            }

            return file;
        }
    }
}