using System;
using System.Collections.Generic;
using Athena.Configuration;

namespace Athena
{
    internal static class AthenaContextExtensions
    {
        public static string ApplicationKey = "_application";
        public static string ContextKey = "_athenacontext";
        public static string RequestIdKey = "_requestId";
        
        public static IDisposable EnterApplication(this IDictionary<string, object> environment, AthenaContext context, 
            string application)
        {
            var previousApplication = environment.GetCurrentApplication();
            var previousContext = environment.GetCurrentContext();
            var previousRequestId = environment.GetRequestId();
            
            environment[ApplicationKey] = application;
            environment[ContextKey] = context;
            environment[RequestIdKey] = Guid.NewGuid().ToString("N");

            return new ApplicationDisposable(previousApplication, previousRequestId, previousContext, environment);
        }
        
        public static string GetCurrentApplication(this IDictionary<string, object> environment)
        {
            return environment.Get(ApplicationKey, "");
        }

        public static AthenaContext GetCurrentContext(this IDictionary<string, object> environment)
        {
            return environment.Get<AthenaContext>(ContextKey);
        }

        public static string GetRequestId(this IDictionary<string, object> environment)
        {
            return environment.Get(RequestIdKey, "");
        }

        private class ApplicationDisposable : IDisposable
        {
            private readonly string _previousApplication;
            private readonly string _previousRequestId;
            private readonly AthenaContext _previouseContext;
            private readonly IDictionary<string, object> _environment;

            public ApplicationDisposable(string previousApplication, string previousRequestId, 
                AthenaContext previouseContext, IDictionary<string, object> environment)
            {
                _previousApplication = previousApplication;
                _environment = environment;
                _previousRequestId = previousRequestId;
                _previouseContext = previouseContext;
            }

            public void Dispose()
            {
                _environment[ApplicationKey] = _previousApplication;
                _environment[ContextKey] = _previouseContext;
                _environment[RequestIdKey] = _previousRequestId;
            }
        }
    }
}