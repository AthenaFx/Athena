using System;
using System.Collections.Generic;
using Athena.Configuration;

namespace Athena.Diagnostics
{
    public static class BootstrapperExtensions
    {
        public static PartConfiguration<DiagnosticsConfiguration> EnableDiagnostics(
            this AthenaBootstrapper bootstrapper, Func<IDictionary<string, object>, bool> enabledCheck = null)
        {
            return bootstrapper.Part<DiagnosticsConfiguration>().Configure(x => x.EnabledWhen(enabledCheck));
        }
    }
}