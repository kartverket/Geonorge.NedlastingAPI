using System;
using System.Reflection;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Geonorge.Nedlasting.Core;
using Geonorge.Nedlasting.Core.Search;
using log4net;
using Nest;

namespace Geonorge.Nedlasting.Infrastructure.Search
{
    public class ElasticDocumentIndexer : IDocumentIndexer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly ElasticClient _client;
        
        public ElasticDocumentIndexer(AppSettings appSettings)
        {
            var settings = new ConnectionSettings(new Uri(appSettings.ElasticSearchHostname)).DefaultIndex(appSettings.ElasticSearchIndexName);
            
            _client = new ElasticClient(settings);
        }
        
        public async Task Index(Document document)
        {
            Log.Debug("Indexing document with title: " +  document.Title);
            
            var asyncIndexResponse = await _client.IndexDocumentAsync(document);
            
            Log.Debug("Response from indexing request: " + asyncIndexResponse.Result.GetStringValue());
        }
    }
}