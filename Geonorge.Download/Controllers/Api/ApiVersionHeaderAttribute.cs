using Microsoft.AspNetCore.Mvc.Filters;

namespace Geonorge.Download.Controllers.Api
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ApiVersionHeaderAttribute(string version) : Attribute, IAsyncAlwaysRunResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            context.HttpContext.Response.Headers["X-API-VERSION"] = version;
            await next();
        }
    }
}