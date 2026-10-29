using Microsoft.AspNetCore.Mvc.Filters;

namespace Geonorge.Download.Controllers.Api
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ApiVersionHeaderAttribute(string version) : Attribute, IAsyncResourceFilter, IAsyncAlwaysRunResultFilter
    {
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            context.HttpContext.Response.Headers["X-API-VERSION"] = version;
            await next();
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (!context.HttpContext.Response.HasStarted)
                context.HttpContext.Response.Headers["X-API-VERSION"] = version;
            await next();
        }
    }
}