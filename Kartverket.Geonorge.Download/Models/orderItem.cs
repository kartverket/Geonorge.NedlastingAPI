using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Geonorge.NedlastingApi.V2;

namespace Kartverket.Geonorge.Download.Models
{
    [Table("orderItem")]
    public class OrderItem
    {
        public OrderItem()
        {
            Status = OrderItemStatus.WaitingForProcessing;
            FileId = Guid.NewGuid();
            AccessConstraint = new AccessConstraint();
        }

        [Key]
        public int Id { get; set; }

        [Index("IDX_FileId", 1, IsUnique = false)]
        public Guid FileId { get; set; }

        /// <summary>
        /// The complete url to the file to be downloaded.
        /// </summary>
        public string DownloadUrl { get; set; }

        /// <summary>
        /// FileName will be used when delivering the file to the user.
        /// </summary>
        public string FileName { get; set; }

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
        public string Message { get; set; }

        public string Format { get; set; }
        public AreaType Area { get; set; }
        public string Coordinates { get; set; }
        public string CoordinateSystem { get; set; }
        public ProjectionType Projection { get; set; }
        public string MetadataUuid { get; set; }
        public string MetadataName { get; set; }

        [NotMapped]
        public AccessConstraint AccessConstraint { get; set; }

        [NotMapped]
        public Guid MetadataUuidAsGuid => Guid.Parse(MetadataUuid);

        public bool IsReadyForDownload()
        {
            return Status == OrderItemStatus.ReadyForDownload;
        }
    }
}