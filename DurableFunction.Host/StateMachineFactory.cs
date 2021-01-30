using Microsoft.Extensions.Configuration;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace StateChartsDotNet.DurableFunction.Host
{
    public interface IStateMachineFactory
    {
        bool TryResolveIdentifier(string identifier, out IStateChartMetadata definition);
    }

    public abstract class StateMachineFactory : IStateMachineFactory
    {
        protected readonly IConfiguration _config;

        public StateMachineFactory(IConfiguration config)
        {
            _config = config;
        }

        public IConfiguration Configuration => _config;

        public virtual bool TryResolveIdentifier(string identifier, out IStateChartMetadata definition)
        {
            definition = null;
            return false;
        }

        internal bool InnerTryResolveIdentifier(string identifier, out IStateChartMetadata definition)
        {
            identifier.CheckArgNull(nameof(identifier));

            bool IsMatch((PropertyInfo prop, StateMachineDefinitionAttribute attr) item)
            {
                var propertyType = item.prop.PropertyType;

                if (!typeof(IStateChartMetadata).IsAssignableFrom(propertyType))
                {
                    return false;
                }

                return string.Compare(item.attr.Name, identifier, false, CultureInfo.InvariantCulture) == 0;
            }

            var targetProps = this.GetType().GetProperties()
                                            .Select(p => (prop: p, attr: p.GetCustomAttribute<StateMachineDefinitionAttribute>()))
                                            .Where(IsMatch)
                                            .ToArray();

            if (targetProps.Length == 1)
            {
                definition = (IStateChartMetadata) targetProps[0].prop.GetValue(this);
                return true;
            }
            else if (targetProps.Length > 1)
            {
                throw new InvalidOperationException($"State machine identifier '{identifier}' is mapped to multiple possible definitions.");
            }
            else
            {
                return TryResolveIdentifier(identifier, out definition);
            }
        }
    }
}
