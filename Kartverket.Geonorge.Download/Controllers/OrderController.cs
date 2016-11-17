using System.Web.Mvc;
using Kartverket.Geonorge.Download.Services;
using Kartverket.Geonorge.Utilities;

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
        public ActionResult Details(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                var order = _orderService.Find(id);
                if (order != null)
                {
                    if (!order.BelongsToUser(SecurityClaim.GetUsername()))
                        return new HttpUnauthorizedResult();

                    return View(order);
                }
            }
            return HttpNotFound();
        }
    }
}