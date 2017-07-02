using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Athena.Configuration;

namespace Athena.Web
{
    public class WebApplicationsSettings
    {
        private readonly ICollection<Tuple<int, Func<IDictionary<string, object>, bool>,
            AppFunctionDefinition>> _webApplications =
            new Collection<Tuple<int, Func<IDictionary<string, object>, bool>,
                AppFunctionDefinition>>();

        public WebApplicationsSettings AddApplication<TSettings>(TSettings settings, 
            Func<IDictionary<string, object>, TSettings, bool> filter = null) where TSettings : AppFunctionDefinition
        {
            var order = 1;

            if (filter == null)
                order = int.MaxValue;

            filter = filter ?? ((x, y) => true);

            if (_webApplications.Any(x => x.Item3.Name == settings.Name && x.Item3.GetType() == typeof(TSettings)))
            {
                throw new InvalidOperationException(
                    $"There is already a application named {settings.Name} of type {typeof(TSettings)}");
            }

            _webApplications.Add(new Tuple<int, Func<IDictionary<string, object>, bool>, 
                AppFunctionDefinition>(order, env => filter(env, settings), settings));

            return this;
        }

        internal IReadOnlyCollection<Tuple<int, Func<IDictionary<string, object>, bool>,
            AppFunctionDefinition>> GetApplications()
        {
            return _webApplications.ToList();
        }
    }
}