using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Geonorge.AuthLib.Common;
using Geonorge.Download.Components;
using Geonorge.Download.Controllers.Api;
using Geonorge.Download.Models;
using Geonorge.Download.Services;
using Geonorge.Download.Services.Auth;
using Geonorge.Download.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Serilog;
using SimpleBlazorMultiselect;
using StackExchange.Redis;
using System;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Setup Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

var gcsSection = builder.Configuration.GetSection("Gcs");
var bucketName = gcsSection["Bucket"];

builder.Services.AddSingleton(StorageClient.Create());
builder.Services.AddSingleton(new GcsSettings(bucketName));


// --- Database ---
builder.Services.AddDbContext<DownloadContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

if (!builder.Environment.IsDevelopment())
{
    // --- Redis Data Protection ---
    string redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection") ?? throw new InvalidOperationException("Redis connection string is not configured.");
    Log.Logger.Information("Using Redis connection string: {RedisConnectionString}", redisConnectionString);
    var redis = ConnectionMultiplexer.Connect(redisConnectionString);
    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(redis, "dp:keys")
        .SetApplicationName("Geonorge.Download")
        .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
}

// --- AuthN/AuthZ ---
builder.Services.AddScoped<IBaatAuthzApi>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<BaatAuthzApi>>();
    var hfac = sp.GetRequiredService<IHttpClientFactory>();
    return new BaatAuthzApi(logger, config, hfac);
});
builder.Services.AddScoped<IGeonorgeAuthorizationService, GeonorgeAuthorizationService>();
//builder.Services.AddScoped<GeonorgeOpenIdConnectEvents>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.AccessDeniedPath = "/error/unauthorized";
        //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = false;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(25);
        options.Cookie.SameSite = SameSiteMode.Lax;
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, oidc =>
    {
        // Fill from configuration/secrets
        //oidc.TokenValidationParameters.ValidIssuer = builder.Configuration["auth:oidc:Issuer"];
        oidc.Authority = builder.Configuration["auth:oidc:Authority"];
        oidc.ClientId = builder.Configuration["auth:oidc:ClientId"];
        oidc.ClientSecret = builder.Configuration["auth:oidc:ClientSecret"];
            
        // Core OIDC
        oidc.ResponseType = OpenIdConnectResponseType.Code;
        oidc.UsePkce = true;

        oidc.SaveTokens = true;
        oidc.GetClaimsFromUserInfoEndpoint = true;

        //oidc.TokenValidationParameters = new TokenValidationParameters
        //{
        //    NameClaimType = "name",
        //    RoleClaimType = ClaimTypes.Role,
        //    ValidateIssuer = true
        //};

        // Scopes
        oidc.Scope.Clear();
        oidc.Scope.Add("openid");
        oidc.Scope.Add("profile");
        oidc.Scope.Add("email");

        oidc.CallbackPath = "/signin-oidc";
        oidc.SignedOutCallbackPath = "/signout-callback-oidc";
        //oidc.SignedOutRedirectUri = "/?logout=true"; // builder.Configuration["auth:oidc:PostLogoutRedirectUri"];
        oidc.RemoteSignOutPath = "/signout-oidc";
        oidc.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = async ctx =>
            {
                await ClaimsHelper.AddClaims(ctx.Principal, ctx.HttpContext);
            },
            OnRedirectToIdentityProvider = ctx =>
            {
                var req = ctx.Request;

                var host = req.Host.Value; // change if proxy doesn't preserve original Host
                var proto = req.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? req.Scheme;

                ctx.ProtocolMessage.RedirectUri = $"{proto}://{host}{ctx.Options.CallbackPath}";
                return Task.CompletedTask;
            }
        };
    })
    .AddScheme<BasicMachineAuthOptions, BasicMachineAuthHandler>(
        BasicMachineAuthHandler.SchemeName,
        options => { })
    .AddScheme<BasicAuthOptions, BasicAuthHandler>(
        BasicAuthHandler.SchemeName,
        options => builder.Configuration.GetSection("auth:BasicAuth").Bind(options))
    .AddScheme<ExternalTokenOptions, ExternalTokenHandler>(
        ExternalTokenHandler.SchemeName,
        options => builder.Configuration.GetSection("auth:ExternalToken").Bind(options))
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["auth:oidc:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidAudience = "account",           // must match the aud in the token
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            // If server and issuer have slight time drift
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        // If IdP sends tokens without a "typ" header or with "at+jwt", this avoids strict checks
        options.MapInboundClaims = false; // keep standard JWT claim types like "sub", "scope"
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                await ClaimsHelper.AddClaims(ctx.Principal, ctx.HttpContext);
            },
            OnAuthenticationFailed = ctx =>
            {
                ctx.NoResult(); // avoid throwing
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

builder.Services.AddDbContextFactory<DownloadContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Services ---
builder.Services.AddSingleton<IRegisterFetcher, RegisterFetcher>();
builder.Services.AddScoped<IEiendomService, EiendomService>();
builder.Services.AddScoped<ICapabilitiesService, CapabilitiesService>();
builder.Services.AddScoped<IDownloadService, DownloadService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IBasicAuthenticationCredentialValidator, BasicAuthenticationCredentialValidator>();
builder.Services.AddScoped<IClipperService, ClipperService>();
builder.Services.AddScoped<IOrderBundleService, OrderBundleService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMachineAccountService, MachineAccountService>();

// --- Internal Services ---
builder.Services.AddScoped<IUpdateMetadataService, UpdateMetadataService>();

// Optional service using HttpClient
builder.Services.AddHttpClient<IExternalRequestService, ExternalRequestService>(client =>
{
    var timeout = int.TryParse(builder.Configuration["HttpTimeout"], out var seconds) && seconds > 0
        ? seconds : 60;
    client.Timeout = TimeSpan.FromSeconds(timeout);
});

// --- Controllers and API versioning ---
builder.Services.AddControllers(options =>
    {
        //options.RespectBrowserAcceptHeader = true;
        //options.ReturnHttpNotAcceptable = true;

        //options.FormatterMappings.SetMediaTypeMappingForFormat("xml", "application/xml");
        //options.FormatterMappings.SetMediaTypeMappingForFormat("json", "application/json");
    })
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    })
    .AddXmlSerializerFormatters();


