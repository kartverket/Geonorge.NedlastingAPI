using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Geonorge.Download.Models.Postgres
{
    public class DownloadUsage
    {
        public Guid RequestId { get;set;}

        public DownloadUsage()
        {
            RequestId = Guid.NewGuid();
        }

        public List<DownloadUsageEntry> Entries { get; } = new List<DownloadUsageEntry>();

        public void AddEntry(DownloadUsageEntry entry) {
            entry.RequestId = RequestId;
            Entries.Add(entry);
        }
    }

    [Table("DownloadUsage")]
    public class DownloadUsageEntry
    {
        [Key]
        public int Id { get; set; } 
        
        public Guid RequestId { get;set;}

        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Metadata uuid
        /// </summary>
        public string? Uuid { get; set; }

        public string? AreaCode { get; set; }
        
        public string? AreaName { get; set; }
        
        public string? Format { get; set; }
        
        public string? Projection { get; set; }
        
        /// <summary>
        /// http://register.dev.geonorge.no/metadata-kodelister/brukergrupper
        /// </summary>
        public string? Group { get; set; }

        // We are saving the list of purpose code values into a single column in the database to avoid the need for any
        // relations
        [Column("Purpose")] private string? PurposeAsStrings { get; set; }

        /// <summary>
        /// http://register.dev.geonorge.no/metadata-kodelister/formal
        /// </summary>
        [NotMapped]
        public IEnumerable<string> Purpose
        {
            get => PurposeAsStrings?.Split(';') ?? Enumerable.Empty<string>();
            set => PurposeAsStrings = value != null ? string.Join(";", value) : null;
        }

        public string? SoftwareClientVersion { get; set; }

        public string? SoftwareClient { get; set; }

        public DownloadUsageEntry()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}