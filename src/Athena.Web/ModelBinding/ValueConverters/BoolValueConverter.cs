using System;

namespace Athena.Web.ModelBinding.ValueConverters
{
    public class BoolValueConverter : ParseValueConverter<bool>
    {
        protected override bool Parse(string stringValue, out bool success)
        {
            if (stringValue.Equals("on", StringComparison.CurrentCultureIgnoreCase))
            {
                success = true;
                return true;
            }

            bool parsed;
            success = bool.TryParse(stringValue, out parsed);

            return parsed;
        }
    }
}