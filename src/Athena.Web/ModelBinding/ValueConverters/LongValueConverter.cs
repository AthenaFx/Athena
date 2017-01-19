namespace Athena.Web.ModelBinding.ValueConverters
{
    public class LongValueConverter : ParseValueConverter<long>
    {
        protected override long Parse(string stringValue, out bool success)
        {
            long parsed;
            success = long.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}