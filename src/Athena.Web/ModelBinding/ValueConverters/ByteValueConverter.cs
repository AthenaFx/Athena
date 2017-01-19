namespace Athena.Web.ModelBinding.ValueConverters
{
    public class ByteValueConverter : ParseValueConverter<byte>
    {
        protected override byte Parse(string stringValue, out bool success)
        {
            byte parsed;
            success = byte.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}