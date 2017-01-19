using System;

namespace Athena.Web.ModelBinding.ValueConverters
{
    public class TimeSpanValueConverter : ParseValueConverter<TimeSpan>
    {
        protected override TimeSpan Parse(string stringValue, out bool success)
        {
            TimeSpan timeSpan;
            success = TimeSpan.TryParse(stringValue, out timeSpan);
            return timeSpan;
        }
    }
}