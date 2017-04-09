using System.Threading.Tasks;

namespace Athena.Web.Parsing
{
    public interface ResultParser
    {
        string[] MatchingMediaTypes { get; }
        Task<ParsingResult> Parse(object output);
    }
}