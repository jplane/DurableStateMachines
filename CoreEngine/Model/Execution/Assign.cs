﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.Model.Execution
{
    internal class Assign : ExecutableContent
    {
        private readonly string _location;
        private readonly string _expression;
        private readonly string _body;

        public Assign(XElement element)
            : base(element)
        {
            element.CheckArgNull(nameof(element));

            _location = element.Attribute("location").Value;
            _expression = element.Attribute("expr")?.Value ?? string.Empty;
            _body = element.Value ?? string.Empty;
        }

        protected override async Task _Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            if (!string.IsNullOrWhiteSpace(_expression))
            {
                var value = await context.Eval<object>(_expression);

                context.SetDataValue(_location, value);

                context.LogDebug($"Set {_location} = {value}");
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
