﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Logging;
using Athena.PubSub;

namespace Athena.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public sealed class AthenaApplications : AthenaBootstrapper, AthenaSetupContext
    {
        private readonly Stopwatch _timer;

        private readonly ConcurrentDictionary<string, PartConfiguration> _partConfigurations =
            new ConcurrentDictionary<string, PartConfiguration>();

        private readonly ConcurrentBag<Tuple<Type, Func<object, bool>, Func<object, AthenaContext, Task>>>
            _shutdowHandlers = new ConcurrentBag<Tuple<Type, Func<object, bool>, Func<object, AthenaContext, Task>>>();

        private readonly IDictionary<string, AppFunctionBuilder> _applicationBuilders
            = new ConcurrentDictionary<string, AppFunctionBuilder>();

        public string ApplicationName { get; }

        public string Environment { get; }

        public TSetting GetSetting<TSetting>(string key = null) where TSetting : class
        {
            var settings = _partConfigurations.ToDictionary(x => x.Key, x => x.Value.GetPart());

            if (string.IsNullOrEmpty(key))
                key = typeof(TSetting).AssemblyQualifiedName;

            Logger.Write(LogLevel.Debug, $"Getting settings {typeof(TSetting)} with key {key}");
            
            return settings.ContainsKey(key) ? settings[key] as TSetting : null;
        }

        public PartConfiguration<TPart> Configure<TPart>(string key = null) where TPart : class, new()
        {
            if (string.IsNullOrEmpty(key))
                key = typeof(TPart).AssemblyQualifiedName;

            PartConfiguration configuration;

            if (!_partConfigurations.TryGetValue(key, out configuration))
            {
                throw new InvalidOperationException(
                    $"There is no configuration part of type {typeof(TPart)} with key {key}");
            }
            
            Logger.Write(LogLevel.Debug, $"Starting configuration for {typeof(TPart)} with key {key}");
            
            return (PartConfiguration<TPart>) configuration;
        }

        public PartConfiguration<TPart> ConfigureWith<TPart, TEvent>(
            Func<TPart, TEvent, AthenaSetupContext, Task> setup,
            Func<TEvent, bool> filter = null, string key = null)
            where TPart : class, new() where TEvent : SetupEvent
        {
            if (string.IsNullOrEmpty(key))
                key = typeof(TPart).AssemblyQualifiedName;
            
            Logger.Write(LogLevel.Debug, $"Starting configuration for {typeof(TPart)} with key {key}");

            var partConfiguration = (PartConfiguration<TPart>) _partConfigurations.GetOrAdd(key,
                x => new PartConfiguration<TPart>(this, key));

            partConfiguration.WithSetup(setup, filter);

            return partConfiguration;
        }

        public PartConfiguration<TPart> ConfigureWith<TPart>(string key = null) where TPart : class, new()
        {
            if (string.IsNullOrEmpty(key))
                key = typeof(TPart).AssemblyQualifiedName;
            
            Logger.Write(LogLevel.Debug, $"Starting configuration for {typeof(TPart)} with key {key}");
            
            var partConfiguration = (PartConfiguration<TPart>) _partConfigurations.GetOrAdd(key,
                x => new PartConfiguration<TPart>(this, key));

            return partConfiguration;
        }

        public AthenaBootstrapper ShutDownWith<TEvent>(Func<TEvent, AthenaContext, Task> shutDown,
            Func<TEvent, bool> filter = null)
            where TEvent : ShutdownEvent
        {
            Logger.Write(LogLevel.Debug, $"Adding shutdown handler for {typeof(TEvent)}");
            
            filter = filter ?? (x => true);

            _shutdowHandlers.Add(new Tuple<Type, Func<object, bool>, Func<object, AthenaContext, Task>>(typeof(TEvent),
                x => filter((TEvent) x), (evnt, context) => shutDown((TEvent) evnt, context)));

            return this;
        }

        public IReadOnlyCollection<Assembly> ApplicationAssemblies { get; }

        private AthenaApplications(string applicationName, string environment,
            IReadOnlyCollection<Assembly> applicationAssemblies)
        {
            Environment = environment;
            ApplicationAssemblies = applicationAssemblies;
            ApplicationName = applicationName;
            _timer = Stopwatch.StartNew();
        }

        public Task DefineApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> builder)
        {
            if (_applicationBuilders.ContainsKey(name))
                throw new InvalidOperationException($"There is already a application named {name}");
            
            Logger.Write(LogLevel.Debug, $"Defining application with name {name}");

            _applicationBuilders[name] = builder(new AppFunctionBuilder(this));

            return Done(new ApplicationDefined(name));
        }

        public Task UpdateApplication(string name, Func<AppFunctionBuilder, AppFunctionBuilder> builder)
        {
            if (!_applicationBuilders.ContainsKey(name))
                throw new InvalidOperationException($"There is no application named {name}");
            
            Logger.Write(LogLevel.Debug, $"Updating application \"{name}\"");

            _applicationBuilders[name] = builder(_applicationBuilders[name]);

            return Done(new ApplicationDefinitionModified(name));
        }

        public IReadOnlyCollection<string> GetDefinedApplications()
        {
            Logger.Write(LogLevel.Debug, $"Getting defined applications");
            
            return _applicationBuilders.Keys.ToList();
        }

        public async Task Done(SetupEvent evnt)
        {
            Logger.Write(LogLevel.Debug, $"{evnt} done");
            
            await Task.WhenAll(_partConfigurations
                    .Select(x => x.Value.TrySetUp(evnt, this)))
                .ConfigureAwait(false);

            await this.Publish(evnt).ConfigureAwait(false);
        }

        public async Task<AthenaContext> Build()
        {
            Logger.Write(LogLevel.Debug, $"Starting context build");
            
            await Done(new BootstrapStarted(ApplicationName, Environment)).ConfigureAwait(false);

            await Done(new BeforeApplicationsCompilation()).ConfigureAwait(false);

            var compilationResults = await Task.WhenAll(_applicationBuilders
                    .Select(x => CompileApplication(x.Key, x.Value)))
                .ConfigureAwait(false);

            var applications = compilationResults.ToDictionary(x => x.Item1, x => x.Item2);

            await Done(new ApplicationsCompiled()).ConfigureAwait(false);

            var context = new ApplicationsContext(ApplicationAssemblies, ApplicationName, Environment, applications,
                _shutdowHandlers, _partConfigurations.ToDictionary(x => x.Key, x => x.Value.GetPart()));

            await Done(new BootstrapCompleted(ApplicationName, Environment, _timer.Elapsed, context))
                .ConfigureAwait(false);
            
            Logger.Write(LogLevel.Debug, $"Context build finished");

            return context;
        }

        private async Task<Tuple<string, AppFunc>> CompileApplication(string name, AppFunctionBuilder builder)
        {
            var timer = Stopwatch.StartNew();
            
            var result = builder.Compile();
            
            timer.Stop();

            await Done(new ApplicationCompiled(name, result.Item2, timer.Elapsed));
            
            return new Tuple<string, AppFunc>(name, result.Item1);
        }
        
        public static AthenaBootstrapper From(string environment, params Assembly[] applicationAssemblies)
        {
            return From(environment, Assembly.GetEntryAssembly().GetName().Name.Replace(".", ""),
                applicationAssemblies);
        }

        public static AthenaBootstrapper From(string environment, string applicationName,
            params Assembly[] applicationAssemblies)
        {
            return new AthenaApplications(applicationName, environment, applicationAssemblies);
        }
    }
}