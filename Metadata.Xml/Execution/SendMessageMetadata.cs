using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.DataManipulation;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Metadata.Xml.DataManipulation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Execution
{
    public class SendMessageMetadata : ExecutableContentMetadata, ISendMessageMetadata
    {
        private readonly Lazy<Func<dynamic, string>> _getType;
        private readonly Lazy<Func<dynamic, string>> _getMessageName;
        private readonly Lazy<Func<dynamic, string>> _getTarget;
        private readonly Lazy<Func<dynamic, string>> _getDelay;
        private readonly Lazy<Func<dynamic, object>> _getContentValue;

        public SendMessageMetadata(XElement element)
            : base(element)
        {
            _getType = new Lazy<Func<dynamic, string>>(() =>
            {
                return ExpressionCompiler.Compile<string>(this.TypeExpression);
            });

            _getMessageName = new Lazy<Func<dynamic, string>>(() =>
            {
                return ExpressionCompiler.Compile<string>(this.MessageExpression);
            });

            _getTarget = new Lazy<Func<dynamic, string>>(() =>
            {
                return ExpressionCompiler.Compile<string>(this.TargetExpression);
            });

            _getDelay = new Lazy<Func<dynamic, string>>(() =>
            {
                return ExpressionCompiler.Compile<string>(this.DelayExpression);
            });

            _getContentValue = new Lazy<Func<dynamic, object>>(() =>
            {
                var node = _element.ScxmlElement("content");

                if (node == null)
                {
                    return _ => string.Empty;
                }

                var expression = node.Attribute("expr")?.Value;

                if (!string.IsNullOrWhiteSpace(expression))
                {
                    return ExpressionCompiler.Compile<object>(expression);
                }
                else
                {
                    return _ => node.Value ?? string.Empty;
                }
            });
        }

        public string Id => _element.Attribute("id")?.Value ?? string.Empty;

        public string IdLocation => _element.Attribute("idlocation")?.Value ?? string.Empty;

        private string Message => _element.Attribute("event")?.Value ?? string.Empty;

        private string MessageExpression => _element.Attribute("eventexpr")?.Value ?? string.Empty;

        private string Target => _element.Attribute("target")?.Value ?? string.Empty;

        private string TargetExpression => _element.Attribute("targetexpr")?.Value ?? string.Empty;

        private string Type => _element.Attribute("type")?.Value ?? string.Empty;

        private string TypeExpression => _element.Attribute("typeexpr")?.Value ?? string.Empty;

        private string Delay => _element.Attribute("delay")?.Value ?? string.Empty;

        private string DelayExpression => _element.Attribute("delayexpr")?.Value ?? string.Empty;

        private IEnumerable<string> Namelist
        {
            get => (_element.Attribute("eventexpr")?.Value ?? string.Empty).Split(" ");
        }

        public string GetType(dynamic data)
        {
            if (string.IsNullOrWhiteSpace(this.Type) && string.IsNullOrWhiteSpace(this.TypeExpression))
            {
                throw new ModelValidationException("Service type or typeExpression must be specified.");
            }
            else if (!string.IsNullOrWhiteSpace(this.Type) && !string.IsNullOrWhiteSpace(this.TypeExpression))
            {
                throw new ModelValidationException("Only one of service type and typeExpression can be specified.");
            }
            else if (!string.IsNullOrWhiteSpace(this.Type))
            {
                return this.Type;
            }
            else
            {
                return _getType.Value(data);
            }
        }

        public string GetMessageName(dynamic data)
        {
            if (string.IsNullOrWhiteSpace(this.Message) && string.IsNullOrWhiteSpace(this.MessageExpression))
            {
                throw new ModelValidationException("Service event or eventExpression must be specified.");
            }
            else if (!string.IsNullOrWhiteSpace(this.Message) && !string.IsNullOrWhiteSpace(this.MessageExpression))
            {
                throw new ModelValidationException("Only one of service event and eventExpression can be specified.");
            }
            else if (!string.IsNullOrWhiteSpace(this.Message))
            {
                return this.Message;
            }
            else
            {
                return _getMessageName.Value(data);
            }
        }

        public string GetTarget(dynamic data)
        {
            if (string.IsNullOrWhiteSpace(this.Target) && string.IsNullOrWhiteSpace(this.TargetExpression))
            {
                throw new ModelValidationException("Service target or targetExpression must be specified.");
            }
            else if (!string.IsNullOrWhiteSpace(this.Target) && !string.IsNullOrWhiteSpace(this.TargetExpression))
            {
                throw new ModelValidationException("Only one of service target and targetExpression can be specified.");
            }
            else if (!string.IsNullOrWhiteSpace(this.Target))
            {
                return this.Target;
            }
            else
            {
                return _getTarget.Value(data);
            }
        }

        public TimeSpan GetDelay(dynamic data)
        {
            if (string.IsNullOrWhiteSpace(this.Delay) && string.IsNullOrWhiteSpace(this.DelayExpression))
            {
                throw new ModelValidationException("Service delay or delayExpression must be specified.");
            }
            else if (!string.IsNullOrWhiteSpace(this.Delay) && !string.IsNullOrWhiteSpace(this.DelayExpression))
            {
                throw new ModelValidationException("Only one of service delay and delayExpression can be specified.");
            }
            else if (!string.IsNullOrWhiteSpace(this.Delay))
            {
                return TimeSpan.Parse(this.Delay);
            }
            else
            {
                return TimeSpan.Parse(_getDelay.Value(data));
            }
        }

        public object GetContent(dynamic data)
        {
            return _getContentValue.Value(data);
        }

        public IReadOnlyDictionary<string, Func<dynamic, object>> GetParams()
        {
            var nodes = _element.ScxmlElements("param");

            if (this.Namelist.Any() && nodes.Any())
            {
                throw new ModelValidationException("Only one of service namelist and <params> can be specified.");
            }

            if (this.Namelist.Any())
            {
                return new ReadOnlyDictionary<string, Func<dynamic, object>>(
                    this.Namelist.Select(n => new ParamMetadata(n)).ToDictionary(p => p.Name, p => (Func<dynamic, object>)p.GetValue));
            }
            else
            {
                return new ReadOnlyDictionary<string, Func<dynamic, object>>(
                    nodes.Select(n => new ParamMetadata(n)).ToDictionary(p => p.Name, p => (Func<dynamic, object>)p.GetValue));
            }
        }
    }
}