// --- Caching ---

builder.Services.AddStackExchangeRedisOutputCache(o =>
{
    o.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    o.InstanceName = "oc:";
});

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("MetaTag", b =>
    {
        b.Cache()
         .SetVaryByHeader("Accept")
         .SetVaryByHeader("Accept-Language")
         .SetVaryByQuery("*")
         .SetLocking(true)
         .With(ctx =>
         {
             ctx.EnableOutputCaching = true;
             ctx.AllowCacheLookup = true;
             ctx.AllowCacheStorage = true;
             ctx.AllowLocking = true;

             ctx.CacheVaryByRules.RouteValueNames = new[] { "metadataUuid" };

             if (ctx.HttpContext.Request.RouteValues.TryGetValue("metadataUuid", out var v) &&
                 v is string uuid && !string.IsNullOrWhiteSpace(uuid))
             {
                 ctx.Tags.Add($"meta:{uuid.ToLowerInvariant()}");
                 ctx.ResponseExpirationTimeSpan = TimeSpan.FromDays(30);
             }
             else
             {
                 ctx.ResponseExpirationTimeSpan = TimeSpan.FromHours(10);
                 ctx.Tags.Add("meta:__missing__");
             }

             return true;
         });
    });
});

// --- OpenAPI ---
builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(3, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;    
        options.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// --- Swagger ---
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.DocumentFilter<TagDescriptionsDocumentFilter>(xmlPath);
    options.OperationFilter<RemoveVersionParameterFilter>(); // version parameter not needed in requests, just version in path
    options.OperationFilter<AuthorizeCheckOperationFilter>();
    options.DocumentFilter<HideSecuritySchemesPerDocFilter>();
    options.SchemaFilter<XsdExampleSchemaFilter>();
    //options.OperationFilter<MultipleExamplesOperationFilter>();
    options.CustomSchemaIds(type => type.Name);

    options.SwaggerDoc("internal", new OpenApiInfo
    {
        Title = "Geonorge nedlastings-API (internal)",
        Version = "internal",
        Description = "Endpoints intended for internal operators, dataset providers, and admin tooling."
    });

    options.SwaggerDoc("v3", new OpenApiInfo 
    { 
        Title = "Geonorge nedlastings-API", 
        Version = "v3",
        Description = """
        A client will start by calling capabilities (api/capabilities/{metadataUuid}) this is the root API call for a dataset. Capabilities will announce the rest of the resources with links (href) and relation (rel).
        For more info implementing api please also see [documentation](https://nedlasting.dev.geonorge.no/help/documentation)
        """
    });

    options.SwaggerDoc("latest", new OpenApiInfo
    {
        Title = "Geonorge nedlastings-API",
        Version = "latest",
        Description = """
        A client will start by calling capabilities (api/capabilities/{metadataUuid}) this is the root API call for a dataset. Capabilities will announce the rest of the resources with links (href) and relation (rel).
        For more info implementing api please also see [documentation](https://nedlasting.dev.geonorge.no/help/documentation)
        """
    });

    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (docName.Equals("internal"))
            return string.Equals(apiDesc.GroupName, "internal", StringComparison.OrdinalIgnoreCase);

        if (docName.Equals("latest"))
        {
            if (apiDesc.RelativePath != null && apiDesc.RelativePath.StartsWith("api/v3"))
            {
                return false;
            }
            return string.Equals(apiDesc.GroupName, "latest", StringComparison.OrdinalIgnoreCase);
        }

        var metadata = apiDesc.ActionDescriptor.EndpointMetadata
            .OfType<ApiVersionMetadata>()
            .FirstOrDefault();

        if (metadata == null)
        {
            Console.WriteLine($"[SWAGGER DEBUG] Skipping {apiDesc.RelativePath} (no version metadata)");
            return false;
        }
        else if (apiDesc.RelativePath != null && !apiDesc.RelativePath.StartsWith("api/v3"))
        {
            return false;
        }

        var majors = metadata.Map(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit)
                         .DeclaredApiVersions
                         .Select(v => $"v{v.MajorVersion}");

        return majors.Contains(docName);
    });

    options.AddSecurityDefinition("GeoID", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "GeoID access token."
    });

    options.AddSecurityDefinition("Machine account", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Basic",
        In = ParameterLocation.Header,
        Description = "For machine accounts. Enter username and password"
    });

    options.AddSecurityDefinition("ExternalToken", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "External client bearer token."
    });

    options.AddSecurityDefinition("FME", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Basic",
        In = ParameterLocation.Header,
        Description = "For FME user. Enter username and password"
    });
});

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

    options.AddPolicy("AllowKartkatalog", policy =>
        policy.WithOrigins(
                "http://kartkatalog.dev.geonorge.no",
                "https://kartkatalog.dev.geonorge.no",
                "https://kartkatalog.test.geonorge.no",
                "https://kartkatalog.geonorge.no",
                "https://kartkatalog-frontend.dev.geonorge.no",
                "http://kartkatalog-frontend.dev.geonorge.no")
            .AllowAnyMethod().AllowAnyHeader().AllowCredentials());

    options.AddPolicy("AllowKartkatalogV2", policy =>
        policy.WithOrigins(
                "http://kartkatalog.dev.geonorge.no",
                "https://kartkatalog.dev.geonorge.no",
                "http://kurv.dev.geonorge.no",
                "https://kurv.dev.geonorge.no",
                "https://kartkatalog.test.geonorge.no",
                "https://kartkatalog.geonorge.no")
            .AllowAnyMethod().AllowAnyHeader().AllowCredentials());

    options.AddPolicy("AllowKartkatalog_2", policy =>
        policy.WithOrigins(
                "http://kartkatalog.dev.geonorge.no",
                "https://kartkatalog.dev.geonorge.no",
                "https://kartkatalog.test.geonorge.no",
                "https://kartkatalog.geonorge.no",
                "https://localhost:44355",
                "http://localhost:50081")
            .AllowAnyMethod().AllowAnyHeader().AllowCredentials());
});

