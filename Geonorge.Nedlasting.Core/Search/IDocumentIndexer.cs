using System.Threading.Tasks;

namespace Geonorge.Nedlasting.Core.Search
{
    public interface IDocumentIndexer
    {
        Task Index(Document document);
    }
}