using Athena.Resources;

namespace Athena.Web
{
    public interface FindStatusCodeFromResult
    {
        int FindFor(object result);
    }
}