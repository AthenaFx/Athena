using System;
using System.Collections.Generic;

namespace Athena.Configuration
{
    public abstract class AppFunctionDefinition
    {
        private readonly ICollection<Func<AppFunctionBuilder, AppFunctionBuilder>> _modifiers 
            = new List<Func<AppFunctionBuilder, AppFunctionBuilder>>();

        public abstract string Name { get; }
        
        internal virtual void ModifyWith(Func<AppFunctionBuilder, AppFunctionBuilder> modifier)
        {
            _modifiers.Add(modifier);
        }

        public virtual Func<AppFunctionBuilder, AppFunctionBuilder> GetApplicationBuilder()
        {
            Func<AppFunctionBuilder, AppFunctionBuilder> currentItem = DefineDefaultApplication;

            foreach (var modifier in _modifiers)
            {
                var lastItem = currentItem;
                
                currentItem = builder => modifier(lastItem(builder));
            }

            return currentItem;
        }

        protected abstract AppFunctionBuilder DefineDefaultApplication(AppFunctionBuilder builder);
    }
}