using Geonorge.NedlastingApi.V3;
using System.Collections.Generic;

namespace Kartverket.Geonorge.Download.Controllers.Api.V2
{
    internal class OrderMapper
    {
        internal OrderType ConvertToV3(global::Geonorge.NedlastingApi.V2.OrderType inputOrder)
        {
            return new OrderType()
            {
                email = inputOrder.email,
                orderLines = ConvertToV3(inputOrder.orderLines)
            };
        }

        private OrderLineType[] ConvertToV3(global::Geonorge.NedlastingApi.V2.OrderLineType[] orderLines)
        {
            var convertedOrderLines = new List<OrderLineType>();
            foreach (var inputOrderLine in orderLines)
            {
                convertedOrderLines.Add(new OrderLineType
                {
                    metadataUuid = inputOrderLine.metadataUuid,
                    areas = ConvertToV3(inputOrderLine.areas),
                    coordinates = inputOrderLine.coordinates,
                    coordinatesystem = inputOrderLine.coordinatesystem,
                    formats = ConvertToV3(inputOrderLine.formats),
                    projections = ConvertToV3(inputOrderLine.projections)
                });
            }
            return convertedOrderLines.ToArray();
        }

        private ProjectionType[] ConvertToV3(global::Geonorge.NedlastingApi.V2.ProjectionType[] inputProjections)
        {
            var projections = new List<ProjectionType>();
            foreach(var inputProjection in inputProjections)
            {
                projections.Add(new ProjectionType
                {
                    code = inputProjection.code,
                    codespace = inputProjection.codespace,
                    name = inputProjection.name
                });
            }
            return projections.ToArray();
        }

        private FormatType[] ConvertToV3(global::Geonorge.NedlastingApi.V2.FormatType[] inputFormats)
        {
            var formats = new List<FormatType>();
            foreach(var inputFormat in inputFormats)
            {
                formats.Add(new FormatType
                {
                    name = inputFormat.name,
                    version = inputFormat.version,
                });
            }
            return formats.ToArray();
        }

        private OrderAreaType[] ConvertToV3(global::Geonorge.NedlastingApi.V2.OrderAreaType[] inputAreas)
        {
            var areas = new List<OrderAreaType>();
            foreach (var inputArea in inputAreas)
            {
                areas.Add(new OrderAreaType
                {
                    code = inputArea.code,
                    name = inputArea.name,
                    type = inputArea.type
                });
            }
            return areas.ToArray();
        }

    }
}