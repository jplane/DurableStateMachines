using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.Model.DataManipulation
{
    internal class Param
    {
        private readonly string _name;
        private readonly string _location;
        private readonly string _expression;

        public Param(XElement element)
        {
            element.CheckArgNull(nameof(element));

            _name = element.Attribute("name").Value;
            _location = element.Attribute("location")?.Value ?? string.Empty;
            _expression = element.Attribute("expr")?.Value ?? string.Empty;
        }
    }
}
