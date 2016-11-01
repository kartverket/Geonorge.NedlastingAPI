using System;

namespace Kartverket.Geonorge.Download.Models.Api.Internal
{
    /// <summary>
    /// Provides updated information about a file.
    /// </summary>
    public class UpdateFileStatusRequest
    {
        /// <summary>
        /// The file id
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// Valid values: WaitingForProcessing, ReadyForDownload, Error
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Url to the processed file
        /// </summary>
        public string DownloadUrl { get; set; }

        /// <summary>
        /// Optional message, should be provided for errors.
        /// </summary>
        public string Message { get; set; }
       
    }
}