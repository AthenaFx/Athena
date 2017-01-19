using System.Reflection;
using System.Threading.Tasks;

namespace Athena.Web.ModelBinding.PropertyBinders
{
    public interface PropertyBinder
    {
        bool Matches(PropertyInfo propertyInfo);
        Task<bool> Bind(object instance, PropertyInfo propertyInfo, BindingContext bindingContext);
    }
}