using System.Collections.Generic;

namespace Kartverket.Geonorge.Download.Models
{
    public class DownloadUsage
    {
        public List<DownloadUsageEntry> Entries { get; } = new List<DownloadUsageEntry>();

        public void AddEntry(DownloadUsageEntry entry) => Entries.Add(entry);
    }

    public class DownloadUsageEntry
    {
        /// <summary>
        /// Metadata uuid
        /// </summary>
        public string Uuid { get; }

        /// <summary>
        /// http://register.dev.geonorge.no/metadata-kodelister/brukergrupper
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// http://register.dev.geonorge.no/metadata-kodelister/formal
        /// </summary>
        public string Purpose { get; }

        public DownloadUsageEntry(string uuid, string group, string purpose)
        {
            Uuid = uuid;
            Group = group;
            Purpose = purpose;
        }
    }
}