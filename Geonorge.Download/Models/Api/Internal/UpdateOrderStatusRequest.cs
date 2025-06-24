namespace Geonorge.Download.Models.Api.Internal
{
    public class UpdateOrderStatusRequest
    {
        /// <summary>
        ///     The order uuid
        /// </summary>
        public string OrderUuid { get; set; }

        /// <summary>
        /// Url to the processed file
        /// </summary>
        public string DownloadUrl { get; set; }

        /// <summary>
        ///     Valid values: WaitingForProcessing, ReadyForDownload, Error
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///     The event that has occured. Valid values: FilePackingFinished
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        ///     Optional message, should be provided for errors.
        /// </summary>
        public string Message { get; set; }
    }

    public enum UpdateOrderEventTypes
    {
        FilePackingFinished
    }
}