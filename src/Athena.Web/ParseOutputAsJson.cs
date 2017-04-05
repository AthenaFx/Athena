using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Athena.Web
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

            var serialized = JsonConvert.SerializeObject(output, Settings);

            return new ParsingResult("application/json", await serialized.ToStream().ConfigureAwait(false));
        }
    }
}