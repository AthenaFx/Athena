using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Web.ModelBinding
{
    public interface BindingContext
    {
        Task<DataBinderResult> Bind(Type type);
        void PrefixWith(string prefix);
        string GetKey(string name);
        string GetPrefix();
        IDisposable OpenChildContext(string prefix);

        IDictionary<string, object> Environment { get; }
    }
}