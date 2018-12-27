using System.Threading.Tasks;
using CodeHollow.FeedReader;
using Geonorge.FeedReader;
using Geonorge.Nedlasting.Core.Search;

namespace Geonorge.Nedlasting.Infrastructure.Search
{
    public class AtomFeedIndexer : IFeedIndexer
    {
        private readonly IDocumentIndexer _documentIndexer;

        public AtomFeedIndexer(IDocumentIndexer documentIndexer)
        {
            _documentIndexer = documentIndexer;
        }

        public async Task Index(AtomFeed feed)
        {
            Feed parsedRootFeed = await CodeHollow.FeedReader.FeedReader.ReadAsync(feed.Url);

            foreach (var item in parsedRootFeed.Items)
            {
                await _documentIndexer.Index(new Document()
                {
                    Title = item.Title,
                    Description = item.Description,
                    Epsg = item.GetGeonorgeFeedItem().Epsg?.Value
                });
            }
           
        }
    }
}