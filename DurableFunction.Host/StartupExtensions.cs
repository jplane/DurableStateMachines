using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateChartsDotNet.DurableFunction.Host
{
    public static class StartupExtensions
    {
        public static void UseDurableStateMachines(this IServiceCollection collection)
        {
        }

        public static void UseDurableStateMachines<TDefinitionFactory>(this IServiceCollection collection)
            where TDefinitionFactory : class, IStateMachineFactory
        {
            collection.AddScoped<IStateMachineFactory, TDefinitionFactory>();
            collection.UseDurableStateMachines();
        }
    }
}
