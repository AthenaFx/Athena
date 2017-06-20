using System;
using System.Collections.Concurrent;

namespace Athena.Settings
{
    public static class ApplicationSettings
    {
        private static readonly ConcurrentDictionary<Type, object> Settings = new ConcurrentDictionary<Type, object>();

        public static TSetting GetSettings<TSetting>() where TSetting : new()
        {
            return (TSetting) Settings.GetOrAdd(typeof(TSetting), x => new TSetting());
        }

        public static void AlterSettings<TSetting>(this AthenaBootstrapper bootstrapper, Func<TSetting, TSetting> alter) 
            where TSetting : new()
        {
            //TODO:Lock correctly
            var settings = GetSettings<TSetting>();

            Settings[typeof(TSetting)] = alter(settings);
        }
    }
}