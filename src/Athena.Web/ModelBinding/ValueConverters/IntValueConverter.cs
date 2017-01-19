namespace Athena.Web.ModelBinding.ValueConverters
{
    public class IntValueConverter : ParseValueConverter<int>
    {
        protected override int Parse(string stringValue, out bool success)
        {
            int parsed;
            success = int.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}