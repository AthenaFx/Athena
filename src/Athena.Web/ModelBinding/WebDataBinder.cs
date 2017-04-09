using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Binding;

namespace Athena.Web.ModelBinding
{
    public class WebDataBinder : EnvironmentDataBinder
    {
        private readonly IReadOnlyCollection<ModelBinder> _modelBinders;

        public WebDataBinder(IReadOnlyCollection<ModelBinder> modelBinders)
        {
            _modelBinders = modelBinders;
        }

        public Task<DataBinderResult> Bind(Type to, IDictionary<string, object> environment)
        {
            return _modelBinders.Bind(to, new DefaultBindingContext(_modelBinders, environment));
        }
    }
}