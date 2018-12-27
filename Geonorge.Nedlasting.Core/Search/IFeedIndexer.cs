using System.Threading.Tasks;

namespace Geonorge.Nedlasting.Core.Search
{
    public interface IFeedIndexer
    {
        Task Index(AtomFeed feed);
    }
}