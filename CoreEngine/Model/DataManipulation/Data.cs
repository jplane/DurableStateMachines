using CoreEngine.Model.States;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.Model.DataManipulation
{
    internal class Data
    {
        private readonly string _id;
        private readonly string _source;
        private readonly string _expression;
        private readonly string _body;

        public Data(XElement element)
        {
            element.CheckArgNull(nameof(element));

            _id = element.Attribute("id").Value;
            _source = element.Attribute("src")?.Value ?? string.Empty;
            _expression = element.Attribute("expr")?.Value ?? string.Empty;
            _body = element.Value ?? string.Empty;
        }

        public async Task Init(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            try
            {
                if (!string.IsNullOrWhiteSpace(_expression))
                {
                    context.SetDataValue(_id, await context.Eval<object>(_expression));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                context.EnqueueExecutionError(ex);
            }
        }
    }
}
