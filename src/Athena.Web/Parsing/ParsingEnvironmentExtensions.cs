using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Logging;

namespace Athena.Web.Parsing
{
    public static class ParsingEnvironmentExtensions
    {
        public static IDisposable UsingParser(this IDictionary<string, object> environment, ResultParser parser)
        {
            var previousParser = environment.Get<ResultParser>("result-parser");

            environment["result-parser"] = parser;

            return new ParserDisposable(() => environment["result-parser"] = previousParser);
        }

        public static async Task ParseAndWrite(this IDictionary<string, object> environment, object output)
        {
            var response = environment.GetResponse();

            var parser = environment.Get<ResultParser>("result-parser");

            if (parser == null)
            {
                Logger.Write(LogLevel.Debug, $"No parser found for request {environment.GetRequestId()}");
                
                await response.Write((output ?? "").ToString()).ConfigureAwait(false);

                return;
            }

            var outputResult = await parser.Parse(output);

            response.Headers.ContentType = outputResult.ContentType;
            
            Logger.Write(LogLevel.Debug, $"Writing body using parser {parser}");

            if (outputResult.Body == null)
                return;

            using (outputResult.Body)
                await response.Write(outputResult.Body).ConfigureAwait(false);
        }

        private class ParserDisposable : IDisposable
        {
            private readonly Action _dispose;

            public ParserDisposable(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                _dispose();
            }
        }
    }
}