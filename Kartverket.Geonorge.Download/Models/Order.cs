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
        public int referenceNumber { get; set; }

        /// <summary>
        /// The identifier used for external referencing 
        /// Unique for all new orders, currently set to zero value guid for all old orders
        /// </summary>
        [Index("IDX_Uuid", 1, IsUnique = false)]
        public Guid Uuid { get; set; }

        [StringLength(50)]
        public string email { get; set; }

        public DateTime? orderDate { get; set; }

        public string username { get; set; }

        public virtual List<OrderItem> orderItem { get; set; }

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

            return orderItem.FirstOrDefault(o => o.FileId == fileIdAsGuid);
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

        private bool ContainsRestrictedDatasets()
        {
            return orderItem.Any(o => o.AccessConstraint.IsRestricted());
        }
    }
}