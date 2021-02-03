using DSM.Common;
using DSM.Common.Model.States;
using DSM.FunctionClient;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace DSM.FunctionHost
{
    public interface IStateMachineDefinitionProvider
    {
        IStateMachineMetadata GetStateMachine(string identifier);
    }

    internal class DefinitionResolver
    {
        private readonly Lazy<IStateMachineDefinitionProvider[]> _providers;
        private readonly ConcurrentDictionary<string, IStateMachineMetadata> _funcs;

        public DefinitionResolver()
        {
            _funcs = new ConcurrentDictionary<string, IStateMachineMetadata>();

            _providers = new Lazy<IStateMachineDefinitionProvider[]>(() =>
                Assemblies.SelectMany(assembly => assembly.GetTypes())
                            .Where(t => typeof(IStateMachineDefinitionProvider).IsAssignableFrom(t) &&
                                        t.GetConstructor(Type.EmptyTypes) != null)
                            .Select(t => (IStateMachineDefinitionProvider)Activator.CreateInstance(t)).ToArray());
        }

        public static Assembly[] Assemblies { get; set; } = { };

        private static bool IsMatch(PropertyInfo prop, string attributeName, string identifier)
        {
            var propertyType = prop.PropertyType;

            if (string.Compare(attributeName, identifier, false, CultureInfo.InvariantCulture) != 0)
            {
                return false;
            }
            else if (!typeof(IStateMachineMetadata).IsAssignableFrom(propertyType) ||
                     prop.GetGetMethod() == null ||
                     (!prop.GetGetMethod().IsStatic && prop.ReflectedType.GetConstructor(Type.EmptyTypes) == null))
            {
                return false;
            }

            return true;
        }

        public IStateMachineMetadata Resolve(string identifier)
        {
            identifier.CheckArgNull(nameof(identifier));

            return _funcs.GetOrAdd(identifier, _ =>
            {
                var targetProps = Assemblies.SelectMany(assembly => assembly.GetTypes())
                                            .SelectMany(t => t.GetProperties())
                                            .Select(p => (prop: p, attr: p.GetCustomAttribute<StateMachineDefinitionAttribute>()))
                                            .Where(tuple => tuple.attr != null && IsMatch(tuple.prop, tuple.attr.Name, identifier))
                                            .ToArray();

                if (targetProps.Length == 1)
                {
                    var prop = targetProps[0].prop;

                    if (prop.GetGetMethod().IsStatic)
                    {
                        return (IStateMachineMetadata)targetProps[0].prop.GetValue(null);
                    }
                    else
                    {
                        var instance = Activator.CreateInstance(prop.ReflectedType);

                        Debug.Assert(instance != null);

                        return (IStateMachineMetadata)targetProps[0].prop.GetValue(instance);
                    }
                }
                else if (targetProps.Length > 1)
                {
                    throw new InvalidOperationException($"State machine identifier '{identifier}' is mapped to multiple possible definitions.");
                }
                else
                {
                    var candidates = _providers.Value
                                               .Select(p => p.GetStateMachine(identifier))
                                               .Where(definition => definition != null)
                                               .ToArray();

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
