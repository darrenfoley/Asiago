namespace Asiago.Core.Web
{
    /// <summary>
    /// Web options.
    /// </summary>
    internal class WebOptions
    {
        /// <summary>
        /// Base URL for the app.
        /// </summary>
        public required Uri BaseUrl { get; set; }
    }
}
