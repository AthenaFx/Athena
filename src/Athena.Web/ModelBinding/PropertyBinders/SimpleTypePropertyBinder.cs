using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Web.ModelBinding.BindingSources;
using Athena.Web.ModelBinding.ValueConverters;

namespace Athena.Web.ModelBinding.PropertyBinders
{
    public class SimpleTypePropertyBinder : PropertyBinder
    {
        private readonly IReadOnlyCollection<ValueConverter> _valueConverters;
        private readonly IReadOnlyCollection<BindingSource> _bindingSources;

        public SimpleTypePropertyBinder(IReadOnlyCollection<ValueConverter> valueConverters, IReadOnlyCollection<BindingSource> bindingSources)
        {
            _valueConverters = valueConverters;
            _bindingSources = bindingSources;
        }

        public bool Matches(PropertyInfo propertyInfo)
        {
            return _valueConverters.CanConvert(propertyInfo.PropertyType);
        }

        public async Task<bool> Bind(object instance, PropertyInfo propertyInfo, BindingContext bindingContext)
        {
            if (!await _bindingSources.ContainsKey(bindingContext.GetKey(propertyInfo.Name), bindingContext.Environment).ConfigureAwait(false))
                return false;

            var conversionResult = _valueConverters.Convert(propertyInfo.PropertyType, await _bindingSources.Get(bindingContext.GetKey(propertyInfo.Name), bindingContext.Environment).ConfigureAwait(false), bindingContext);

            if (!conversionResult.Success)
                return false;

            propertyInfo.SetValue(instance, conversionResult.Result, new object[0]);

            return true;
        }
    }
}