// --- Localization ---
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// --- Blazor Components ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.WebHost.UseStaticWebAssets();
SimpleMultiselectGlobals.Standalone = true;
var app = builder.Build();

app.UseRequestLocalization(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("nb-NO"),
        new CultureInfo("en")
    };
    options.DefaultRequestCulture = new RequestCulture(supportedCultures[0]);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider
    {
        CookieName = "_culture"
    });
});

// --- Swagger Setup ---
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// protect internal API documentation
app.UseWhen(ctx => (ctx.Request.Path.StartsWithSegments("/swagger/internal") || 
                    ctx.Request.Path.StartsWithSegments("/swagger-internal")), branch =>
{
    branch.Use(async (context, next) =>
    {
        var authenticateResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!authenticateResult.Succeeded || !(authenticateResult.Principal?.Identity?.IsAuthenticated ?? false))
        {
            await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = context.Request.PathBase + context.Request.Path + context.Request.QueryString
            });
            return;
        }

        context.User = authenticateResult.Principal;

        if (!context.User.IsInRole(GeonorgeRoles.MetadataAdmin))
        {
            await context.ForbidAsync();
            return;
        }

        await next();
    });
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
            $"/swagger/latest/swagger.json",
            $"Geonorge nedlastings-API (latest)");

    options.SwaggerEndpoint(
            $"/swagger/v3/swagger.json",
            $"Geonorge nedlastings-API 3.0");

    //foreach (var description in provider.ApiVersionDescriptions.OrderByDescending(d => d.ApiVersion))
    //{
    //    options.SwaggerEndpoint(
    //        $"/swagger/{description.GroupName}/swagger.json",
    //        $"Geonorge nedlastings-API {description.ApiVersion}");
    //}
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
            $"/swagger/internal/swagger.json",
            $"Geonorge nedlastings-API (internal)");
    options.RoutePrefix = "swagger-internal";
});


