using System;

namespace Athena.Configuration
{
    public static class AppFunctionDefinitionExtensions
    {
        public static TAppDefinition ModifyApplication<TAppDefinition>(this TAppDefinition definition,
            Func<AppFunctionBuilder, AppFunctionBuilder> modifier)
            where TAppDefinition : AppFunctionDefinition
        {
            definition.ModifyWith(modifier);

            return definition;
        }
    }
}