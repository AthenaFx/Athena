using System.Threading.Tasks;

namespace Athena.Web
{
    public interface ResultParser
    {
        string[] MatchingMediaTypes { get; }
        Task<ParsingResult> Parse(object output);
    }
}