using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena
{
    public interface AthenaBootstrapper
    {
        void DefineApplication(string name, Func<IDictionary<string, object>, Task> app, bool overwrite = true);   
    }
}