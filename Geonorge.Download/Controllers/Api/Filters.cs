using Geonorge.NedlastingApi.V3;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;

namespace Geonorge.Download.Controllers.Api
{
    internal class RemoveVersionParameterFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var versionParameter = operation.Parameters?.FirstOrDefault(p => p.Name == "api-version");
            if (versionParameter != null)
            {
                operation.Parameters.Remove(versionParameter);
            }
        }
    }

    internal class TagDescriptionsDocumentFilter : IDocumentFilter
    {
        private readonly Dictionary<string, string> _controllerSummaries;

        public TagDescriptionsDocumentFilter(string xmlPath)
        {
            _controllerSummaries = GetControllerSummaries(xmlPath);
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Find all unique tag names used in operations
            var tagsInUse = swaggerDoc.Paths
                .SelectMany(p => p.Value.Operations)
                .SelectMany(op => op.Value.Tags)
                .Select(t => t.Name)
                .Distinct();

            swaggerDoc.Tags = new List<OpenApiTag>();

            foreach (var tag in tagsInUse)
            {
                var controllerName = tag + "Controller";
                _controllerSummaries.TryGetValue(controllerName, out string summary);

                swaggerDoc.Tags.Add(new OpenApiTag
                {
                    Name = tag,
                    Description = summary // may be null if not found
                });
            }
        }

        private Dictionary<string, string> GetControllerSummaries(string xmlPath)
        {
            var summaries = new Dictionary<string, string>();
            var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
            var xpathDoc = new XPathDocument(xmlPath);
            var navigator = xpathDoc.CreateNavigator();

            // Find all classes with Controller suffix in XML docs
            var nodes = navigator.Select($"/doc/members/member[starts-with(@name, 'T:{assemblyName}.') and substring(@name, string-length(@name) - 9) = 'Controller']");
            while (nodes.MoveNext())
            {
                var memberName = nodes.Current.GetAttribute("name", "");
                // Format: T:YourNamespace.YourController
                var className = memberName.Split('.').Last();
                var summaryNode = nodes.Current.SelectSingleNode("summary");
                if (summaryNode != null)
                {
                    summaries[className] = summaryNode.InnerXml.Trim();
                }
            }
            return summaries;
        }
    }

    internal class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check for Authorize attribute
            var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any()
                            || context.MethodInfo.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any();

            if (hasAuthorize)
            {
                operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    // endpoints secured with Keycloak JWT
                    [ new OpenApiSecurityScheme { Reference = new OpenApiReference
                        { Type = ReferenceType.SecurityScheme, Id = "Bearer" } } ] = Array.Empty<string>(),

                    // endpoints secured with the external token
                    [ new OpenApiSecurityScheme { Reference = new OpenApiReference
                        { Type = ReferenceType.SecurityScheme, Id = "ExternalToken" } } ] = Array.Empty<string>()
                }
            };
            }
        }
    }

    //internal sealed class XsdExampleSchemaFilter : ISchemaFilter
    //{
    //    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    //    {
    //        schema.Xml ??= new OpenApiXml();
    //        if (string.IsNullOrEmpty(schema.Xml.Name) && context.Type.Namespace != "System" && context.Type.Namespace != "System.Collections.Generic")
    //        {
    //            schema.Xml.Name = context.Type.Name;
    //        }                

    //        switch (context.Type.Name)
    //        {
    //            // ---------- Capabilities ----------------------------------------------------
    //            // Source: examples.txt – CapabilitiesType :contentReference[oaicite:0]{index=0}
    //            case nameof(CapabilitiesType):
    //                schema.Example = new OpenApiObject
    //                {
    //                    ["supportsProjectionSelection"] = new OpenApiBoolean(true),
    //                    ["supportsFormatSelection"] = new OpenApiBoolean(true),
    //                    ["supportsPolygonSelection"] = new OpenApiBoolean(true),
    //                    ["supportsAreaSelection"] = new OpenApiBoolean(true),
    //                    ["mapSelectionLayer"] = new OpenApiString("raster-n250"),
    //                    ["supportsDownloadBundling"] = new OpenApiBoolean(false),
    //                    ["distributedBy"] = new OpenApiString("Geonorge"),
    //                    ["deliveryNotificationByEmail"] = new OpenApiBoolean(false),
    //                    ["_links"] = new OpenApiArray
    //                {
    //                    new OpenApiObject
    //                    {
    //                        ["href"] = new OpenApiString("http://nedlasting.dev.geonorge.no/api/codelists/projection/041f1e6e-bdbc-4091-b48f-8a5990f3cc5b"),
    //                        ["rel"]  = new OpenApiString("http://rel.geonorge.no/download/projection"),
    //                        ["templatedSpecified"] = new OpenApiBoolean(false)
    //                    }
    //                }
    //                };
    //                break;

    //            // ---------- Projection ------------------------------------------------------
    //            // Source: examples.txt – first ProjectionType element :contentReference[oaicite:1]{index=1}
    //            case nameof(ProjectionType):
    //                schema.Example = new OpenApiObject
    //                {
    //                    ["code"] = new OpenApiString("25832"),
    //                    ["name"] = new OpenApiString("EUREF89 UTM sone 32, 2d"),
    //                    ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25832")
    //                };
    //                break;

    //            // ---------- Area ------------------------------------------------------------
    //            // Source: examples.txt – first AreaType element :contentReference[oaicite:2]{index=2}
    //            case nameof(AreaType):
    //                schema.Example = new OpenApiObject
    //                {
    //                    ["type"] = new OpenApiString("fylke"),
    //                    ["name"] = new OpenApiString("Akershus"),
    //                    ["code"] = new OpenApiString("02"),
    //                    ["projections"] = new OpenApiArray
    //                {
    //                    new OpenApiObject
    //                    {
    //                        ["code"]      = new OpenApiString("25832"),
    //                        ["name"]      = new OpenApiString("EUREF89 UTM sone 32, 2d"),
    //                        ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25832")
    //                    }
    //                },
    //                    ["formats"] = new OpenApiArray
    //                {
    //                    new OpenApiObject { ["name"] = new OpenApiString("SOSI 4.5") }
    //                }
    //                };
    //                break;

    //            // ---------- Format ----------------------------------------------------------
    //            // Source: examples.txt – first FormatType element :contentReference[oaicite:3]{index=3}
    //            case nameof(FormatType):
    //                schema.Example = new OpenApiObject { ["name"] = new OpenApiString("SOSI 4.5") };
    //                break;

    //            // ---------- CanDownloadRequest ---------------------------------------------
    //            // Source: examples.txt – CanDownloadRequestType :contentReference[oaicite:4]{index=4}
    //            case nameof(CanDownloadRequestType):
    //                schema.Example = new OpenApiObject
    //                {
    //                    ["metadataUuid"] = new OpenApiString("73f863ba-628f-48af-b7fa-30d3ab331b8d"),
    //                    ["coordinates"] = new OpenApiString("344754 7272921 404330 7187619 304134 7156477 344754 7272921"),
    //                    ["coordinateSystem"] = new OpenApiString("25833")
    //                };
    //                break;

    //            // ---------- CanDownloadResponse --------------------------------------------
    //            // Source: examples.txt – CanDownloadResponseType :contentReference[oaicite:5]{index=5}
    //            case nameof(CanDownloadResponseType):
    //                schema.Example = new OpenApiObject
    //                {
    //                    ["canDownload"] = new OpenApiBoolean(true),
    //                    ["message"] = new OpenApiString("sample string 2"),
    //                    ["_links"] = new OpenApiArray
    //                {
    //                    new OpenApiObject
    //                    {
    //                        ["href"]      = new OpenApiString("sample string 1"),
    //                        ["rel"]       = new OpenApiString("sample string 2"),
    //                        ["templated"] = new OpenApiBoolean(true)
    //                    }
    //                }
    //                };
    //                break;

    //            // ---------- ClipperFileResponse --------------------------------------------
    //            // Source: examples.txt – ClipperFileResponseType :contentReference[oaicite:6]{index=6}
    //            case nameof(ClipperFileResponseType):
    //                schema.Example = new OpenApiObject
    //                {
    //                    ["valid"] = new OpenApiBoolean(true),
    //                    ["message"] = new OpenApiString("sample string 2"),
    //                    ["url"] = new OpenApiString("sample string 3")
    //                };
    //                break;

    //            // ---------- Order -----------------------------------------------------------
    //            // Source: examples.txt – OrderType :contentReference[oaicite:7]{index=7}
    //            case nameof(OrderType):
    //                schema.Example = new OpenApiObject
    //                {
    //                    ["downloadAsBundle"] = new OpenApiBoolean(false),
    //                    ["email"] = new OpenApiString("bruker@epost.no"),
    //                    ["orderLines"] = new OpenApiArray
    //                {
    //                    new OpenApiObject
    //                    {
    //                        ["areas"] = new OpenApiArray
    //                        {
    //                            new OpenApiObject
    //                            {
    //                                ["code"] = new OpenApiString("02"),
    //                                ["name"] = new OpenApiString("Akershus"),
    //                                ["type"] = new OpenApiString("fylke")
    //                            }
    //                        },
    //                        ["formats"] = new OpenApiArray
    //                        {
    //                            new OpenApiObject { ["name"] = new OpenApiString("SOSI 4.5") },
    //                            new OpenApiObject { ["name"] = new OpenApiString("GML 3.2.1") }
    //                        },
    //                        ["metadataUuid"] = new OpenApiString("041f1e6e-bdbc-4091-b48f-8a5990f3cc5b"),
    //                        ["projections"]  = new OpenApiArray
    //                        {
    //                            new OpenApiObject
    //                            {
    //                                ["code"]      = new OpenApiString("25832"),
    //                                ["name"]      = new OpenApiString("EUREF89 UTM sone 32, 2d"),
    //                                ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25832")
    //                            }
    //                        }
    //                    }
    //                }
    //                };
    //                break;

    //            // ---------- OrderReceipt ----------------------------------------------------
    //            // Source: examples.txt – OrderReceiptType :contentReference[oaicite:8]{index=8}
    //            case nameof(OrderReceiptType):
    //                schema.Example = new OpenApiObject
    //                {
    //                    ["referenceNumber"] = new OpenApiString("O-123456789"),
    //                    ["files"] = new OpenApiArray
    //                {
    //                    new OpenApiObject
    //                    {
    //                        ["downloadUrl"] = new OpenApiString("nedlastbarURI"),
    //                        ["fileSize"]    = new OpenApiString("12345"),
    //                        ["name"]        = new OpenApiString("AdministrativeEnheter_02_32.gml"),
    //                        ["status"]      = new OpenApiString("ReadyForDownload")
    //                    }
    //                },
    //                    ["orderDate"] = new OpenApiString("2024-01-01T00:00:00"),
    //                    ["downloadAsBundle"] = new OpenApiBoolean(false)
    //                };
    //                break;
    //        }
    //    }
    //}
}