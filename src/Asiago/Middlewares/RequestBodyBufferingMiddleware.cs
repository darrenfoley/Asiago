using Asiago.Controllers.Attributes;

namespace Asiago.Middlewares
{
    /// <summary>
    /// Allows the request body to be read multiple times in endpoints with <see cref="EnableRequestBodyBufferingAttribute"/> on them.
    /// </summary>
    internal class RequestBodyBufferingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestBodyBufferingMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<EnableRequestBodyBufferingAttribute>() != null)
            {
                context.Request.EnableBuffering();
            }

            await _next(context);
        }
    }
}
