using System;

namespace Athena.Web.ModelBinding.ValueConverters
{
    public class DateTimeValueConverter : ParseValueConverter<DateTime>
    {
        protected override DateTime Parse(string stringValue, out bool success)
        {
            DateTime parsed;
            success = DateTime.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}