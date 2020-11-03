using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.Model
{
    internal class Invoke
    {
        private readonly XElement _element;
        private readonly _State _state;
        private readonly bool _autoforward;

        public Invoke(XElement element, _State state)
        {
            _element = element;
            _state = state;

            var afattr = element.Attribute("autoforward");
            
            if (afattr != null && bool.TryParse(afattr.Value, out bool result))
            {
                _autoforward = result;
            }
            else
            {
                _autoforward = false;
            }
        }

        private string GetId(ExecutionContext context)
        {
            var idattr = _element.Attribute("id");

            if (idattr != null)
            {
                return idattr.Value;
            }
            else
            {
                var idlocationattr = _element.Attribute("idlocation");

                if (idlocationattr == null || !context.ExecutionState.TryGetValue(idlocationattr.Value, out object value))
                {
                    throw new InvalidOperationException("Unable to resolve invoke ID.");
                }
                else
                {
                    return (string) value;
                }
            }
        }

        public void ProcessExternalEvent(ExecutionContext context, Event externalEvent)
        {
            var id = GetId(context);

            if (id == externalEvent.InvokeId)
            {
                ApplyFinalize(externalEvent);
            }

            if (_autoforward)
            {
                context.EventPublisher.Send(id, externalEvent);
            }
        }

        private void ApplyFinalize(Event externalEvent)
        {
        }
    }
}
