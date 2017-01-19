using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Web.ModelBinding.ValueConverters;

namespace Athena.Web.ModelBinding.PropertyBinders
{
    public class ComplexTypePropertyBinder : PropertyBinder
    {
        private readonly IReadOnlyCollection<ValueConverter> _valueConverters;

        public ComplexTypePropertyBinder(IReadOnlyCollection<ValueConverter> valueConverters)
        {
            _valueConverters = valueConverters;
        }

        public bool Matches(PropertyInfo propertyInfo)
        {
            return !_valueConverters.CanConvert(propertyInfo.PropertyType);
        }

        public async Task<bool> Bind(object instance, PropertyInfo propertyInfo, BindingContext bindingContext)
        {
            using (bindingContext.OpenChildContext($"{propertyInfo.Name}_"))
            {
                var result = await bindingContext.Bind(propertyInfo.PropertyType).ConfigureAwait(false);

                if (!result.Success)
                    return false;

                propertyInfo.SetValue(instance, result.Result, new object[0]);

                return true;
            }
        }
    }
}