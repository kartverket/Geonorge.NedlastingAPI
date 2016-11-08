using System.Web.Mvc;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;

namespace Kartverket.Geonorge.Download.Controllers
{
    public class OrderController : Controller
    {
        // GET: /order/details/1307
        public ActionResult Details(int id)
        {
            var order = new OrderServiceV2(new DownloadContext()).Find(id);
            return order != null ? (ActionResult) View(order) : HttpNotFound();
        }
    }
}
