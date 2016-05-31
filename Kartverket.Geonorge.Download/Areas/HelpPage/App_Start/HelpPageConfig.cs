// Uncomment the following to provide samples for PageResult<T>. Must also add the Microsoft.AspNet.WebApi.OData
// package to your project.
////#define Handle_PageResultOfT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using System.Web.Http;
#if Handle_PageResultOfT
using System.Web.Http.OData;
#endif

namespace Kartverket.Geonorge.Download.Areas.HelpPage
{
    /// <summary>
    /// Use this class to customize the Help Page.
    /// For example you can set a custom <see cref="System.Web.Http.Description.IDocumentationProvider"/> to supply the documentation
    /// or you can provide the samples for the requests/responses.
    /// </summary>
    public static class HelpPageConfig
    {
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "Kartverket.Geonorge.Download.Areas.HelpPage.TextSample.#ctor(System.String)",
            Justification = "End users may choose to merge this string with existing localized resources.")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly",
            MessageId = "bsonspec",
            Justification = "Part of a URI.")]
        public static void Register(HttpConfiguration config)
        {
            //// Uncomment the following to use the documentation from XML documentation file.
            config.SetDocumentationProvider(new XmlDocumentationProvider(HttpContext.Current.Server.MapPath("~/App_Data/XmlDocument.xml")));

            CapabilitiesType c = new CapabilitiesType();
            c.supportsAreaSelection = true;
            c.supportsFormatSelection = true;
            c.supportsProjectionSelection = true;
            c.supportsPolygonSelection = false;

            LinkType l1 = new LinkType() { href = "http://download.dev.geonorge.no/api/codelists/projection/041f1e6e-bdbc-4091-b48f-8a5990f3cc5b", rel = "http://rel.geonorge.no/download/projection" };
            LinkType l2 = new LinkType() { href = "http://download.dev.geonorge.no/api/codelists/format/041f1e6e-bdbc-4091-b48f-8a5990f3cc5b", rel = "http://rel.geonorge.no/download/format" };
            LinkType l3 = new LinkType() { href = "http://download.dev.geonorge.no/api/codelists/area/041f1e6e-bdbc-4091-b48f-8a5990f3cc5b", rel = "http://rel.geonorge.no/download/area" };
            LinkType l4 = new LinkType() { href = "http://download.dev.geonorge.no/api/order", rel = "http://rel.geonorge.no/download/order" };
            List<LinkType> l = new List<LinkType>();
            l.Add(l1);
            l.Add(l2);
            l.Add(l3);
            l.Add(l4);
            c._links = l.ToArray();

            ProjectionType p1 = new ProjectionType() { code = "25832", name = "EUREF89 UTM sone 32, 2d", codespace = "http://www.opengis.net/def/crs/EPSG/0/25832" };
            ProjectionType p2 = new ProjectionType() { code = "25833", name = "EUREF89 UTM sone 33, 2d", codespace = "http://www.opengis.net/def/crs/EPSG/0/25833" };
            ProjectionType p3 = new ProjectionType() { code = "25835", name = "EUREF89 UTM sone 35, 2d", codespace = "http://www.opengis.net/def/crs/EPSG/0/25835" };
            List<ProjectionType> p = new List<ProjectionType>();
            p.Add(p1);
            p.Add(p2);
            p.Add(p3);

            FormatType f1 = new FormatType() { name = "SOSI 4.5" };
            FormatType f2 = new FormatType() { name = "GML 3.2.1" };

            List<FormatType> f = new List<FormatType>();
            f.Add(f1);
            f.Add(f2);

            AreaType a1 = new AreaType() { name = "Akershus", code= "02", type= "fylke" };
            List<ProjectionType> a1P = new List<ProjectionType>();
            a1P.Add(p1);
            a1.projections = a1P.ToArray();
            List<FormatType> a1F = new List<FormatType>();
            a1F.Add(f1);
            a1.formats = a1F.ToArray();

            AreaType a2 = new AreaType() { name = "Agdenes", code = "1622", type = "kommune" };
            List<ProjectionType> a2P = new List<ProjectionType>();
            a2P.Add(p1);
            a2P.Add(p2);
            a2.projections = a2P.ToArray();
            List<FormatType> a2F = new List<FormatType>();
            a2F.Add(f2);
            a2.formats = a2F.ToArray();

            AreaType a3 = new AreaType() { name = "Landsdekkende", code = "0000", type = "landsdekkende" };
            List<ProjectionType> a3P = new List<ProjectionType>();
            a3P.Add(p2);
            a3.projections = a3P.ToArray();
            List<FormatType> a3F = new List<FormatType>();
            a3F.Add(f1);
            a3F.Add(f2);
            a3.formats = a3F.ToArray();

            List<AreaType> a = new List<AreaType>();
            a.Add(a1);
            a.Add(a2);
            a.Add(a3);

            List<OrderAreaType> at = new List<OrderAreaType>();
            at.Add(new OrderAreaType() { name = a1.name, code = a1.code, type = a1.type });

            OrderType o = new OrderType();
            o.email = "bruker@epost.no";
            List<OrderLineType> ol = new List<OrderLineType>();
            OrderLineType ol1 = new OrderLineType();
            ol1.metadataUuid = "041f1e6e-bdbc-4091-b48f-8a5990f3cc5b";
            ol1.projections = p.ToArray();
            ol1.formats = f.ToArray();
            ol1.areas = at.ToArray();
            ol.Add(ol1);
            o.orderLines = ol.ToArray();

            OrderReceiptType or = new OrderReceiptType();
            or.referenceNumber = "O-123456789";
            FileType file = new FileType() { name = "AdministrativeEnheter_02_32.gml", downloadUrl="nedlastbarURI", fileSize = "12345" };
            FileType file2 = new FileType() { name = "AdministrativeEnheter_02_32.sos", downloadUrl = "nedlastbarURI", fileSize = "12345" };
            FileType file3 = new FileType() { name = "AdministrativeEnheter_02_33.gml", downloadUrl = "nedlastbarURI", fileSize = "12345" };
            FileType file4 = new FileType() { name = "AdministrativeEnheter_02_33.sos", downloadUrl = "nedlastbarURI", fileSize = "12345" };
            FileType file5 = new FileType() { name = "AdministrativeEnheter_02_35.gml", downloadUrl = "nedlastbarURI", fileSize = "12345" };
            FileType file6 = new FileType() { name = "AdministrativeEnheter_02_35.sos", downloadUrl = "nedlastbarURI", fileSize = "12345" };
            List<FileType> ftl = new List<FileType>();
            ftl.Add(file);
            ftl.Add(file2);
            ftl.Add(file3);
            ftl.Add(file4);
            ftl.Add(file5);
            ftl.Add(file6);
            or.files = ftl.ToArray();


            config.SetSampleObjects(new Dictionary<Type, object>
            {
                {typeof(string), "sample string"},
                {typeof(CapabilitiesType), c},
                {typeof(List<AreaType>),a },
                {typeof(OrderType),o },
                {typeof(OrderReceiptType),or },
                {typeof(List<ProjectionType>),p },
                {typeof(List<FormatType>),f }
            });


            //// Uncomment the following to use "sample string" as the sample for all actions that have string as the body parameter or return type.
            //// Also, the string arrays will be used for IEnumerable<string>. The sample objects will be serialized into different media type 
            //// formats by the available formatters.
            //config.SetSampleObjects(new Dictionary<Type, object>
            //{
            //    {typeof(string), "sample string"},
            //    {typeof(IEnumerable<string>), new string[]{"sample 1", "sample 2"}}
            //});

            // Extend the following to provide factories for types not handled automatically (those lacking parameterless
            // constructors) or for which you prefer to use non-default property values. Line below provides a fallback
            // since automatic handling will fail and GeneratePageResult handles only a single type.
#if Handle_PageResultOfT
            config.GetHelpPageSampleGenerator().SampleObjectFactories.Add(GeneratePageResult);
#endif

            // Extend the following to use a preset object directly as the sample for all actions that support a media
            // type, regardless of the body parameter or return type. The lines below avoid display of binary content.
            // The BsonMediaTypeFormatter (if available) is not used to serialize the TextSample object.
            config.SetSampleForMediaType(
                new TextSample("Binary JSON content. See http://bsonspec.org for details."),
                new MediaTypeHeaderValue("application/bson"));

            //// Uncomment the following to use "[0]=foo&[1]=bar" directly as the sample for all actions that support form URL encoded format
            //// and have IEnumerable<string> as the body parameter or return type.
            //config.SetSampleForType("[0]=foo&[1]=bar", new MediaTypeHeaderValue("application/x-www-form-urlencoded"), typeof(IEnumerable<string>));

            //// Uncomment the following to use "1234" directly as the request sample for media type "text/plain" on the controller named "Values"
            //// and action named "Put".
            //config.SetSampleRequest("1234", new MediaTypeHeaderValue("text/plain"), "Values", "Put");
            config.SetSampleRequest("api/order", new MediaTypeHeaderValue("application/x-www-form-urlencoded"), "Order", "PostOrder");

            config.SetSampleRequest("api/capabilities/041f1e6e-bdbc-4091-b48f-8a5990f3cc5b", new MediaTypeHeaderValue("application/json"), "Capabilities", "GetCapabilities");
            config.SetSampleRequest("api/codelists/projection/041f1e6e-bdbc-4091-b48f-8a5990f3cc5b", new MediaTypeHeaderValue("application/json"), "Capabilities", "GetProjections");
            config.SetSampleRequest("api/codelists/format/041f1e6e-bdbc-4091-b48f-8a5990f3cc5b", new MediaTypeHeaderValue("application/json"), "Capabilities", "GetFormats");
            config.SetSampleRequest("api/codelists/area/041f1e6e-bdbc-4091-b48f-8a5990f3cc5b", new MediaTypeHeaderValue("application/json"), "Capabilities", "GetAreas");
            //// Uncomment the following to use the image on "../images/aspNetHome.png" directly as the response sample for media type "image/png"
            //// on the controller named "Values" and action named "Get" with parameter "id".
            //config.SetSampleResponse(new ImageSample("../images/aspNetHome.png"), new MediaTypeHeaderValue("image/png"), "Values", "Get", "id");

            //// Uncomment the following to correct the sample request when the action expects an HttpRequestMessage with ObjectContent<string>.
            //// The sample will be generated as if the controller named "Values" and action named "Get" were having string as the body parameter.
            //config.SetActualRequestType(typeof(string), "Values", "Get");

            //// Uncomment the following to correct the sample response when the action returns an HttpResponseMessage with ObjectContent<string>.
            //// The sample will be generated as if the controller named "Values" and action named "Post" were returning a string.
            //config.SetActualResponseType(typeof(string), "Values", "Post");
        }

#if Handle_PageResultOfT
        private static object GeneratePageResult(HelpPageSampleGenerator sampleGenerator, Type type)
        {
            if (type.IsGenericType)
            {
                Type openGenericType = type.GetGenericTypeDefinition();
                if (openGenericType == typeof(PageResult<>))
                {
                    // Get the T in PageResult<T>
                    Type[] typeParameters = type.GetGenericArguments();
                    Debug.Assert(typeParameters.Length == 1);

                    // Create an enumeration to pass as the first parameter to the PageResult<T> constuctor
                    Type itemsType = typeof(List<>).MakeGenericType(typeParameters);
                    object items = sampleGenerator.GetSampleObject(itemsType);

                    // Fill in the other information needed to invoke the PageResult<T> constuctor
                    Type[] parameterTypes = new Type[] { itemsType, typeof(Uri), typeof(long?), };
                    object[] parameters = new object[] { items, null, (long)ObjectGenerator.DefaultCollectionSize, };

                    // Call PageResult(IEnumerable<T> items, Uri nextPageLink, long? count) constructor
                    ConstructorInfo constructor = type.GetConstructor(parameterTypes);
                    return constructor.Invoke(parameters);
                }
            }

            return null;
        }
#endif
    }
}