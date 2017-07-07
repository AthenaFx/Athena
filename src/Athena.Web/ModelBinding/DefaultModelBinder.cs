using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Web.ModelBinding.PropertyBinders;

namespace Athena.Web.ModelBinding
{
    public class DefaultModelBinder : ModelBinder
    {
        private readonly IReadOnlyCollection<PropertyBinder> _propertyBinders;

        public DefaultModelBinder(IReadOnlyCollection<PropertyBinder> propertyBinders)
        {
            _propertyBinders = propertyBinders;
        }

        public bool Matches(Type type)
        {
            return type.GetTypeInfo().GetConstructors().Count(x => x.GetParameters().Length == 0) == 1;
        }

        public async Task<DataBinderResult> Bind(Type type, BindingContext bindingContext)
        {
            var instance = Activator.CreateInstance(type);

            var binderTasks = type
                .GetTypeInfo()
                .GetProperties()
                .Where(x => x.CanWrite)
                .Select(x => _propertyBinders.Bind(instance, x, bindingContext));

            var success = (await Task.WhenAll(binderTasks).ConfigureAwait(false)).Any(x => x);

            return new DataBinderResult(instance, success);
        }
    }
}