namespace Asiago.Controllers.Attributes
{
    /// <summary>
    /// Indicates that the HTTP request body should be buffered so that it can be read mutiple times.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    internal class EnableRequestBodyBufferingAttribute : Attribute
    {
    }
}
