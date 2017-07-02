using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Configuration;
using Athena.MetaData;
using Athena.Resources;
using Athena.Routing;
using Athena.Transactions;

namespace Athena.CommandHandling
{
    public class CommandSenderConfiguration : AppFunctionDefinition
    {
        private readonly ICollection<Transaction> _transactions = new List<Transaction>();
        
        public string Name { get; } = "commandhandler";
        
        public CommandSenderConfiguration HandleTransactionsWith(Transaction transaction)
        {
            _transactions.Add(transaction);

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
                    builder.Bootstrapper.ApplicationAssemblies)
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
                .Last("HandleTransactions",
                    next => new HandleTransactions(next, _transactions.ToList()).Invoke)
                .Last("SupplyMetaData", next => new SupplyMetaData(next).Invoke)
                .Last("RouteToResource", next => new RouteToResource(next, routers).Invoke)
                .Last("EnsureEndpointExists", next => new EnsureEndpointExists(next, routeCheckers, x => 
                    throw new CommandHandlerNotFoundException(x.Get<object>("command").GetType())).Invoke)
                .Last("ExecuteResource", next => new ExecuteResource(next, resourceExecutors).Invoke);
        }
    }
}