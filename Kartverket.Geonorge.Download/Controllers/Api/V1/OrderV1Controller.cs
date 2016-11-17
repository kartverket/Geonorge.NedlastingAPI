using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using Geonorge.NedlastingApi.V1;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;

namespace Kartverket.Geonorge.Download.Controllers.Api.V1
{
    public class OrderV1Controller : ApiController
    {
        private readonly IOrderService _orderService;

        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public OrderV1Controller(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Creates new order for data to download
        /// </summary>
        /// <param name="order">OrderType model</param>
        /// <returns>OrderReceiptType model with orderreference and a list of files to download if they are prepopulated, otherwise the files are delivered via email</returns>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("api/order")]
        [HttpPost]
        [ResponseType(typeof(OrderReceiptType))]
        public IHttpActionResult PostOrder(OrderType order)
        {
            try
            {
                global::Geonorge.NedlastingApi.V2.OrderType convertedOrder = ConvertFromVersion1ToVersion2(order);

                // no support for authentication in version 1 of api - username is always null
                // order requests for restricted datasets will always throw AccessRestrictionException
                Order savedOrder = _orderService.CreateOrder(convertedOrder, null); 

                return Ok(ConvertToReceipt(savedOrder));
            }
            catch (AccessRestrictionException e)
            {
                Log.Info("Download Access denied", e);
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return InternalServerError(ex);
            }
        }

        private OrderReceiptType ConvertToReceipt(Order savedOrder)
        {
            return new OrderReceiptType()
            {
                referenceNumber = savedOrder.referenceNumber.ToString(),
                files = ConvertToFiles(savedOrder.orderItem)
            };
        }

        private FileType[] ConvertToFiles(List<OrderItem> orderItems)
        {
            var files = new List<FileType>();
            foreach (var item in orderItems)
            {
                files.Add(new FileType()
                {
                    name = item.FileName,
                    downloadUrl = item.DownloadUrl
                });
            }

            return files.ToArray();
        }

        private global::Geonorge.NedlastingApi.V2.OrderType ConvertFromVersion1ToVersion2(OrderType order)
        {
            return new global::Geonorge.NedlastingApi.V2.OrderType()
            {
                email = order.email,
                orderLines = ConvertFromVersion1ToVersion2(order.orderLines)
            };
        }

        private global::Geonorge.NedlastingApi.V2.OrderLineType[] ConvertFromVersion1ToVersion2(OrderLineType[] orderLines)
        {
            var convertedOrderLines = new List<global::Geonorge.NedlastingApi.V2.OrderLineType>();
            foreach (var line in orderLines)
            {
                convertedOrderLines.Add(new global::Geonorge.NedlastingApi.V2.OrderLineType()
                {
                    areas = ConvertFromVersion1ToVersion2(line.areas),
                    formats = ConvertFromVersion1ToVersion2(line.formats),
                    coordinates = line.coordinates,
                    coordinatesystem = line.coordinatesystem,
                    metadataUuid = line.metadataUuid,
                    projections = ConvertFromVersion1ToVersion2(line.projections)
                });
            }
            return convertedOrderLines.ToArray();
        }

        private global::Geonorge.NedlastingApi.V2.ProjectionType[] ConvertFromVersion1ToVersion2(ProjectionType[] projections)
        {
            var convertedProjections = new List<global::Geonorge.NedlastingApi.V2.ProjectionType>();
            foreach (ProjectionType projection in projections)
            {
                convertedProjections.Add(new global::Geonorge.NedlastingApi.V2.ProjectionType()
                {
                    name = projection.name,
                    code = projection.code,
                    codespace = projection.codespace
                });
            }
            return convertedProjections.ToArray();
        }

        private global::Geonorge.NedlastingApi.V2.FormatType[] ConvertFromVersion1ToVersion2(FormatType[] formats)
        {
            var convertedFormats = new List<global::Geonorge.NedlastingApi.V2.FormatType>();
            foreach (FormatType format in formats)
            {
                convertedFormats.Add(new global::Geonorge.NedlastingApi.V2.FormatType()
                {
                    name = format.name,
                    version = format.version
                });
            }
            return convertedFormats.ToArray();
        }

        private global::Geonorge.NedlastingApi.V2.OrderAreaType[] ConvertFromVersion1ToVersion2(OrderAreaType[] areas)
        {
            var convertedAreas = new List<global::Geonorge.NedlastingApi.V2.OrderAreaType>();
            foreach (OrderAreaType area in areas)
            {
                convertedAreas.Add(new global::Geonorge.NedlastingApi.V2.OrderAreaType()
                {
                    code = area.code,
                    name = area.name,
                    type = area.type
                });
            }
            return convertedAreas.ToArray();
        }
    }
}