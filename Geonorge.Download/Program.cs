using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Geonorge.Download.Components;
using Geonorge.Download.Controllers.Api;
using Geonorge.Download.Models;
using Geonorge.Download.Services;
using Geonorge.Download.Services.Auth;
using Geonorge.Download.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Serilog;
using StackExchange.Redis;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Setup Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// --- Database ---
builder.Services.AddDbContext<DownloadContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

if (!builder.Environment.IsDevelopment())
{
    // --- Redis Data Protection ---
    string redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection") ?? throw new InvalidOperationException("Redis connection string is not configured.");
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
        options.GroupNameFormat = "'v'V"; // Produces "v3.0"
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
    options.OperationFilter<RemoveVersionParameterFilter>();
    options.OperationFilter<AuthorizeCheckOperationFilter>();

    //options.SwaggerDoc("v1", new OpenApiInfo { Title = "Download API v1", Version = "v1" });
    //options.SwaggerDoc("v2", new OpenApiInfo { Title = "Download API v2", Version = "v2" });
    options.SwaggerDoc("v3", new OpenApiInfo { Title = "Geonorge nedlastings-API", Version = "v3" });

    options.CustomSchemaIds(type => type.Name);    

    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        var metadata = apiDesc.ActionDescriptor.EndpointMetadata
            .OfType<ApiVersionMetadata>()
            .FirstOrDefault();

        if (metadata == null)
        {
            Console.WriteLine($"[SWAGGER DEBUG] Skipping {apiDesc.RelativePath} (no version metadata)");
            return false;
        }
        else if (apiDesc.RelativePath != null && apiDesc.RelativePath.StartsWith("api/v3"))
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
//if (app.Environment.IsDevelopment())
//{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in provider.ApiVersionDescriptions.OrderByDescending(d => d.ApiVersion))
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Geonorge nedlastings-API {description.ApiVersion}");
        }
        //options.RoutePrefix = "swagger";
        //options.ConfigObject.AdditionalItems["urls.primaryName"] = "v3";
    });
//}

// --- Middleware ---
app.UseCors("AllowAll"); // Or switch to a named policy as needed

//if (app.Environment.IsDevelopment())
//{
//    app.UseHttpsRedirection();
//}
//else
//{
    // If docker/k8s
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
    });
//}
app.UseAuthorization();

app.UseStaticFiles();
app.UseAntiforgery();

// --- Endpoints ---
app.MapControllers(); // For versioned REST API
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode(); // For Blazor pages

// TODO: Remove this in production!
//app.MapGet("/debug-secrets", (IConfiguration cfg) =>
//{
//    return new
//    {
//        Secret1 = cfg["GeoID:BaatAuthzApiCredentials"],
//        ExtToken = cfg["ExternalToken:Token"],
//        BasicAuthUser0 = cfg["basicAuth:users:0:username"],
//        BasicAuthUser1 = cfg["basicAuth:users:0:roles:0:name"]
//    };
//});


app.Run();
