using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Exceptions;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Metadata.Xml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Execution
{
    public abstract class SendMessageMetadata : ExecutableContentMetadata, ISendMessageMetadata
    {
        private readonly Lazy<Func<dynamic, string>> _getType;
        private readonly Lazy<Func<dynamic, string>> _getMessageName;
        private readonly Lazy<Func<dynamic, string>> _getTarget;
        private readonly Lazy<Func<dynamic, string>> _getDelay;
        private readonly Lazy<Func<dynamic, object>> _getContentValue;

        internal SendMessageMetadata(XElement element)
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
                var node = this.Element.ScxmlElement("content");

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

        protected abstract XElement Element { get; }

        public string Id => this.Element.Attribute("id")?.Value;

        public string IdLocation => this.Element.Attribute("idlocation")?.Value;

        private string Message => this.Element.Attribute("event")?.Value;

        private string MessageExpression => this.Element.Attribute("eventexpr")?.Value;

        private string Target => this.Element.Attribute("target")?.Value;

        private string TargetExpression => this.Element.Attribute("targetexpr")?.Value;

        private string Type => this.Element.Attribute("type")?.Value;

        private string TypeExpression => this.Element.Attribute("typeexpr")?.Value;

        private string Delay => this.Element.Attribute("delay")?.Value;

        private string DelayExpression => this.Element.Attribute("delayexpr")?.Value;

        private IEnumerable<string> Namelist
        {
            get
            {
                var namelist = this.Element.Attribute("namelist")?.Value;
                
                if (namelist != null)
                {
                    return namelist.Split(" ");
                }
                else
                {
                    return Enumerable.Empty<string>();
                }
            }
        }

        public string GetType(dynamic data)
        {
            if (this.Type == null && this.TypeExpression == null)
            {
                throw new MetadataValidationException("Service type or typeExpression must be specified.");
            }
            else if (this.Type != null && this.TypeExpression != null)
            {
                throw new MetadataValidationException("Only one of service type and typeExpression can be specified.");
            }
            else if (this.Type != null)
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
            if (this.Message != null && this.MessageExpression != null)
            {
                throw new MetadataValidationException("Only one of service event and eventExpression can be specified.");
            }
            else if (this.Message != null)
            {
                return this.Message;
            }
            else if (this.MessageExpression != null)
            {
                return _getMessageName.Value(data);
            }
            else
            {
                return null;
            }
        }

        public string GetTarget(dynamic data)
        {
            if (this.Target != null && this.TargetExpression != null)
            {
                throw new MetadataValidationException("Only one of service target and targetExpression can be specified.");
            }
            else if (this.Target != null)
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
            if (this.Delay != null && this.DelayExpression != null)
            {
                throw new MetadataValidationException("Only one of service delay and delayExpression can be specified.");
            }
            else if (!string.IsNullOrWhiteSpace(this.Delay))
            {
                return TimeSpan.Parse(this.Delay);
            }
            else if (this.DelayExpression != null)
            {
                return TimeSpan.Parse(_getDelay.Value(data));
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        public object GetContent(dynamic data)
        {
            return _getContentValue.Value(data);
        }

        public IReadOnlyDictionary<string, object> GetParams(dynamic data)
        {
            var nodes = this.Element.ScxmlElements("param");

            if (this.Namelist.Any() && nodes.Any())
            {
                throw new MetadataValidationException("Only one of service namelist and <params> can be specified.");
            }

            IEnumerable<ParamMetadata> parms;

            if (this.Namelist.Any())
            {
                parms = this.Namelist.Select(n => new ParamMetadata(n));
            }
            else
            {
                parms = nodes.Select(n => new ParamMetadata(n));
            }

            return new ReadOnlyDictionary<string, object>(parms.ToDictionary(p => p.Name, p => p.GetValue(data)));
        }
    }
}
