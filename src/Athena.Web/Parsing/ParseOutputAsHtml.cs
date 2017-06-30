using System.IO;
using System.Threading.Tasks;

namespace Athena.Web.Parsing
{
    public class ParseOutputAsHtml : ResultParser
    {
        public string[] MatchingMediaTypes => new[]{"text/html"};
        
        public async Task<ParsingResult> Parse(object output)
        {
            if (output == null)
                return new ParsingResult("text/html", new MemoryStream());

            var streamOutput = output as Stream;

            if (streamOutput != null)
                return new ParsingResult("text/html", streamOutput);

            return new ParsingResult("text/html", await output.ToString().ToStream().ConfigureAwait(false));
        }
    }
}