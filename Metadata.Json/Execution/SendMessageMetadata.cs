using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Exceptions;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Metadata.Json.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace StateChartsDotNet.Metadata.Json.Execution
{
    public abstract class SendMessageMetadata : ExecutableContentMetadata, ISendMessageMetadata
    {
        private readonly Lazy<Func<dynamic, string>> _getType;
        private readonly Lazy<Func<dynamic, string>> _getMessageName;
        private readonly Lazy<Func<dynamic, string>> _getTarget;
        private readonly Lazy<Func<dynamic, string>> _getDelay;
        private readonly Lazy<Func<dynamic, object>> _getContentValue;

        internal SendMessageMetadata(JObject element)
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
                var node = this.Element.Property("content");

                if (node == null)
                {
                    return _ => string.Empty;
                }
                else if (node.Value is JObject)
                {
                    return _ => node.Value.ToString();
                }
                else
                {
                    var expression = node.Value.Value<string>();

                    return ExpressionCompiler.Compile<object>(expression);
                }
            });
        }

        protected abstract JObject Element { get; }

        public string Id => this.Element.Property("id")?.Value.Value<string>();

        public string IdLocation => this.Element.Property("idlocation")?.Value.Value<string>();

        private string Message => this.Element.Property("event")?.Value.Value<string>();

        private string MessageExpression => this.Element.Property("eventexpr")?.Value.Value<string>();

        private string Target => this.Element.Property("target")?.Value.Value<string>();

        private string TargetExpression => this.Element.Property("targetexpr")?.Value.Value<string>();

        private string Type => this.Element.Property("type")?.Value.Value<string>();

        private string TypeExpression => this.Element.Property("typeexpr")?.Value.Value<string>();

        private string Delay => this.Element.Property("delay")?.Value.Value<string>();

        private string DelayExpression => this.Element.Property("delayexpr")?.Value.Value<string>();

        private IEnumerable<string> Namelist
        {
            get
            {
                var namelist = this.Element.Property("namelist")?.Value.Value<string>();
                
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
                throw new InvalidOperationException("Service type or typeExpression must be specified.");
            }
            else if (this.Type != null && this.TypeExpression != null)
            {
                throw new InvalidOperationException("Only one of service type and typeExpression can be specified.");
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
                throw new InvalidOperationException("Only one of service event and eventExpression can be specified.");
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
                throw new InvalidOperationException("Only one of service target and targetExpression can be specified.");
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
                throw new InvalidOperationException("Only one of service delay and delayExpression can be specified.");
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
            var nodes = this.Element.Property("params")?.Value.Value<JArray>();

            if (this.Namelist.Any() && nodes.Any())
            {
                throw new InvalidOperationException("Only one of service namelist and <params> can be specified.");
            }

            IEnumerable<ParamMetadata> parms;

            if (this.Namelist.Any())
            {
                parms = this.Namelist.Select(n => new ParamMetadata(n));
            }
            else
            {
                parms = nodes.Cast<JObject>().Select(n => new ParamMetadata(n));
            }

            return new ReadOnlyDictionary<string, object>(parms.ToDictionary(p => p.Name, p => p.GetValue(data)));
        }
    }
}
