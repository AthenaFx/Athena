namespace Athena.Web.ModelBinding.ValueConverters
{
    public class SByteValueConverter : ParseValueConverter<sbyte>
    {
        protected override sbyte Parse(string stringValue, out bool success)
        {
            sbyte parsed;
            success = sbyte.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}