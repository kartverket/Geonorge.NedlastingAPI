using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Kartverket.Geonorge.Download.Models
{
    [Table("orderDownload")]
    public class Order
    {
        public Order()
        {
            orderItem = new List<OrderItem>();
            orderDate = DateTime.Now;
            Uuid = Guid.NewGuid();
        }

        /// <summary>
        /// this used to be the external identifier
        /// deprecated in favour of the Uuid property
        /// this is still the primary key in the database
        /// </summary>
        [Key]
        [Column("referenceNumber")]
        public int referenceNumber { get; set; }

        /// <summary>
        /// The identifier used for external referencing 
        /// Unique for all new orders, currently set to zero value guid for all old orders
        /// </summary>
        [Index("IDX_Uuid", 1, IsUnique = false)]
        public Guid Uuid { get; set; }

        [StringLength(50)]
        [Column("email")]
        public string email { get; set; }

        [Column("orderDate")]
        public DateTime? orderDate { get; set; }

        [Column("username")]
        public string username { get; set; }

        public virtual List<OrderItem> orderItem { get; set; }

        /// <summary>
        /// Set to true if the client has requested that the order items should be bundled together.
        /// </summary>
        public bool DownloadAsBundle { get; set; }

        /// <summary>
        /// The url to the bundle of all order items in this order. Can be null until the bundle has been created.
        /// </summary>
        public string DownloadBundleUrl { get; set; }

        /// <summary>
        /// Timestamp of when an email notification with bundle download url was sent to user.
        /// </summary>
        public DateTime? DownloadBundleNotificationSent { get; set; }

        public void AddOrderItems(List<OrderItem> items)
        {
            orderItem.AddRange(items);
        }

        public OrderItem GetItemWithFileId(string fileId)
        {
            Guid fileIdAsGuid;

            var parseResult = Guid.TryParse(fileId, out fileIdAsGuid);
            if (!parseResult)
                return null;

            return orderItem.FirstOrDefault(o => o.Uuid == fileIdAsGuid);
        }

        public void AddAccessConstraints(List<DatasetAccessConstraint> accessConstraints)
        {
            foreach (var item in orderItem)
            {
                var datasetAccessConstraint = accessConstraints.FirstOrDefault(c => c.MetadataUuid == item.MetadataUuid);
                if (datasetAccessConstraint != null)
                    item.AccessConstraint = datasetAccessConstraint.AccessConstraint;
            }
        }

        /// <summary>
        /// Restricted datasets require registered username in order and the username must be the same as the requesting username
        /// </summary>
        /// <param name="requestUsername"></param>
        /// <returns></returns>
        public bool CanBeDownloadedByUser(string requestUsername)
        {
            if (ContainsRestrictedDatasets() && !BelongsToUser(requestUsername))
                return false;

            return true;
        }

        public bool BelongsToUser(string requestUsername)
        {
            return string.Equals(username, requestUsername, StringComparison.CurrentCultureIgnoreCase);
        }

        public bool ContainsRestrictedDatasets()
        {
            return orderItem.Any(o => o.AccessConstraint.IsRestricted());
        }

        /// <summary>
        /// Bundling can work on both pre-generated files referenced by FileUuid in Filliste
        /// and on OrderItem.Uuid if the client has requested a clippable area of a dataset.
        /// </summary>
        /// <returns></returns>
        public List<string> CollectFileIdsForBundling()
        {
            var ids = new List<string>();
            foreach (var item in orderItem)
            {
                ids.Add(item.CollectIdForBundling());
            }
            return ids;
        }

        public bool IsReadyForBundleDownload()
        {
            return DownloadAsBundle && orderItem.All(item => item.IsReadyForDownload());
        }
    }
}