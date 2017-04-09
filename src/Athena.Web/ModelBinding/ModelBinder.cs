using System;
using System.Threading.Tasks;
using Athena.Binding;

namespace Athena.Web.ModelBinding
{
    public interface ModelBinder
    {
        bool Matches(Type type);
        Task<DataBinderResult> Bind(Type type, BindingContext bindingContext);
    }
}