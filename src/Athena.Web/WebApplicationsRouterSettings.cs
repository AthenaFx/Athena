using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Athena.Configuration;

namespace Athena.Web
{
    public class WebApplicationsRouterSettings
    {
        private readonly ICollection<Tuple<int, Func<IDictionary<string, object>, bool>,
            WebApplicationSettings>> _webApplications =
            new Collection<Tuple<int, Func<IDictionary<string, object>, bool>,
                WebApplicationSettings>>();

        public WebApplicationsRouterSettings AddApplication(WebApplicationSettings settings, 
            Func<IDictionary<string, object>, WebApplicationSettings, bool> filter = null)
        {
            var order = 1;

            if (filter == null)
                order = int.MaxValue;

            filter = filter ?? ((x, y) => true);

            if (_webApplications.Any(x => x.Item3.Name == settings.Name))
            {
                throw new InvalidOperationException(
                    $"There is already a application named {settings.Name}");
            }

            _webApplications.Add(new Tuple<int, Func<IDictionary<string, object>, bool>, 
                WebApplicationSettings>(order, env => filter(env, settings), settings));

            return this;
        }

        internal IReadOnlyCollection<Tuple<int, Func<IDictionary<string, object>, bool>,
            WebApplicationSettings>> GetApplications()
        {
            return _webApplications.ToList();
        }
    }
}