using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Binding;

namespace Athena.Web.ModelBinding
{
    public class DefaultBindingContext : BindingContext
    {
        private string _currentPrefix;
        private readonly IReadOnlyCollection<ModelBinder> _modelBinders;

        public DefaultBindingContext(IReadOnlyCollection<ModelBinder> modelBinders, IDictionary<string, object> environment)
            : this(modelBinders, environment, "")
        {
            
        }

        public DefaultBindingContext(IReadOnlyCollection<ModelBinder> modelBinders, IDictionary<string, object> environment, string prefix)
        {
            _currentPrefix = prefix;
            _modelBinders = modelBinders;
            Environment = environment;
        }

        public Task<DataBinderResult> Bind(Type type)
        {
            return _modelBinders.Bind(type, this);
        }

        public void PrefixWith(string prefix)
        {
            _currentPrefix = $"{_currentPrefix}{prefix}";
        }

        public string GetKey(string name)
        {
            return $"{_currentPrefix}{name}".ToLower();
        }

        public string GetPrefix()
        {
            return _currentPrefix;
        }

        public IDisposable OpenChildContext(string prefix)
        {
            var oldPrefix = _currentPrefix;
            
            PrefixWith(prefix.ToLower());

            return new Disposable(oldPrefix, x => _currentPrefix = x);
        }

        public IDictionary<string, object> Environment { get; }

        private class Disposable : IDisposable
        {
            private readonly string _oldPrefix;
            private readonly Action<string> _reset;

            public Disposable(string oldPrefix, Action<string> reset)
            {
                _oldPrefix = oldPrefix;
                _reset = reset;
            }

            public void Dispose()
            {
                _reset(_oldPrefix);
            }
        }
    }
}