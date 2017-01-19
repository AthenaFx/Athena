namespace Athena.Web.ModelBinding.ValueConverters
{
    public class ULongValueConverter : ParseValueConverter<ulong>
    {
        protected override ulong Parse(string stringValue, out bool success)
        {
            ulong parsed;
            success = ulong.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}