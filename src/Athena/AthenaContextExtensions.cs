using System;
using System.Collections.Generic;

namespace Athena
{
    internal static class AthenaContextExtensions
    {
        public static string ApplicationKey = "_application";

        public static IDisposable EnterApplication(this IDictionary<string, object> environment, string application)
        {
            var previousApplication = environment.GetCurrentApplication();

            environment[ApplicationKey] = application;

            return new ApplicationDisposable(previousApplication, environment);
        }

        private class ApplicationDisposable : IDisposable
        {
            private readonly string _previousApplication;
            private readonly IDictionary<string, object> _environment;

            public ApplicationDisposable(string previousApplication, IDictionary<string, object> environment)
            {
                _previousApplication = previousApplication;
                _environment = environment;
            }

            public void Dispose()
            {
                _environment[ApplicationKey] = _previousApplication;
            }
        }
    }
}