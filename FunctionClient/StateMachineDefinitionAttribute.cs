using System;
using System.Collections.Generic;
using System.Text;
using DSM.Metadata.States;

namespace DSM.FunctionClient
{
    /// <summary>
    /// Used to identify properties on public types that expose state machine definitions. Such properties should return instances of <see cref="StateMachine{TData}"/>
    /// </summary>
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
