using System.Web.Mvc;
using Kartverket.Geonorge.Download.Services;

namespace Kartverket.Geonorge.Download.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: /order/details/1337
        public ActionResult Details(int id)
        {
            var order = _orderService.Find(id);
            return order != null ? (ActionResult) View(order) : HttpNotFound();
        }
    }
}