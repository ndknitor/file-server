namespace FileServer.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILoggerFactory loggerFactory;
        public LoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.next = next;
            this.loggerFactory = loggerFactory;
        }
        public async Task Invoke(HttpContext context)
        {
            //string ip = context.Request.Headers["CF-CONNECTING-IP"].FirstOrDefault();
            string ip = context.Connection.RemoteIpAddress.ToString();
            var logger = loggerFactory.CreateLogger("LoggingService");
            var requestLog =
@$"📥📥📥📥📥 [REQUEST] 📥📥📥📥📥
🪪 Connection Id: {context.Connection.Id}
👤 Client IP: {ip}
🛣️ Path: {context.Request.Path}
🤖 Method: {context.Request.Method}
🔍 Query: {context.Request.QueryString}
📝 Content-Type: {context.Request.ContentType}
📏 Content-Length: {context.Request.ContentLength}
📥📥📥📥📥📥📥📥📥📥📥📥📥📥";
            logger.LogInformation(requestLog);
            await next(context);

            var responseLog =
@$"📤📤📤📤📤 [RESPONSE] 📤📤📤📤📤
🪪 Connection Id: {context.Connection.Id}
👤 Client IP: {ip}
🛣️ Path: {context.Request.Path}
🤖 Method: {context.Request.Method}
🔍 Query: {context.Request.QueryString}
🔢 Status Code: {context.Response.StatusCode}
📝 Content-Type: {context.Response.ContentType}
📏 Content-Length: {context.Response.ContentLength}
📤📤📤📤📤📤📤📤📤📤📤📤📤📤📤📤";

            if (context.Response.StatusCode < 300)
            {
                logger.LogInformation(responseLog);
            }
            else if (context.Response.StatusCode >= 400 && context.Response.StatusCode <= 500)
            {
                logger.LogWarning(responseLog);
            }
            else
            {
                logger.LogError(responseLog);
            }
        }
    }
}