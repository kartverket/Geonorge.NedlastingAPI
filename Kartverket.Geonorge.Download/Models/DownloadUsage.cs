using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Kartverket.Geonorge.Download.Models
{
    public class DownloadUsage
    {
        public List<DownloadUsageEntry> Entries { get; } = new List<DownloadUsageEntry>();

        public void AddEntry(DownloadUsageEntry entry) => Entries.Add(entry);
    }

    [Table("DownloadUsage")]
    public class DownloadUsageEntry
    {
        [Key]
        public int Id { get; set; } 
        
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Metadata uuid
        /// </summary>
        public string Uuid { get; set; }

        /// <summary>
        /// http://register.dev.geonorge.no/metadata-kodelister/brukergrupper
        /// </summary>
        public string Group { get; set; }

        // We are saving the list of purpose code values into a single column in the database to avoid the need for any
        // relations
        [Column("Purpose")] private string PurposeAsStrings { get; set; }

        /// <summary>
        /// http://register.dev.geonorge.no/metadata-kodelister/formal
        /// </summary>
        public IEnumerable<string> Purpose
        {
            get => PurposeAsStrings != null ? PurposeAsStrings.Split(',').ToList() : new List<string>();
            set => PurposeAsStrings = string.Join(",", value);
        }

        public string SoftwareClientVersion { get; set; }

        public string SoftwareClient { get; set; }

        public DownloadUsageEntry(string uuid, string group, IEnumerable<string> purpose)
        {
            Uuid = uuid;
            Group = group;
            Purpose = purpose;
            Timestamp = DateTime.Now;
        }

        public DownloadUsageEntry(string metadataUuid, string userGroup, string[] purpose, string softwareClient,
            string softwareClientVersion) : this(metadataUuid, userGroup, purpose)
        {
            SoftwareClient = softwareClient;
            SoftwareClientVersion = softwareClientVersion;
        }
    }
}