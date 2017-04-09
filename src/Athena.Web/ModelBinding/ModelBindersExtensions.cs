using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Logging;

namespace Athena.Web.ModelBinding
{
    public static class ModelBindersExtensions
    {
        public static async Task<DataBinderResult> Bind(this IReadOnlyCollection<ModelBinder> modelBinders, Type type, BindingContext bindingContext)
        {
            var binder = GetMatchingBinders(modelBinders, type).FirstOrDefault();

            if (binder == null)
            {
                Logger.Write(LogLevel.Debug, $"Failed to find a matching modelbinder for type: {type}");

                return new DataBinderResult(null, false);
            }

            Logger.Write(LogLevel.Debug, $"Going to bind type: {type} using {binder}.");

            var result = await binder.Bind(type, bindingContext).ConfigureAwait(false);

            Logger.Write(LogLevel.Debug, $"Finished binding type: {type} using {binder}. Result: IsValid = {result.Success}, Instance = {result.Result?.ToString() ?? "null"}.");

            return result;
        }

        private static IEnumerable<ModelBinder> GetMatchingBinders(IEnumerable<ModelBinder> modelBinders, Type type)
        {
            return modelBinders.Where(x => x.Matches(type));
        }
    }
}