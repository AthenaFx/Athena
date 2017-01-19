namespace Athena.Web.ModelBinding.ValueConverters
{
    public class ShortValueConverter : ParseValueConverter<short>
    {
        protected override short Parse(string stringValue, out bool success)
        {
            short parsed;
            success = short.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}