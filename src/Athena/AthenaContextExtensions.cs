using System;
using System.Collections.Generic;

namespace Athena
{
    internal static class AthenaContextExtensions
    {
        public static string ApplicationKey = "_application";
        public static string ContextKey = "_athenacontext";
        
        public static IDisposable EnterApplication(this IDictionary<string, object> environment, AthenaContext context, 
            string application)
        {
            var previousApplication = environment.GetCurrentApplication();
            var previousContext = environment.GetCurrentContext();
            
            environment[ApplicationKey] = application;
            environment[ContextKey] = context;

            return new ApplicationDisposable(previousApplication, previousContext, environment);
        }
        
        public static string GetCurrentApplication(this IDictionary<string, object> environment)
        {
            return environment.Get(ApplicationKey, "");
        }

        public static AthenaContext GetCurrentContext(this IDictionary<string, object> environment)
        {
            return environment.Get<AthenaContext>(ContextKey);
        }

        private class ApplicationDisposable : IDisposable
        {
            private readonly string _previousApplication;
            private readonly AthenaContext _previouseContext;
            private readonly IDictionary<string, object> _environment;

            public ApplicationDisposable(string previousApplication, AthenaContext previouseContext, 
                IDictionary<string, object> environment)
            {
                _previousApplication = previousApplication;
                _environment = environment;
                _previouseContext = previouseContext;
            }

            public void Dispose()
            {
                _environment[ApplicationKey] = _previousApplication;
                _environment[ContextKey] = _previouseContext;
            }
        }
    }
}