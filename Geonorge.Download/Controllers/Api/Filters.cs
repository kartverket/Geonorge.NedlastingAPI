using Geonorge.Download.Controllers.Api.V3;
using Geonorge.NedlastingApi.V3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;

namespace Geonorge.Download.Controllers.Api
{
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

    internal class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check for Authorize attribute
            var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any()
                            || context.MethodInfo.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any();

            if (!hasAuthorize)
                return;

            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "GeoID" }
                }] = Array.Empty<string>()
            });

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Machine account" }
                }] = Array.Empty<string>()
            });

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ExternalToken" }
                }] = Array.Empty<string>()
            });

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "FME" }
                }] = Array.Empty<string>()
            });
        }
    }

    internal sealed class HideSecuritySchemesPerDocFilter : IDocumentFilter
    {
        private static readonly Dictionary<string, HashSet<string>> _visibleByDoc =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // public docs => public schemes
                ["latest"] = new(StringComparer.OrdinalIgnoreCase) { "GeoID", "Machine account" },
                ["v3"] = new(StringComparer.OrdinalIgnoreCase) { "GeoID", "Machine account" },

                // internal doc => internal schemes
                ["internal"] = new(StringComparer.OrdinalIgnoreCase) { "ExternalToken", "FME" }
            };

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (!_visibleByDoc.TryGetValue(context.DocumentName, out var visible))
                return;

            var allSchemeIds = swaggerDoc.Components?.SecuritySchemes?.Keys?.ToList() ?? new();
            foreach (var id in allSchemeIds)
            {
                if (!visible.Contains(id))
                    swaggerDoc.Components.SecuritySchemes.Remove(id);
            }

            foreach (var pathKvp in swaggerDoc.Paths)
            {
                foreach (var op in pathKvp.Value.Operations.Values)
                {
                    if (op.Security == null || op.Security.Count == 0) continue;

                    for (int i = op.Security.Count - 1; i >= 0; i--)
                    {
                        var req = op.Security[i];
                        var keys = req.Keys.ToList();
                        bool removedAny = false;

                        foreach (var scheme in keys)
                        {
                            var id = scheme.Reference?.Id ?? scheme.Name;
                            if (!string.IsNullOrEmpty(id) && !visible.Contains(id))
                            {
                                req.Remove(scheme);
                                removedAny = true;
                            }
                        }

                        if (removedAny && req.Count == 0)
                            op.Security.RemoveAt(i);
                    }
                }
            }
        }
    }

    internal sealed class XsdExampleSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            schema.Xml ??= new OpenApiXml();
            if (string.IsNullOrEmpty(schema.Xml.Name) && context.Type.Namespace != "System" && context.Type.Namespace != "System.Collections.Generic")
            {
                schema.Xml.Name = context.Type.Name;
            }

            switch (context.Type.Name)
            {
                // --- /api/capabilities/{metadataUuid} ---
                case nameof(CapabilitiesType):
                    schema.Example = new OpenApiObject
                    {
                        ["supportsProjectionSelection"] = new OpenApiBoolean(true),
                        ["supportsFormatSelection"] = new OpenApiBoolean(true),
                        ["supportsPolygonSelection"] = new OpenApiBoolean(true),
                        ["supportsAreaSelection"] = new OpenApiBoolean(true),
                        ["mapSelectionLayer"] = new OpenApiString("raster-n250"),
                        ["supportsDownloadBundling"] = new OpenApiBoolean(true),
                        ["distributedBy"] = new OpenApiString("Geonorge"),
                        ["deliveryNotificationByEmail"] = new OpenApiBoolean(false),
                        ["_links"] = new OpenApiArray
                        {
                            new OpenApiObject
                            {
                                ["href"] = new OpenApiString("https://nedlasting.geonorge.no/api/codelists/projection/041f1e6e-bdbc-4091-b48f-8a5990f3cc5b"),
                                ["rel"]  = new OpenApiString("http://rel.geonorge.no/download/projection")
                            },
                            new OpenApiObject
                            {
                                ["href"] = new OpenApiString("https://nedlasting.geonorge.no/api/codelists/format/041f1e6e-bdbc-4091-b48f-8a5990f3cc5b"),
                                ["rel"]  = new OpenApiString("http://rel.geonorge.no/download/format")
                            },
                            new OpenApiObject
                            {
                                ["href"] = new OpenApiString("https://nedlasting.geonorge.no/api/codelists/area/041f1e6e-bdbc-4091-b48f-8a5990f3cc5b"),
                                ["rel"]  = new OpenApiString("http://rel.geonorge.no/download/area")
                            },
                            new OpenApiObject
                            {
                                ["href"] = new OpenApiString("https://nedlasting.geonorge.no/api/order"),
                                ["rel"]  = new OpenApiString("http://rel.geonorge.no/download/order")
                            },
                            new OpenApiObject
                            {
                                ["href"] = new OpenApiString("https://nedlasting.geonorge.no/api/capabilities/041f1e6e-bdbc-4091-b48f-8a5990f3cc5b"),
                                ["rel"]  = new OpenApiString("self")
                            },
                            new OpenApiObject
                            {
                                ["href"] = new OpenApiString("https://nedlasting.geonorge.no/api/can-download"),
                                ["rel"]  = new OpenApiString("http://rel.geonorge.no/download/can-download")
                            }
                        }
                    };
                    break;
                // --- /api/codelists/projection/{metadataUuid} ---
                case nameof(ProjectionType):
                    schema.Example = new OpenApiArray
                    {
                        new OpenApiObject
                        {
                            ["code"] = new OpenApiString("25832"),
                            ["name"] = new OpenApiString("EUREF89 UTM sone 32, 2d"),
                            ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25832"),
                            ["formats"] = new OpenApiArray
                            {
                                new OpenApiObject { ["name"] = new OpenApiString("GeoJSON") },
                                new OpenApiObject { ["name"] = new OpenApiString("GML 3.2.1") },
                                new OpenApiObject { ["name"] = new OpenApiString("PostGIS 12") }
                            }
                        },
                        new OpenApiObject
                        {
                            ["code"] = new OpenApiString("25833"),
                            ["name"] = new OpenApiString("EUREF89 UTM sone 33, 2d"),
                            ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25833"),
                            ["formats"] = new OpenApiArray
                            {
                                new OpenApiObject { ["name"] = new OpenApiString("GeoJSON") },
                                new OpenApiObject { ["name"] = new OpenApiString("GML 3.2.1") },
                                new OpenApiObject { ["name"] = new OpenApiString("PostGIS 12") }
                            }
                        },
                        new OpenApiObject
                        {
                            ["code"] = new OpenApiString("4258"),
                            ["name"] = new OpenApiString("EUREF 89 Geografisk (ETRS 89) 2d"),
                            ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/4258"),
                            ["formats"] = new OpenApiArray
                            {
                                new OpenApiObject { ["name"] = new OpenApiString("GeoJSON") }
                            }
                        }
                    };
                    break;
                // --- /api/codelists/area/{metadataUuid} ---
                case nameof(AreaType):
                    schema.Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("fylke"),
                        ["name"] = new OpenApiString("Agder"),
                        ["code"] = new OpenApiString("42"),
                        ["projections"] = new OpenApiArray
                        {
                            new OpenApiObject
                            {
                                ["code"] = new OpenApiString("4258"),
                                ["name"] = new OpenApiString("EUREF 89 Geografisk (ETRS 89) 2d"),
                                ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/4258"),
                                ["formats"] = new OpenApiArray
                                {
                                    new OpenApiObject { ["name"] = new OpenApiString("GeoJSON") }
                                }
                            },
                            new OpenApiObject
                            {
                                ["code"] = new OpenApiString("25832"),
                                ["name"] = new OpenApiString("EUREF89 UTM sone 32, 2d"),
                                ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25832"),
                                ["formats"] = new OpenApiArray
                                {
                                    new OpenApiObject { ["name"] = new OpenApiString("GML") },
                                    new OpenApiObject { ["name"] = new OpenApiString("PostGIS") },
                                    new OpenApiObject { ["name"] = new OpenApiString("GeoJSON") }
                                }
                            },
                            new OpenApiObject
                            {
                                ["code"] = new OpenApiString("25833"),
                                ["name"] = new OpenApiString("EUREF89 UTM sone 33, 2d"),
                                ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25833"),
                                ["formats"] = new OpenApiArray
                                {
                                    new OpenApiObject { ["name"] = new OpenApiString("GeoJSON") },
                                    new OpenApiObject { ["name"] = new OpenApiString("GML") },
                                    new OpenApiObject { ["name"] = new OpenApiString("PostGIS") }
                                }
                            }
                        },
                        ["formats"] = new OpenApiArray
                        {
                            new OpenApiObject
                            {
                                ["name"] = new OpenApiString("GeoJSON"),
                                ["projections"] = new OpenApiArray
                                {
                                    new OpenApiObject
                                    {
                                        ["code"] = new OpenApiString("4258"),
                                        ["name"] = new OpenApiString("EUREF 89 Geografisk (ETRS 89) 2d"),
                                        ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/4258")
                                    },
                                    new OpenApiObject
                                    {
                                        ["code"] = new OpenApiString("25833"),
                                        ["name"] = new OpenApiString("EUREF89 UTM sone 33, 2d"),
                                        ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25833")
                                    },
                                    new OpenApiObject
                                    {
                                        ["code"] = new OpenApiString("25832"),
                                        ["name"] = new OpenApiString("EUREF89 UTM sone 32, 2d"),
                                        ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25832")
                                    }
                                }
                            },
                            new OpenApiObject
                            {
                                ["name"] = new OpenApiString("GML"),
                                ["projections"] = new OpenApiArray
                                {
                                    new OpenApiObject
                                    {
                                        ["code"] = new OpenApiString("25832"),
                                        ["name"] = new OpenApiString("EUREF89 UTM sone 32, 2d"),
                                        ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25832")
                                    },
                                    new OpenApiObject
                                    {
                                        ["code"] = new OpenApiString("25833"),
                                        ["name"] = new OpenApiString("EUREF89 UTM sone 33, 2d"),
                                        ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25833")
                                    }
                                }
                            },
                            new OpenApiObject
                            {
                                ["name"] = new OpenApiString("PostGIS"),
                                ["projections"] = new OpenApiArray
                                {
                                    new OpenApiObject
                                    {
                                        ["code"] = new OpenApiString("25832"),
                                        ["name"] = new OpenApiString("EUREF89 UTM sone 32, 2d"),
                                        ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25832")
                                    },
                                    new OpenApiObject
                                    {
                                        ["code"] = new OpenApiString("25833"),
                                        ["name"] = new OpenApiString("EUREF89 UTM sone 33, 2d"),
                                        ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25833")
                                    }
                                }
                            }
                        }
                    };
                    break;
                // --- /api/codelists/format/{metadataUuid} ---
                case nameof(FormatType):
                    schema.Example = new OpenApiArray
                    {
                        new OpenApiObject
                        {
                            ["name"] = new OpenApiString("GeoJSON"),
                            ["projections"] = new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    ["code"] = new OpenApiString("25832"),
                                    ["name"] = new OpenApiString("EUREF89 UTM sone 32, 2d"),
                                    ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25832")
                                },
                                new OpenApiObject
                                {
                                    ["code"] = new OpenApiString("25833"),
                                    ["name"] = new OpenApiString("EUREF89 UTM sone 33, 2d"),
                                    ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25833")
                                },
                                new OpenApiObject
                                {
                                    ["code"] = new OpenApiString("4258"),
                                    ["name"] = new OpenApiString("EUREF 89 Geografisk (ETRS 89) 2d"),
                                    ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/4258")
                                }
                            }
                        },
                        new OpenApiObject
                        {
                            ["name"] = new OpenApiString("GML"),
                            ["projections"] = new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    ["code"] = new OpenApiString("25832"),
                                    ["name"] = new OpenApiString("EUREF89 UTM sone 32, 2d"),
                                    ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25832")
                                },
                                new OpenApiObject
                                {
                                    ["code"] = new OpenApiString("25833"),
                                    ["name"] = new OpenApiString("EUREF89 UTM sone 33, 2d"),
                                    ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25833")
                                }
                            }
                        },
                        new OpenApiObject
                        {
                            ["name"] = new OpenApiString("PostGIS"),
                            ["projections"] = new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    ["code"] = new OpenApiString("25832"),
                                    ["name"] = new OpenApiString("EUREF89 UTM sone 32, 2d"),
                                    ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25832")
                                },
                                new OpenApiObject
                                {
                                    ["code"] = new OpenApiString("25833"),
                                    ["name"] = new OpenApiString("EUREF89 UTM sone 33, 2d"),
                                    ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25833")
                                }
                            }
                        }
                    };
                    break;
                // --- /api/can-download ---
                case nameof(CanDownloadRequestType):
                    schema.Example = new OpenApiObject
                    {
                        ["metadataUuid"] = new OpenApiString("73f863ba-628f-48af-b7fa-30d3ab331b8d"),
                        ["coordinates"] = new OpenApiString("344754 7272921 404330 7187619 304134 7156477 344754 7272921"),
                        ["coordinateSystem"] = new OpenApiString("25833")
                    };
                    break;

                case nameof(CanDownloadResponseType):
                    schema.Example = new OpenApiObject
                    {
                        ["canDownload"] = new OpenApiBoolean(true)
                    };
                    break;
                // --- /api/validate-clipperfile/{metadataUuid} ---
                case nameof(ClipperFileResponseType):
                    schema.Example = new OpenApiObject
                    {
                        ["valid"] = new OpenApiBoolean(true),
                        ["message"] = new OpenApiString(""),
                        ["url"] = new OpenApiString("https://nedlasting.geonorge.no/clipperfiles/3114fe13-43ga-26c5-91da-2e3e533f23ca.geojson")
                    };
                    break;
                // --- /api/order ---
                case nameof(OrderType):
                    schema.Example = new OpenApiObject
                    {
                        ["downloadAsBundle"] = new OpenApiBoolean(false),
                        ["email"] = new OpenApiString("bruker@epost.no"),
                        ["orderLines"] = new OpenApiArray
                    {
                        new OpenApiObject
                        {
                            ["areas"] = new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    ["code"] = new OpenApiString("32"),
                                    ["name"] = new OpenApiString("Akershus"),
                                    ["type"] = new OpenApiString("fylke")
                                }
                            },
                            ["formats"] = new OpenApiArray
                            {
                                new OpenApiObject { ["name"] = new OpenApiString("GeoJSON ") },
                                new OpenApiObject { ["name"] = new OpenApiString("GML 3.2.1") }
                            },
                            ["metadataUuid"] = new OpenApiString("041f1e6e-bdbc-4091-b48f-8a5990f3cc5b"),
                            ["projections"]  = new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    ["code"]      = new OpenApiString("25832"),
                                    ["name"]      = new OpenApiString("EUREF89 UTM sone 32, 2d"),
                                    ["codespace"] = new OpenApiString("http://www.opengis.net/def/crs/EPSG/0/25832")
                                }
                            }
                        }
                    }
                    };
                    break;

                case nameof(OrderReceiptType):
                    schema.Example = new OpenApiObject
                    {
                        ["referenceNumber"] = new OpenApiString("3f854e9c-5345-4428-b2fd-1ea3db5d2f7a"),
                        ["files"] = new OpenApiArray
                        {
                            new OpenApiObject
                            {
                                ["downloadUrl"] = new OpenApiString("https://nedlasting.geonorge.no/api/download/order/3f854e9c-5345-4428-b2fd-1ea3db5d2f7a/0e23b13e-cd0f-4d24-9497-408c4f65aa33"),
                                ["name"]        = new OpenApiString("Basisdata_32_Akershus_25832_Kommuner_GeoJSON.zip"),
                                ["fileId"]      = new OpenApiString("0e23b13e-cd0f-4d24-9497-408c4f65aa33"),
                                ["metadataUuid"] = new OpenApiString("041f1e6e-bdbc-4091-b48f-8a5990f3cc5b"),
                                ["area"]        = new OpenApiString("32"),
                                ["projection"]  = new OpenApiString("25832"),
                                ["format"]        = new OpenApiString("GeoJSON "),
                                ["status"]      = new OpenApiString("ReadyForDownload"),
                                ["metadataName"] = new OpenApiString("Kommuner"),
                                ["areaName"]    = new OpenApiString("Akershus"),
                                ["projectionName"] = new OpenApiString("EUREF89 UTM sone 32, 2d")
                            },
                            new OpenApiObject
                            {
                                ["downloadUrl"] = new OpenApiString("https://nedlasting.geonorge.no/api/download/order/3f854e9c-5345-4428-b2fd-1ea3db5d2f7a/64303302-b64e-46e2-8fa6-b31bee8f23da"),
                                ["name"]        = new OpenApiString("Basisdata_32_Akershus_25832_Kommuner_GML.zip"),
                                ["fileId"]      = new OpenApiString("64303302-b64e-46e2-8fa6-b31bee8f23da"),
                                ["metadataUuid"] = new OpenApiString("041f1e6e-bdbc-4091-b48f-8a5990f3cc5b"),
                                ["area"]        = new OpenApiString("32"),
                                ["projection"]  = new OpenApiString("25832"),
                                ["format"]        = new OpenApiString("GML 3.2.1"),
                                ["status"]      = new OpenApiString("ReadyForDownload"),
                                ["metadataName"] = new OpenApiString("Kommuner"),
                                ["areaName"]    = new OpenApiString("Akershus"),
                                ["projectionName"] = new OpenApiString("EUREF89 UTM sone 32, 2d")
                            }
                        },
                        ["email"] = new OpenApiString("bruker@epost.no"),
                        ["orderDate"] = new OpenApiString("2025-09-23T12:00:00.0Z"),
                        ["downloadAsBundle"] = new OpenApiBoolean(false)
                    };
                    break;
            }
        }
    }


    //internal sealed class ExamplesOperationFilter : IOperationFilter
    //{
    //    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    //    {
    //        // ----- Request body examples (by body parameter type) -----
    //        var bodyParam = context.MethodInfo
    //            .GetParameters()
    //            .FirstOrDefault(p => p.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("FromBody")));
    //        if (operation.RequestBody != null && bodyParam != null)
    //        {
    //            foreach (var (mediaKey, mediaType) in operation.RequestBody.Content)
    //            {
    //                mediaType.Examples ??= new Dictionary<string, OpenApiExample>();
    //                switch (bodyParam.ParameterType.Name)
    //                {
    //                    case nameof(OrderType):
    //                        mediaType.Examples["Minimal"] = new OpenApiExample
    //                        {
    //                            Summary = "Small order",
    //                            Value = new OpenApiObject
    //                            {
    //                                ["customerId"] = new OpenApiString("C-1001"),
    //                                ["items"] = new OpenApiArray
    //                            {
    //                                new OpenApiObject
    //                                {
    //                                    ["sku"] = new OpenApiString("ABC-123"),
    //                                    ["qty"] = new OpenApiInteger(1)
    //                                }
    //                            }
    //                            }
    //                        };
    //                        mediaType.Examples["WithNotes"] = new OpenApiExample
    //                        {
    //                            Summary = "Order with notes & priority",
    //                            Value = new OpenApiObject
    //                            {
    //                                ["customerId"] = new OpenApiString("C-1001"),
    //                                ["priority"] = new OpenApiString("Express"),
    //                                ["notes"] = new OpenApiString("Leave at back door"),
    //                                ["items"] = new OpenApiArray
    //                            {
    //                                new OpenApiObject
    //                                {
    //                                    ["sku"] = new OpenApiString("XYZ-999"),
    //                                    ["qty"] = new OpenApiInteger(2)
    //                                }
    //                            }
    //                            }
    //                        };
    //                        break;
    //                }
    //            }
    //        }

    //        // ----- Response examples (by status code & declared response type) -----
    //        foreach (var apiResponse in context.ApiDescription.SupportedResponseTypes)
    //        {
    //            var statusCode = apiResponse.StatusCode.ToString();
    //            if (!operation.Responses.TryGetValue(statusCode, out var response))
    //                continue;

    //            foreach (var kvp in response.Content)
    //            {
    //                var mediaTypeKey = kvp.Key;              // e.g. application/json
    //                var mediaType = kvp.Value;

    //                mediaType.Examples ??= new Dictionary<string, OpenApiExample>();

    //                var modelType = apiResponse.Type;        // The declared type for this status code
    //                var underlying = Nullable.GetUnderlyingType(modelType) ?? modelType;

    //                switch (underlying.Name)
    //                {
    //                    case nameof(OrderReceiptType):
    //                        mediaType.Examples["Accepted"] = new OpenApiExample
    //                        {
    //                            Summary = "Receipt for a paid order",
    //                            Value = new OpenApiObject
    //                            {
    //                                ["orderId"] = new OpenApiString("ORD-2025-0001"),
    //                                ["status"] = new OpenApiString("Accepted"),
    //                                ["total"] = new OpenApiDouble(149.90),
    //                                ["currency"] = new OpenApiString("NOK"),
    //                                ["downloadUrl"] = new OpenApiString("https://api.example.org/orders/ORD-2025-0001/download")
    //                            }
    //                        };
    //                        mediaType.Examples["Queued"] = new OpenApiExample
    //                        {
    //                            Summary = "Receipt for queued processing",
    //                            Value = new OpenApiObject
    //                            {
    //                                ["orderId"] = new OpenApiString("ORD-2025-0002"),
    //                                ["status"] = new OpenApiString("Queued"),
    //                                ["eta"] = new OpenApiString("2025-09-23T12:00:00Z")
    //                            }
    //                        };
    //                        break;

    //                    case nameof(ProblemDetails):
    //                        mediaType.Examples["ServerError"] = new OpenApiExample
    //                        {
    //                            Summary = "Generic server error",
    //                            Value = new OpenApiObject
    //                            {
    //                                ["title"] = new OpenApiString("Order failed"),
    //                                ["status"] = new OpenApiInteger(500),
    //                                ["detail"] = new OpenApiString("Unexpected error while creating order.")
    //                            }
    //                        };
    //                        mediaType.Examples["Forbidden"] = new OpenApiExample
    //                        {
    //                            Summary = "Forbidden",
    //                            Value = new OpenApiObject
    //                            {
    //                                ["title"] = new OpenApiString("Access denied"),
    //                                ["status"] = new OpenApiInteger(403),
    //                                ["detail"] = new OpenApiString("You do not have permission to create orders.")
    //                            }
    //                        };
    //                        break;
    //                }
    //            }
    //        }
    //    }
    //}

}