using System;
using System.Threading.Tasks;

namespace Athena.Web.ModelBinding
{
    public interface ModelBinder
    {
        bool Matches(Type type);
        Task<DataBinderResult> Bind(Type type, BindingContext bindingContext);
    }
}