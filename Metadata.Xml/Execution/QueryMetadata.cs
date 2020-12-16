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
    public abstract class QueryMetadata : ExecutableContentMetadata, IQueryMetadata
    {
        private readonly Lazy<Func<dynamic, string>> _getType;
        private readonly Lazy<Func<dynamic, string>> _getTarget;

        internal QueryMetadata(XElement element)
            : base(element)
        {
            _getType = new Lazy<Func<dynamic, string>>(() =>
            {
                return ExpressionCompiler.Compile<string>(this.TypeExpression);
            });

            _getTarget = new Lazy<Func<dynamic, string>>(() =>
            {
                return ExpressionCompiler.Compile<string>(this.TargetExpression);
            });
        }

        protected abstract XElement Element { get; }

        public string ResultLocation => this.Element.Attribute("resultlocation")?.Value;

        private string Target => this.Element.Attribute("target")?.Value;

        private string TargetExpression => this.Element.Attribute("targetexpr")?.Value;

        private string Type => this.Element.Attribute("type")?.Value;

        private string TypeExpression => this.Element.Attribute("typeexpr")?.Value;

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

        public string GetTarget(dynamic data)
        {
            if (this.Target == null && this.TargetExpression == null)
            {
                throw new MetadataValidationException("Service target or targetExpression must be specified.");
            }
            else if (this.Target != null && this.TargetExpression != null)
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

        public IEnumerable<IExecutableContentMetadata> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            var finalizeElements = this.Element.Element("finalize")?.Elements() ?? Enumerable.Empty<XElement>();

            foreach (var node in finalizeElements)
            {
                content.Add(ExecutableContentMetadata.Create(node));
            }

            return content.AsEnumerable();
        }
    }
}
