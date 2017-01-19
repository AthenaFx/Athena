namespace Athena.Web.ModelBinding.ValueConverters
{
    public class CharValueConverter : ParseValueConverter<char>
    {
        protected override char Parse(string stringValue, out bool success)
        {
            char parsed;
            success = char.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}