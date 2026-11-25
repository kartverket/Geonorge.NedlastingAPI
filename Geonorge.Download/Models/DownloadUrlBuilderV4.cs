namespace Geonorge.Download.Models
{
    /// <summary>
    /// Builds download URLs for the V4 API.
    /// V4 uses the path structure: api/v4/orders/{orderUuid}/{fileId}
    /// </summary>
    public class DownloadUrlBuilderV4(IConfiguration config)
    {
        private string _orderUuid;
        private string _fileId;

        public DownloadUrlBuilderV4 OrderId(Guid orderUuid)
        {
            _orderUuid = orderUuid.ToString();
            return this;
        }

        public DownloadUrlBuilderV4 FileId(Guid fileId)
        {
            _fileId = fileId.ToString();
            return this;
        }

        public string Build()
        {
            return BaseUrl() + _fileId;
        }

        public string AsBundle()
        {
            return BaseUrl() + "bundle";
        }

        private string BaseUrl()
        {
            var baseUrl = config["DownloadUrl"] ?? throw new InvalidOperationException("DownloadUrl configuration is missing");
            return $"{baseUrl}api/v4/orders/{_orderUuid}/";
        }
    }
}
