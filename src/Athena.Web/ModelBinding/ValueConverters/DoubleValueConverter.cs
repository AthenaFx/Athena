namespace Athena.Web.ModelBinding.ValueConverters
{
    public class DoubleValueConverter : ParseValueConverter<double>
    {
        protected override double Parse(string stringValue, out bool success)
        {
            double parsed;
            success = double.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}