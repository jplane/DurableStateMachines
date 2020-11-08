﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace CoreEngine.Model.Execution
{
    internal class Else
    {
        private readonly Lazy<List<ExecutableContent>> _content;

        public Else(XElement element)
        {
            element.CheckArgNull(nameof(element));

            _content = new Lazy<List<ExecutableContent>>(() =>
            {
                var content = new List<ExecutableContent>();

                foreach (var node in element.Elements())
                {
                    content.Add(ExecutableContent.Create(node));
                }

                return content;
            });
        }

        public async Task Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.LogInformation("Start: Else.Execute");

            try
            {
                foreach (var content in _content.Value)
                {
                    await content.Execute(context);
                }
            }
            finally
            {
                context.LogInformation("End: Else.Execute");
            }
        }
    }
}
