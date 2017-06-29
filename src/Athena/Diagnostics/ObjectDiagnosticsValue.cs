namespace Athena.Diagnostics
{
    public class ObjectDiagnosticsValue : DiagnosticsValue
    {
        private readonly object _value;

        public ObjectDiagnosticsValue(object value)
        {
            _value = value;
        }

        public string GetStringRepresentation()
        {
            return _value.ToString();
        }
    }
}