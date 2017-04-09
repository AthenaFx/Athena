using System;
using Athena.Binding;

namespace Athena.Web.ModelBinding.ValueConverters
{
    public class StringValueConverter : ValueConverter
    {
        public bool Matches(Type destinationType)
        {
            return destinationType == typeof (string);
        }

        public DataBinderResult Convert(Type destinationType, object value)
        {
            return value == null ? new DataBinderResult(null, false) : new DataBinderResult(value.ToString(), true);
        }
    }
}