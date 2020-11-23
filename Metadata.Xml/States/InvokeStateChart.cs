using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.DataManipulation;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Xml.DataManipulation;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.States
{
    public class InvokeStateChart : IInvokeStateChartMetadata
    {
        private readonly XElement _element;
        private readonly Lazy<string> _uniqueId;
        private readonly Lazy<Func<dynamic, object>> _getContentValue;
        private readonly Lazy<Func<dynamic, string>> _getTypeValue;

        public InvokeStateChart(XElement element)
        {
            _element = element;

            _uniqueId = new Lazy<string>(() =>
            {
                return element.GetUniqueElementPath();
            });

            _getContentValue = new Lazy<Func<dynamic, object>>(() =>
            {
                var node = _element.ScxmlElement("content");

                if (node == null)
                {
                    var src = _element.Attribute("src")?.Value ?? string.Empty;
                    
                    if (!string.IsNullOrEmpty(src))
                    {
                        return _ => src;
                    }
                    else
                    {
                        var srcExpression = _element.Attribute("srcexpr")?.Value ?? string.Empty;

                        if (!string.IsNullOrWhiteSpace(srcExpression))
                        {
                            return ExpressionCompiler.Compile<object>(srcExpression);
                        }
                        else
                        {
                            return _ => string.Empty;
                        }
                    }
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

            _getTypeValue = new Lazy<Func<dynamic, string>>(() =>
            {
                var type = _element.Attribute("type")?.Value ?? string.Empty;

                if (!string.IsNullOrEmpty(type))
                {
                    return _ => type;
                }
                else
                {
                    var typeExpression = _element.Attribute("typeexpr")?.Value ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(typeExpression))
                    {
                        return ExpressionCompiler.Compile<string>(typeExpression);
                    }
                    else
                    {
                        return _ => string.Empty;
                    }
                }
            });
        }

        public bool Autoforward
        {
            get
            {
                var afattr = _element.Attribute("autoforward");

                if (afattr != null && bool.TryParse(afattr.Value, out bool result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
        }

        public string UniqueId => _uniqueId.Value;

        public virtual bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            return true;
        }

        public string Id => _element.Attribute("id")?.Value ?? string.Empty;

        public string IdLocation => _element.Attribute("idlocation")?.Value ?? string.Empty;

        private IEnumerable<string> Namelist
        {
            get
            {
                var names = _element?.Attribute("namelist")?.Value;

                if (string.IsNullOrWhiteSpace(names))
                {
                    return Enumerable.Empty<string>();
                }
                else
                {
                    return names.Split(" ");
                }
            }
        }

        public IEnumerable<IExecutableContentMetadata> GetFinalizeExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            foreach (var node in _element.Elements())
            {
                content.Add(ExecutableContentMetadata.Create(node));
            }

            return content.AsEnumerable();
        }

        public string GetType(dynamic data)
        {
            return _getTypeValue.Value(data);
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
