﻿using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.ModelProvider.Xml.DataManipulation;
using StateChartsDotNet.CoreEngine.ModelProvider.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.States
{
    public class InvokeStateChart : IInvokeStateChart
    {
        private readonly XElement _element;

        public InvokeStateChart(XElement element)
        {
            _element = element;
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

        public IContentMetadata GetContent()
        {
            var node = _element.ScxmlElement("content");

            return node == null ? null : (IContentMetadata) new ContentMetadata(node);
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

        public IEnumerable<IParamMetadata> GetParams()
        {
            var nodes = _element.ScxmlElements("param");

            if (!this.Namelist.Any() && !nodes.Any())
            {
                throw new ModelValidationException("Service namelist or <params> must be specified.");
            }
            else if (this.Namelist.Any() && nodes.Any())
            {
                throw new ModelValidationException("Only one of service namelist and <params> can be specified.");
            }
            else if (this.Namelist.Any())
            {
                return this.Namelist.Select(n => new ParamMetadata(n)).Cast<IParamMetadata>();
            }
            else
            {
                return nodes.Select(n => new ParamMetadata(n)).Cast<IParamMetadata>();
            }
        }
    }
}