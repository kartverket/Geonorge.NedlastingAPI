using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Geonorge.AuthLib.Common;
using Geonorge.Download.Models;
using Geonorge.Download.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Geonorge.Download.Services
{
    /// <summary>
    ///     Basic file services on dataset and file objects.
    /// </summary>
    public class FileService(ILogger<FileService> logger, DownloadContext downloadContext) : IFileService
    {
        /// <summary>
        ///     Retrieve a dataset based on metadata uuid.
        /// </summary>
        /// <param name="metadataUuid"></param>
        /// <returns></returns>
        public async Task<Dataset> GetDatasetAsync(string metadataUuid)
        {
            return await downloadContext.Capabilities.FirstOrDefaultAsync(x => x.MetadataUuid == metadataUuid);
        }

        /// <summary>
        ///     Retreive a file based on uuid.
        /// </summary>
        /// <param name="fileUuid"></param>
        /// <returns></returns>
        public async Task<Models.File> GetFileAsync(string fileUuid)
        {
            var fileGuid = Guid.Parse(fileUuid);
            return await downloadContext.FileList
                .Include(f => f.Dataset)
                .FirstOrDefaultAsync(x => x.Id == fileGuid);
        }

        /// <summary>
        ///     Retrieve a file based on uuid. Checks that the file belongs to the given dataset referenced by the metadata uuid.
        /// </summary>
        /// <param name="fileUuid"></param>
        /// <param name="metadataUuid"></param>
        /// <returns>Returns null if not found or if file does not belong to the given dataset</returns>
        public async Task<Models.File> GetFileAsync(string fileUuid, string metadataUuid)
        {
            var file = await GetFileAsync(fileUuid);

            if (file?.Dataset.MetadataUuid != metadataUuid)
            {
                logger.LogInformation(
                    $"Requested file does not belong to requested dataset. [fileUuid={fileUuid}] [metadataUuid={metadataUuid}]");
                return null;
            }

            return file;
        }

        public bool HasAccess(Models.File file, AuthenticatedUser authenticatedUser)
        {
            var datasetAccessConstraintRequiredRole = file.Dataset.AccessConstraintRequiredRole;
            var fileAccessConstraintRequiredRole = file.AccessConstraintRequiredRole;

            if (!authenticatedUser.HasRole(GeonorgeRoles.MetadataAdmin))
            {
                if (!string.IsNullOrEmpty(datasetAccessConstraintRequiredRole) || !string.IsNullOrEmpty(fileAccessConstraintRequiredRole))
                {
                    bool access = false;

                    List<string> datasetAccessConstraintRequiredRoles = ParseRoles(datasetAccessConstraintRequiredRole);

                    foreach (var requiredRole in datasetAccessConstraintRequiredRoles)
                        if (authenticatedUser.HasRole(requiredRole))
                            access = true;

                    if (!string.IsNullOrEmpty(fileAccessConstraintRequiredRole))
                    {
                        access = false;
                        List<string> fileAccessConstraintRequiredRoles = ParseRoles(fileAccessConstraintRequiredRole);
                        foreach (var requiredRole in fileAccessConstraintRequiredRoles)
                            if (authenticatedUser.HasRole(requiredRole))
                                access = true;
                    }

                    return access;
                }
            }

            return true;
        }

        private List<string> ParseRoles(string requiredRoles)
        {
            List<string> roles = new List<string>();

            if (!string.IsNullOrEmpty(requiredRoles))
            {
                var rolesRequired = requiredRoles.Split(',').Select(r => r.Trim()).ToList();
                foreach (var role in rolesRequired)
                    roles.Add(role);
            }


            return roles;
        }
    }
}