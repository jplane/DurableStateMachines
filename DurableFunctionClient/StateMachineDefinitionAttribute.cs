using System;
using System.Collections.Generic;
using System.Text;

namespace StateChartsDotNet.DurableFunctionClient
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class StateMachineDefinitionAttribute : Attribute
    {
        public StateMachineDefinitionAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
    }
}
