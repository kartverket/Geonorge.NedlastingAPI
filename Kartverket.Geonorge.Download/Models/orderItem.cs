using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Geonorge.Download.Models
{
    [Table("orderItem")]
    public class OrderItem
    {
        public OrderItem()
        {
            Status = OrderItemStatus.WaitingForProcessing;
        }

        [Key]
        public int Id { get; set; }

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

        public virtual orderDownload OrderDownload { get; set; }

        /// <summary>
        /// Status of the order item. Possible values are WaitingForProcessing, ReadyToDownload and Error
        /// </summary>
        public OrderItemStatus Status { get; set; }

        /// <summary>
        /// Contains message from processing service (currently FME)
        /// </summary>
        public string Message { get; set; }
    }
}