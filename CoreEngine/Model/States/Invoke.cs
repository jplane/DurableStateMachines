using CoreEngine.Model.DataManipulation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace CoreEngine.Model.States
{
    internal class Invoke
    {
        private readonly bool _autoforward;
        private readonly string _type;
        private readonly string _typeExpr;
        private readonly string _id;
        private readonly string _idLocation;
        private readonly string _source;
        private readonly string _sourceExpr;
        private readonly string _namelist;
        private readonly Lazy<Content> _content;
        private readonly Lazy<Finalize> _finalize;
        private readonly Lazy<List<Param>> _params;

        public Invoke(XElement element)
        {
            element.CheckArgNull(nameof(element));

            _type = element.Attribute("type")?.Value ?? string.Empty;
            _typeExpr = element.Attribute("typeexpr")?.Value ?? string.Empty;

            _id = element.Attribute("id")?.Value ?? string.Empty;
            _idLocation = element.Attribute("idlocation")?.Value ?? string.Empty;

            _source = element.Attribute("src")?.Value ?? string.Empty;
            _sourceExpr = element.Attribute("srcexpr")?.Value ?? string.Empty;

            _namelist = element.Attribute("namelist")?.Value ?? string.Empty;

            var afattr = element.Attribute("autoforward");
            
            if (afattr != null && bool.TryParse(afattr.Value, out bool result))
            {
                _autoforward = result;
            }
            else
            {
                _autoforward = false;
            }

            _content = new Lazy<Content>(() =>
            {
                var node = element.ScxmlElement("content");

                return node == null ? null : new Content(node);
            });

            _finalize = new Lazy<Finalize>(() =>
            {
                var node = element.ScxmlElement("finalize");

                return node == null ? null : new Finalize(node);
            });

            _params = new Lazy<List<Param>>(() =>
            {
                var nodes = element.ScxmlElements("param");

                return new List<Param>(nodes.Select(n => new Param(n)));
            });
        }

        private string GetId(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            if (! string.IsNullOrWhiteSpace(_id))
            {
                return _id;
            }
            else if (string.IsNullOrWhiteSpace(_idLocation) || !context.TryGet(_idLocation, out object value))
            {
                throw new InvalidOperationException("Unable to resolve invoke ID.");
            }
            else
            {
                return (string) value;
            }
        }

        public Task Execute(ExecutionContext context)
        {
            return Task.CompletedTask;
        }

        public void Cancel(ExecutionContext context)
        {
        }

        public void ProcessExternalEvent(ExecutionContext context, Event externalEvent)
        {
            externalEvent.CheckArgNull(nameof(externalEvent));

            var id = GetId(context);

            if (id == externalEvent.InvokeId)
            {
                ApplyFinalize(externalEvent);
            }

            if (_autoforward)
            {
                // send events to service
            }
        }

        private void ApplyFinalize(Event externalEvent)
        {
        }
    }
}
