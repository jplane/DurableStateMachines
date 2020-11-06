﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace CoreEngine.Model.Execution
{
    internal class ElseIf
    {
        private readonly string _cond;
        private readonly Lazy<List<ExecutableContent>> _content;

        public ElseIf(XElement element)
        {
            element.CheckArgNull(nameof(element));

            _cond = element.Attribute("cond").Value;

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

        public bool ConditionalExecute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var result = context.Eval<bool>(_cond);

            if (result)
            {
                foreach (var content in _content.Value)
                {
                    content.Execute(context);
                }
            }

            return result;
        }
    }
}
