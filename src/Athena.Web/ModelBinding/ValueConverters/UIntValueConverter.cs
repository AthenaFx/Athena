namespace Athena.Web.ModelBinding.ValueConverters
{
    public class UIntValueConverter : ParseValueConverter<uint>
    {
        protected override uint Parse(string stringValue, out bool success)
        {
            uint parsed;
            success = uint.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}