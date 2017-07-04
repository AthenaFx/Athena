using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Configuration;
using Athena.Logging;
using Athena.MetaData;
using Athena.Resources;
using Athena.Routing;
using Athena.Transactions;

namespace Athena.CommandHandling
{
    public class CommandSenderConfiguration : AppFunctionDefinition
    {
        private readonly ICollection<Transaction> _transactions = new List<Transaction>();
        private readonly ICollection<MetaDataSupplier> _metaDataSuppliers = new List<MetaDataSupplier>();
        private Func<Type, IDictionary<string, object>, object> _createHandlerInstanceWith = 
            (x, y) => Activator.CreateInstance(x);
        
        public string Name { get; } = "commandhandler";
        
        public CommandSenderConfiguration HandleTransactionsWith(Transaction transaction)
        {
            Logger.Write(LogLevel.Debug, $"Handling command transactions with {transaction}");
            
            _transactions.Add(transaction);

            return this;
        }

        public CommandSenderConfiguration SupplyMetaDataWith(MetaDataSupplier supplier)
        {
            _metaDataSuppliers.Add(supplier);

            return this;
        }

        public CommandSenderConfiguration CreateHandlerInstanceWith(
            Func<Type, IDictionary<string, object>, object> createHandlerInstance)
        {
            _createHandlerInstanceWith = createHandlerInstance;

            return this;
        }
        
        protected override AppFunctionBuilder DefineDefaultApplication(AppFunctionBuilder builder)
        {
            var routers = new List<EnvironmentRouter>
            {
                RouteCommandToMethod.New(x => x.DeclaringType.Name.EndsWith("Handler") 
                                              && x.Name == "Handle"
                                              && (x.ReturnType == typeof(void) || x.ReturnType == typeof(Task))
                                              && x.GetParameters().Length > 0, 
                    builder.Bootstrapper.ApplicationAssemblies, _createHandlerInstanceWith)
            };

            var binders = new List<EnvironmentDataBinder>
            {
                new BindEnvironment(),
                new BindContext(),
                new CommandDataBinder()
            };

            var resourceExecutors = new List<ResourceExecutor>
            {
                new MethodResourceExecutor(binders)
            };
            
            var routeCheckers = new List<CheckIfResourceExists>
            {
                new CheckIfRouteExists()
            };
            
            return builder
                .First("HandleTransactions",
                    next => new HandleTransactions(next, _transactions.ToList()).Invoke,
                    () => _transactions.GetDiagnosticsData())
                .ContinueWith("SupplyMetaData", next => new SupplyMetaData(next, _metaDataSuppliers.ToList()).Invoke,
                    () => _metaDataSuppliers.GetDiagnosticsData())
                .ContinueWith("RouteToResource", next => new RouteToResource(next, routers).Invoke)
                .ContinueWith("EnsureEndpointExists", next => new EnsureEndpointExists(next, routeCheckers, x => 
                    throw new CommandHandlerNotFoundException(x.Get<object>("command").GetType())).Invoke,
                    () => routeCheckers.GetDiagnosticsData())
                .Last("ExecuteResource", next => new ExecuteResource(next, resourceExecutors).Invoke,
                    () => resourceExecutors.GetDiagnosticsData());
        }
    }
}