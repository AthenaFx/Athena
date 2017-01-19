using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Athena.Web
{
    public class ParseOutputAsJson : ResultParser
    {
        public async Task<ParsingResult> Parse(object output)
        {
            if (output == null)
                return null;

            var serialized = JsonConvert.SerializeObject(output);

            return new ParsingResult("application/json", await serialized.ToStream().ConfigureAwait(false));
        }

        public static bool Matches(IDictionary<string, object> environment)
        {
            return true;
            //return environment.GetRequest().Headers.Accept.Contains("application/json");
        }
    }
}