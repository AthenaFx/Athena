using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Athena.Web.Parsing
{
    public class ParseOutputAsJson : ResultParser
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public string[] MatchingMediaTypes => new[]{"application/json"};

        public async Task<ParsingResult> Parse(object output)
        {
            if (output == null)
                return null;

            var streamOutput = output as Stream;

            if (streamOutput != null)
                return new ParsingResult("application/json", streamOutput);

            var stringOutput = output as string;

            if(stringOutput != null)
                return new ParsingResult("application/json", await stringOutput.ToStream().ConfigureAwait(false));

            var serialized = JsonConvert.SerializeObject(output, Settings);

            return new ParsingResult("application/json", await serialized.ToStream().ConfigureAwait(false));
        }
    }
}