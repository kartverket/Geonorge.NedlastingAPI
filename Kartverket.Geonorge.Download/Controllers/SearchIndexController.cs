using System.Threading.Tasks;
using System.Web.Mvc;
using Geonorge.Nedlasting.Core.Search;
using Kartverket.Geonorge.Utilities;

namespace Kartverket.Geonorge.Download.Controllers
{
    //[BaatAuthorization(Role = SecurityClaim.Role.MetadataAdmin)]
    public class SearchIndexController : Controller
    {
        private readonly IFeedIndexer _feedIndexer;

        public SearchIndexController(IFeedIndexer feedIndexer)
        {
            _feedIndexer = feedIndexer;
        }
        
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> StartIndexing()
        {
            await _feedIndexer.Index(new AtomFeed() {Url = "https://nedlasting.geonorge.no/geonorge/Tjenestefeed.xml"});
            
            return RedirectToAction(nameof(Index));
        }
        
    }
}