using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FileServer.Filters
{
    public class ServerAuthorizationAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            IConfiguration configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
            string fileAuthTokenKey = "FileAuthToken",
            fileAuthToken = configuration.GetValue<string>(fileAuthTokenKey),
            requestFileAuthToken = context.HttpContext.Request.Headers[fileAuthTokenKey];
            if (fileAuthToken == requestFileAuthToken)
            {
                await next();
            }
            else
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.HttpContext.Response.Body.Close();
            }
        }
    }

}