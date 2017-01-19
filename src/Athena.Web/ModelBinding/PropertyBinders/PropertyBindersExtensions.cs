using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Logging;

namespace Athena.Web.ModelBinding.PropertyBinders
{
    public static class PropertyBindersExtensions
    {
        public static async Task<bool> Bind(this IReadOnlyCollection<PropertyBinder> propertyBinders, object instance, PropertyInfo propertyInfo, BindingContext bindingContext)
        {
            var binder = propertyBinders.FirstOrDefault(x => x.Matches(propertyInfo));

            if (binder == null)
            {
                Logger.Write(LogLevel.Info, $"Failed to find a propertybinder for property: {propertyInfo.Name} on: {propertyInfo.DeclaringType?.Name ?? ""}.");
                return false;
            }

            Logger.Write(LogLevel.Debug, $"Going to bind property: {propertyInfo.Name} on: {propertyInfo.DeclaringType?.Name ?? ""} using: {binder}.");

            var result = await binder.Bind(instance, propertyInfo, bindingContext).ConfigureAwait(false);

            Logger.Write(LogLevel.Debug, $"Finished binding property: {propertyInfo.Name} on: {propertyInfo.DeclaringType?.Name} using: {binder} with result: Success = {result}.");

            return result;
        }
    }
}