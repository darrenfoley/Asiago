using Asiago.Middlewares;

namespace Asiago.Extensions
{
    /// <summary>
    /// Extension methods on <see cref="IApplicationBuilder"/>.
    /// </summary>
    internal static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="RequestBodyBufferingMiddleware"/> to the request pipeline.
        /// </summary>
        internal static IApplicationBuilder UseRequestBodyBuffering(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestBodyBufferingMiddleware>();
        }
    }
}
