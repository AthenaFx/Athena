namespace Athena.Web.ModelBinding.ValueConverters
{
    public class DecimalValueConverter : ParseValueConverter<decimal>
    {
        protected override decimal Parse(string stringValue, out bool success)
        {
            decimal parsed;
            success = decimal.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}