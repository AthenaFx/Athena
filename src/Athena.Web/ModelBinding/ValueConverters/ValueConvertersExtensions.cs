using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Binding;
using Athena.Logging;

namespace Athena.Web.ModelBinding.ValueConverters
{
    public static class ValueConvertersExtensions
    {
        public static bool CanConvert(this IReadOnlyCollection<ValueConverter> valueConverters, Type destinationType)
        {
            return GetMatchingConverters(valueConverters, destinationType).Any();
        }

        public static DataBinderResult Convert(this IReadOnlyCollection<ValueConverter> valueConverters, Type destinationType, object value, BindingContext context)
        {
            var converter = GetMatchingConverters(valueConverters, destinationType).FirstOrDefault();

            if (converter == null)
            {
                Logger.Write(LogLevel.Debug, $"Failed to find a matching converter for type: {destinationType?.Name ?? "null"}");

                return new DataBinderResult(null, false);
            }

            var result = converter.Convert(destinationType, value);

            Logger.Write(LogLevel.Debug,
                $"Converted value: \"{value ?? ""}\" to type: {destinationType?.Name ?? "null"} using converted: {converter.GetType().Name} with result: Success = {result.Success}.");

            return result;
        }

        private static IEnumerable<ValueConverter> GetMatchingConverters(IEnumerable<ValueConverter> valueConverters, Type destinationType)
        {
            return valueConverters.Where(x => x.Matches(destinationType)).ToList().AsReadOnly();
        }
    }
}