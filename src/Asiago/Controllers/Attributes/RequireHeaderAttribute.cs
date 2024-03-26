using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.Primitives;

namespace Asiago.Controllers.Attributes
{
    /// <summary>
    /// Requires a request to have a particular header value to match this endpoint.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequireHeaderAttribute : Attribute, IActionConstraint
    {
        private readonly string _name;
        private readonly string _value;

        public RequireHeaderAttribute(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public int Order => 0;

        public bool Accept(ActionConstraintContext context)
        {
            IHeaderDictionary headers = context.RouteContext.HttpContext.Request.Headers;
            return headers.TryGetValue(_name, out StringValues values)
                && values.Contains(_value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
