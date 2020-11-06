using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using CoreEngine.Model.DataManipulation;
using System.Threading.Tasks;

namespace CoreEngine.Model.Execution
{
    internal class Send : ExecutableContent
    {
        private readonly string _event;
        private readonly string _eventExpr;
        private readonly string _target;
        private readonly string _targetExpr;
        private readonly string _type;
        private readonly string _typeExpr;
        private readonly string _id;
        private readonly string _idLocation;
        private readonly string _delay;
        private readonly string _delayExpr;
        private readonly string _namelist;
        private readonly Lazy<Content> _content;
        private readonly Lazy<List<Param>> _params;

        public Send(XElement element)
            : base(element)
        {
            element.CheckArgNull(nameof(element));

            _event = element.Attribute("event")?.Value ?? string.Empty;
            _eventExpr = element.Attribute("eventexpr")?.Value ?? string.Empty;

            _target = element.Attribute("target")?.Value ?? string.Empty;
            _targetExpr = element.Attribute("targetexpr")?.Value ?? string.Empty;

            _type = element.Attribute("type")?.Value ?? string.Empty;
            _typeExpr = element.Attribute("typeexpr")?.Value ?? string.Empty;

            _id = element.Attribute("id")?.Value ?? string.Empty;
            _idLocation = element.Attribute("idlocation")?.Value ?? string.Empty;

            _delay = element.Attribute("delay")?.Value ?? string.Empty;
            _delayExpr = element.Attribute("delayexpr")?.Value ?? string.Empty;

            _namelist = element.Attribute("namelist")?.Value ?? string.Empty;

            _content = new Lazy<Content>(() =>
            {
                var node = element.ScxmlElement("content");

                return node == null ? null : new Content(node);
            });

            _params = new Lazy<List<Param>>(() =>
            {
                var nodes = element.ScxmlElements("param");

                return new List<Param>(nodes.Select(n => new Param(n)));
            });
        }

        public override Task Execute(ExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
