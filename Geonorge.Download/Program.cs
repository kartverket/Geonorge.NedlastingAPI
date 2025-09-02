using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Geonorge.Download.Components;
using Geonorge.Download.Controllers.Api;
using Geonorge.Download.Models;
using Geonorge.Download.Services;
using Geonorge.Download.Services.Auth;
using Geonorge.Download.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Serilog;
using StackExchange.Redis;
using System.Reflection;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

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
        .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
        .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
}

// --- AuthN/AuthZ ---
builder.Services.AddAuthentication("ExternalToken")
                .AddScheme<ExternalTokenOptions, ExternalTokenHandler>(
                    "ExternalToken",
                    options => builder.Configuration.GetSection("ExternalToken").Bind(options));

builder.Services.AddAuthorization();

// --- Services ---
builder.Services.AddSingleton<IRegisterFetcher, RegisterFetcher>();
builder.Services.AddScoped<IEiendomService, EiendomService>();
builder.Services.AddScoped<ICapabilitiesService, CapabilitiesService>();
builder.Services.AddScoped<IDownloadService, DownloadService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IBasicAuthenticationCredentialValidator, BasicAuthenticationCredentialValidator>();
builder.Services.AddScoped<IBasicAuthenticationService, BasicAuthenticationService>();
builder.Services.AddScoped<IGeoIdAuthenticationService, GeoIdAuthentication>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IClipperService, ClipperService>();
builder.Services.AddScoped<IOrderBundleService, OrderBundleService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOrderService, OrderService>();

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
    .AddXmlSerializerFormatters()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });

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

    //options.SchemaFilter<Geonorge.Download.Controllers.Api.V3.XsdExampleSchemaFilter>();
    options.DocumentFilter<TagDescriptionsDocumentFilter>(xmlPath);
    options.OperationFilter<RemoveVersionParameterFilter>(); // version parameter not needed in requests, just version in path
    options.OperationFilter<AuthorizeCheckOperationFilter>();
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

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your Keycloak JWT Bearer token in the format 'Bearer {token}'"
    });

    options.AddSecurityDefinition("ExternalToken", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "Token",
        In = ParameterLocation.Header,
        Description = "External client bearer token. Paste **only** the token."
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

// --- Blazor Components ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// --- Swagger Setup ---
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
                $"/swagger/latest/swagger.json",
                $"Geonorge nedlastings-API (latest)");

        options.SwaggerEndpoint(
                $"/swagger/v3/swagger.json",
                $"Geonorge nedlastings-API 3.0");

        options.SwaggerEndpoint(
                $"/swagger/internal/swagger.json",
                $"Geonorge nedlastings-API (internal)");

        //foreach (var description in provider.ApiVersionDescriptions.OrderByDescending(d => d.ApiVersion))
        //{
        //    options.SwaggerEndpoint(
        //        $"/swagger/{description.GroupName}/swagger.json",
        //        $"Geonorge nedlastings-API {description.ApiVersion}");
        //}
    });

// --- Middleware ---
app.UseCors("AllowAll"); // Or switch to a named policy as needed

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
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

app.Run();

public sealed record GcsSettings(string Bucket);