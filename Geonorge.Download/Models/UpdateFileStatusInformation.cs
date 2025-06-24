using System;

namespace Geonorge.Download.Models
{
    public class UpdateFileStatusInformation
    {
        public string FileId { get; set; }
        public OrderItemStatus Status { get; set; }
        public string DownloadUrl { get; set; }
        public string Message { get; set; }

        public Guid FileIdAsGuid => Guid.Parse(FileId);
    }
}