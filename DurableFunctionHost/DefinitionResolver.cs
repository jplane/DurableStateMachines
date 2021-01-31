using Microsoft.Extensions.Configuration;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.DurableFunctionClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace StateChartsDotNet.DurableFunctionHost
{
    public interface IStateMachineDefinitionProvider
    {
        IStateChartMetadata GetStateMachine(string identifier);
    }

    public interface IStateMachineResolver
    {
        IStateChartMetadata Resolve(string identifier);
    }

    internal class DefinitionResolver : IStateMachineResolver
    {
        private readonly IConfiguration _config;
        private readonly Lazy<IStateMachineDefinitionProvider[]> _providers;
        private readonly ConcurrentDictionary<string, IStateChartMetadata> _funcs;

        public DefinitionResolver(IConfiguration config)
        {
            _config = config;

            _funcs = new ConcurrentDictionary<string, IStateChartMetadata>();

            _providers = new Lazy<IStateMachineDefinitionProvider[]>(() =>
                            Assembly.Load(_config["DEFINITIONS_ASSEMBLY"])  // TODO: change this
                                    .GetTypes()
                                    .Where(t => typeof(IStateMachineDefinitionProvider).IsAssignableFrom(t) &&
                                                t.GetConstructor(Type.EmptyTypes) != null)
                                    .Select(t => (IStateMachineDefinitionProvider) Activator.CreateInstance(t)).ToArray());
        }

        private static bool IsMatch(PropertyInfo prop, string attributeName, string identifier)
        {
            var propertyType = prop.PropertyType;

            if (string.Compare(attributeName, identifier, false, CultureInfo.InvariantCulture) != 0)
            {
                return false;
            }
            else if (!typeof(IStateChartMetadata).IsAssignableFrom(propertyType) ||
                     prop.GetGetMethod() == null ||
                     (!prop.GetGetMethod().IsStatic && prop.ReflectedType.GetConstructor(Type.EmptyTypes) == null))
            {
                return false;
            }

            return true;
        }

        public IStateChartMetadata Resolve(string identifier)
        {
            identifier.CheckArgNull(nameof(identifier));

            return _funcs.GetOrAdd(identifier, _ =>
            {
                var targetProps = Assembly.Load(_config["DEFINITIONS_ASSEMBLY"])  // TODO: change this
                                          .GetTypes()
                                          .SelectMany(t => t.GetProperties())
                                          .Select(p => (prop: p, attr: p.GetCustomAttribute<StateMachineDefinitionAttribute>()))
                                          .Where(tuple => tuple.attr != null && IsMatch(tuple.prop, tuple.attr.Name, identifier))
                                          .ToArray();

                if (targetProps.Length == 1)
                {
                    var prop = targetProps[0].prop;

                    if (prop.GetGetMethod().IsStatic)
                    {
                        return (IStateChartMetadata)targetProps[0].prop.GetValue(null);
                    }
                    else
                    {
                        var instance = Activator.CreateInstance(prop.ReflectedType);

                        Debug.Assert(instance != null);

                        return (IStateChartMetadata)targetProps[0].prop.GetValue(instance);
                    }
                }
                else if (targetProps.Length > 1)
                {
                    throw new InvalidOperationException($"State machine identifier '{identifier}' is mapped to multiple possible definitions.");
                }
                else
                {
                    var candidates = _providers.Value.Select(p => p.GetStateMachine(identifier)).ToArray();

                    if (candidates.Length > 1)
                    {
                        throw new InvalidOperationException($"State machine identifier '{identifier}' is mapped to multiple possible definitions.");
                    }
                    else
                    {
                        return candidates.Length == 1 ? candidates[0] : null;
                    }
                }
            });
        }
    }
}
