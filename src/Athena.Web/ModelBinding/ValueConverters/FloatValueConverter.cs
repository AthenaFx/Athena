namespace Athena.Web.ModelBinding.ValueConverters
{
    public class FloatValueConverter : ParseValueConverter<float>
    {
        protected override float Parse(string stringValue, out bool success)
        {
            float parsed;
            success = float.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}