using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Xml.Data;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.States
{
    public class InvokeStateChartMetadata : IInvokeStateChartMetadata
    {
        private readonly XElement _element;
        private readonly Lazy<string> _uniqueId;
        private readonly Lazy<Func<dynamic, string>> _getRootId;
        private readonly Lazy<Func<dynamic, IRootStateMetadata>> _getRoot;

        internal InvokeStateChartMetadata(XElement element)
        {
            _element = element;

            _uniqueId = new Lazy<string>(() =>
            {
                return element.GetUniqueElementPath();
            });

            _getRootId = new Lazy<Func<dynamic, string>>(() =>
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
                            return ExpressionCompiler.Compile<string>(srcExpression);
                        }
                        else
                        {
                            return _ => string.Empty;
                        }
                    }
                }
                else
                {
                    return _ => string.Empty;
                }
            });

            _getRoot = new Lazy<Func<dynamic, IRootStateMetadata>>(() =>
            {
                var node = _element.ScxmlElement("content");

                if (node != null)
                {
                    StateChart FromSource(string source)
                    {
                        source.CheckArgNull(nameof(source));

                        var xdoc = XDocument.Parse(source);

                        Debug.Assert(xdoc != null);

                        return new StateChart(xdoc);
                    }

                    var expression = node.Attribute("expr")?.Value;

                    if (!string.IsNullOrWhiteSpace(expression))
                    {
                        return data =>
                        {
                            var getSourceFunc = ExpressionCompiler.Compile<string>(expression);

                            Debug.Assert(getSourceFunc != null);

                            var source = getSourceFunc(data);

                            if (string.IsNullOrWhiteSpace(source))
                            {
                                throw new InvalidOperationException("Unable to resolve source for child statechart.");
                            }

                            return FromSource(source);
                        };
                    }
                    else if (node.ScxmlElement("scxml") != null)
                    {
                        return _ => FromSource(node.ScxmlElement("scxml").ToString());
                    }
                }

                return _ => null;
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

            var elements = _element.ScxmlElement("finalize")?.Elements() ?? Enumerable.Empty<XElement>();

            foreach (var node in elements)
            {
                content.Add(ExecutableContentMetadata.Create(node));
            }

            return content.AsEnumerable();
        }

        public string GetRootId(dynamic data)
        {
            return _getRootId.Value(data);
        }

        public IRootStateMetadata GetRoot(dynamic data)
        {
            return _getRoot.Value(data);
        }

        public IReadOnlyDictionary<string, object> GetParams(dynamic data)
        {
            var nodes = _element.ScxmlElements("param");

            if (this.Namelist.Any() && nodes.Any())
            {
                throw new ModelValidationException("Only one of service namelist and <params> can be specified.");
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
