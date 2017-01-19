using System;

namespace Athena.Web.ModelBinding.ValueConverters
{
    public abstract class ParseValueConverter<T> : ValueConverter where T : struct
    {
        public virtual bool Matches(Type destinationType)
        {
            return destinationType == typeof (T);
        }

        public virtual DataBinderResult Convert(Type destinationType, object value)
        {
            if (value == null) 
                return new DataBinderResult(null, false);

            if (value is T)
                return new DataBinderResult((T)value, true);

            bool success;
            var result = Parse(value.ToString(), out success);

            return new DataBinderResult(success ? result : default(T), success);
        }

        protected abstract T Parse(string stringValue, out bool success);
    }
}