using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena
{
    public interface AthenaContext
    {
        void DefineApplication(string name, Func<IDictionary<string, object>, Task> app);
        Task Execute(string application, IDictionary<string, object> environment);
    }
}