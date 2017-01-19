using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena
{
    public static class DataBinder
    {
        public static async Task<T> Bind<T>(this IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders,
            IDictionary<string, object> environment, T defaultValue = default(T))
        {
            var data = await Bind(environmentDataBinders, typeof(T), environment, defaultValue).ConfigureAwait(false);

            return (T) data;
        }

        public static async Task<object> Bind(this IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders, Type to,
            IDictionary<string, object> environment, object defaultValue = null)
        {
            foreach (var dataBinder in environmentDataBinders)
            {
                var result = await dataBinder.Bind(to, environment);

                if (result.Success)
                    return result.Result;
            }

            return defaultValue;
        }
    }
}