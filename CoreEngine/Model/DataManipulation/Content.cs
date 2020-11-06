using System;
using System.Xml.Linq;

namespace CoreEngine.Model.DataManipulation
{
    internal class Content
    {
        private readonly string _expression;
        private readonly string _body;

        public Content(XElement element)
        {
            element.CheckArgNull(nameof(element));

            _expression = element.Attribute("expr")?.Value ?? string.Empty;
            _body = element.Value ?? string.Empty;
        }
    }
}