// --- Middleware ---
app.UseCors("AllowAll"); // Or switch to a named policy as needed

app.UseOutputCache();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
    ForwardLimit = int.Parse(builder.Configuration["HeaderForwardLimit"]!) // Default is 1
});

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/clipperfiles/{**objectKey}", async 
    (
        string objectKey,
        HttpContext http,
        StorageClient storage,
        GcsSettings gcs
    ) =>
{
    if (string.IsNullOrWhiteSpace(objectKey))
        return Results.BadRequest("Missing object key");

    try
    {
        var meta = await storage.GetObjectAsync(gcs.Bucket, $"clipperfiles/{objectKey}");

        http.Response.ContentType = meta.ContentType ?? "application/octet-stream";
        http.Response.ContentLength = (long?)meta.Size;
        http.Response.Headers["Cache-Control"] = "public,max-age=31536000,immutable";

        await storage.DownloadObjectAsync(gcs.Bucket, $"clipperfiles/{objectKey}", http.Response.Body);
        return Results.Empty;
    }
    catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
    {
        return Results.NotFound();
    }
});

app.UseAntiforgery();

// --- Endpoints ---
app.MapControllers(); // For versioned REST API
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode(); // For Blazor pages

app.MapFallback(async context =>
{
    context.Items["originalPath"] = context.Request.Path;

    context.Response.StatusCode = StatusCodes.Status404NotFound;
    await Results.LocalRedirect($"/error/not-found?errorpath={context.Request.Path}").ExecuteAsync(context);
});

app.Run();

public sealed record GcsSettings(string Bucket);

internal sealed class ClaimsHelper
{
    internal static async Task AddClaims(ClaimsPrincipal? principal, HttpContext httpContext)
    {
        var events = httpContext.RequestServices.GetRequiredService<IGeonorgeAuthorizationService>();
        if (principal?.Identity is ClaimsIdentity identity)
        {
            identity.AddClaims(await events.GetClaims(identity));
            var orgNrClaim = identity.Claims.FirstOrDefault(c => c.Type == GeonorgeClaims.OrganizationOrgnr);
            if (orgNrClaim != null && !string.IsNullOrEmpty(orgNrClaim.Value))
            {
                var r_svc = httpContext.RequestServices.GetRequiredService<IRegisterFetcher>();
                var organization = r_svc.GetOrganization(orgNrClaim.Value);
                if (organization != null && !string.IsNullOrWhiteSpace(organization.MunicipalityCode))
                {
                    identity.AddClaim(new Claim("MunicipalityCode", organization.MunicipalityCode));
                }
            }
        }
    }
}