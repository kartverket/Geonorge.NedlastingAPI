using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Geonorge.Download.Models
{
    [Table("orderItem")]
    public class OrderItem
    {
        public OrderItem()
        {
            Status = OrderItemStatus.WaitingForProcessing;
            Uuid = Guid.NewGuid();
            AccessConstraint = new AccessConstraint();
        }

        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The identifier used for external referencing to this order item
        /// Unique for all new orders, currently set to zero value guid for all old orders. See DownloadContext for the database index
        /// </summary>
        public Guid Uuid { get; set; }

        /// <summary>
        /// The id of the file in filliste. See DownloadContext for the database index
        /// </summary>
        public Guid? FileUuid { get; set; }

        /// <summary>
        /// The complete url to the file to be downloaded.
        /// </summary>
        public string? DownloadUrl { get; set; }

        /// <summary>
        /// FileName will be used when delivering the file to the user.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Reference number of the order it is associated with.
        /// </summary>
        public int ReferenceNumber { get; set; }

        public virtual Order Order { get; set; }

        /// <summary>
        /// Status of the order item. Possible values are WaitingForProcessing, ReadyToDownload and Error
        /// </summary>
        public OrderItemStatus Status { get; set; }

        /// <summary>
        /// Contains message from processing service (currently FME)
        /// </summary>
        public string? Message { get; set; }

        public string? Format { get; set; }
        public string? Area { get; set; }
        public string? AreaName { get; set; }
        public string? Coordinates { get; set; }
        public string? CoordinateSystem { get; set; }
        public string? ClipperFile { get; set; }
        public string? Projection { get; set; }
        public string? ProjectionName { get; set; }
        public string? MetadataUuid { get; set; }
        public string? MetadataName { get; set; }

        [NotMapped]
        public AccessConstraint AccessConstraint { get; set; }

        [NotMapped]
        public Guid MetadataUuidAsGuid => Guid.Parse(MetadataUuid);

        [NotMapped]
        public IEnumerable<string> UsagePurpose { get; set; } = new List<string>();
        
        public bool IsReadyForDownload()
        {
            return Status == OrderItemStatus.ReadyForDownload;
        }

        public string CollectIdForBundling()
        {
            return FileUuid?.ToString() ?? Uuid.ToString();
        }

        public DownloadUsageEntry GetDownloadUsageEntry()
        {
            return new DownloadUsageEntry()
            {
                Uuid = MetadataUuid,
                AreaCode = Area,
                AreaName = AreaName,
                Projection = Projection,
                Format = Format,
                Purpose = UsagePurpose
            };
        }

    }
}