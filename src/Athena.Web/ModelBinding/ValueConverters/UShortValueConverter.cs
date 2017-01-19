namespace Athena.Web.ModelBinding.ValueConverters
{
    public class UShortValueConverter : ParseValueConverter<ushort>
    {
        protected override ushort Parse(string stringValue, out bool success)
        {
            ushort parsed;
            success = ushort.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}