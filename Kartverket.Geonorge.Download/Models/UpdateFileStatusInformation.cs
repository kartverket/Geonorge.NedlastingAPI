using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Models
{
    public class UpdateFileStatusInformation
    {
        public int FileId { get; set; }

        public OrderItemStatus Status { get; set; }
        public string DownloadUrl { get; set; }
        public string Message { get; set; }

    }
}