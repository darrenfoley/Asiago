using Coravel;
using Coravel.Queuing.Interfaces;

namespace Asiago.Extensions
{
    /// <summary>
    /// Extension methods on <see cref="WebApplication"/>.
    /// </summary>
    internal static class WebApplicationExtensions
    {
        /// <summary>
        /// Enable logging of errors and task progress for the Coravel Queue.
        /// </summary>
        public static WebApplication EnableQueueLogging(this WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<IQueue>>();
            app.Services.ConfigureQueue()
                .OnError(ex => logger.LogError(ex, "An unhandled exception occured while processing a background task."))
                .LogQueuedTaskProgress(logger);

            return app;
        }
    }
}
