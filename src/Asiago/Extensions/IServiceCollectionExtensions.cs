using Coravel.Invocable;
using System.Reflection;

namespace Asiago.Extensions
{
    /// <summary>
    /// Extension methods on <see cref="IServiceCollection"/>.
    /// </summary>
    internal static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all types from the <paramref name="namespaceName"/> implementing the <see cref="IInvocable"/> interface found in the specified <paramref name="assembly"/>.
        /// </summary>
        public static IServiceCollection AddInvocablesFromNamespace(
            this IServiceCollection services,
            string namespaceName,
            Assembly assembly
            )
        {
            IEnumerable<Type> invocableTypes = assembly.GetTypes()
                .Where(t => t.Namespace != null
                && (t.Namespace == namespaceName || t.Namespace.StartsWith(namespaceName + "."))
                && t.GetInterface(typeof(IInvocable).Name) != null);

            foreach (var invocableType in invocableTypes)
            {
                services.AddTransient(invocableType);
            }

            return services;
        }
    }
}
