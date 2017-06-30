using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.PartialApplications
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class RunPartialApplication
    {
        private readonly AppFunc _next;
        private readonly IEnumerable<Tuple<Func<IDictionary<string, object>, bool>, string>> _applications;

        public RunPartialApplication(AppFunc next, 
            IEnumerable<Tuple<Func<IDictionary<string, object>, bool>, string>> applications)
        {
            _applications = applications;
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var application = _applications.FirstOrDefault(x => x.Item1(environment));

            if (application != null)
                await environment.GetAthenaContext().Execute(application.Item2, environment).ConfigureAwait(false);

            await _next(environment);
        }
    }
}