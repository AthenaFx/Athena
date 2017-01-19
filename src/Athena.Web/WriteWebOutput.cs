using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.Web
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class WriteWebOutput
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<Tuple<Func<IDictionary<string, object>, bool>, ResultParser>> _parsers;

        public WriteWebOutput(AppFunc next, IReadOnlyCollection<Tuple<Func<IDictionary<string, object>, bool>, ResultParser>> parsers)
        {
            _next = next;
            _parsers = parsers;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var outputResults = environment.Get("endpointresults", new List<object>());

            var parser = _parsers.FirstOrDefault(x => x.Item1(environment));

            if (parser == null)
                return;

            foreach (var result in outputResults)
            {
                var outputResult = await parser.Item2.Parse(result);

                var response = environment.GetResponse();

                response.Headers.ContentType = outputResult.ContentType;

                if (outputResult.Body == null) continue;

                using (outputResult.Body)
                    await response.Write(outputResult.Body).ConfigureAwait(false);
            }
        }
    }
}