using System;

namespace Athena.Web.ModelBinding.ValueConverters
{
    public class UriValueConverter : ValueConverter
    {
        public bool Matches(Type destinationType)
        {
            return destinationType == typeof (Uri);
        }

        public DataBinderResult Convert(Type destinationType, object value)
        {
            if(value == null)
                return new DataBinderResult(null, false);

            Uri result;
            var success = Uri.TryCreate(value.ToString(), UriKind.RelativeOrAbsolute, out result);

            return new DataBinderResult(result, success);
            
        }
    }
}