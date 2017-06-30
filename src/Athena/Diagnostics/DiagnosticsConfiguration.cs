using System.Collections.Generic;

namespace Athena.Diagnostics
{
    public class DiagnosticsConfiguration
    {
        private readonly ICollection<string> _allowedKeys = new List<string>();
        private readonly ICollection<string> _disAllowedKeys = new List<string>();
        private bool _allowAll;
        private bool _disAllowAll;

        public DiagnosticsConfiguration Allow(string key)
        {
            _allowedKeys.Add((key ?? "").ToLower());

            return this;
        }

        public DiagnosticsConfiguration Disallow(string key)
        {
            _disAllowedKeys.Add((key ?? "").ToLower());

            return this;
        }

        public DiagnosticsConfiguration AllowAll()
        {
            _allowAll = true;

            return this;
        }

        public DiagnosticsConfiguration DisallowAll()
        {
            _disAllowAll = true;

            return this;
        }

        public DiagnosticsConfiguration ResetGeneral()
        {
            _allowAll = false;
            _disAllowAll = false;

            return this;
        }

        internal bool IsKeyAllowed(string key)
        {
            if (_disAllowAll)
                return false;

            if (_allowAll)
                return true;

            return !_disAllowedKeys.Contains((key ?? "").ToLower()) && _allowedKeys.Contains((key ?? "").ToLower());
        }
    }
}