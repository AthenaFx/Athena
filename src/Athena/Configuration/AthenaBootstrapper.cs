using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena.Configuration
{
    public interface AthenaBootstrapper
    {
        string ApplicationName { get; }
        string Environment { get; }
        IReadOnlyCollection<Assembly> ApplicationAssemblies { get; }
        
        PartConfiguration<TPart> Part<TPart>(string key = null) where TPart : class, new();

        Task<AthenaContext> Build();
    }
}