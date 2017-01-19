using System.Collections.Generic;
using Athena.Web.ModelBinding.BindingSources;
using Athena.Web.ModelBinding.PropertyBinders;
using Athena.Web.ModelBinding.ValueConverters;

namespace Athena.Web.ModelBinding
{
    public static class ModelBinders
    {
        public static IReadOnlyCollection<ModelBinder> GetAll()
        {
            var valueConverters = new List<ValueConverter>
            {
                new BoolValueConverter(),
                new ByteValueConverter(),
                new CharValueConverter(),
                new DateTimeValueConverter(),
                new DecimalValueConverter(),
                new DoubleValueConverter(),
                new FloatValueConverter(),
                new IntValueConverter(),
                new LongValueConverter(),
                new SByteValueConverter(),
                new ShortValueConverter(),
                new StringValueConverter(),
                new TimeSpanValueConverter(),
                new UIntValueConverter(),
                new ULongValueConverter(),
                new UriValueConverter(),
                new UShortValueConverter()
            };

            var bindingSources = new List<BindingSource>
            {
                new CookieBindingSource(),
                new FormDataBindingSource(),
                new JsonRequestBodyBindingSource(),
                new PostedFilesBindingSource(),
                new QueryStringBindingSource()
            };

            return new List<ModelBinder>
            {
                new DefaultModelBinder(new List<PropertyBinder>
                {
                    new CollectionPropertyBinder(),
                    new ComplexTypePropertyBinder(valueConverters),
                    new SimpleTypePropertyBinder(valueConverters, bindingSources)
                })
            };
        }
    }
}