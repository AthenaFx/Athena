using System.Threading.Tasks;

namespace Athena.Web
{
    public interface ResultParser
    {
        Task<ParsingResult> Parse(object output);
    }
}