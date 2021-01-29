using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.ExpressionTrees;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace StateChartsDotNet.Metadata.Execution
{
    public class If<TData> : ExecutableContent<TData>, IIfMetadata
    {
        private readonly Lazy<Func<dynamic, bool>> _condition;

        private MetadataList<ExecutableContent<TData>> _actions;
        private MetadataList<ElseIf<TData>> _elseIfs;
        private Else<TData> _else;

        public If()
        {
            this.Actions = new MetadataList<ExecutableContent<TData>>();
            this.ElseIfs = new MetadataList<ElseIf<TData>>();

            _condition = new Lazy<Func<dynamic, bool>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.ConditionExpression))
                {
                    return ExpressionCompiler.Compile<bool>(this.ConditionExpression);
                }
                else if (this.ConditionFunction != null)
                {
                    return data => this.ConditionFunction(data);
                }
                else
                {
                    return _ => throw new InvalidOperationException("Unable to resolve 'if' condition.");
                }
            });
        }

        public Func<TData, bool> ConditionFunction { get; set; }

        [JsonProperty("conditionexpression")]
        private string ConditionExpression { get; set; }

        [JsonProperty("actions")]
        public MetadataList<ExecutableContent<TData>> Actions
        {
            get => _actions;

            private set
            {
                if (value == null)
                {
                    throw new InvalidOperationException();
                }

                if (_actions != null)
                {
                    _actions.MetadataIdResolver = null;
                }

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "if"}.actions";

                _actions = value;
            }
        }

        [JsonProperty("elseifs")]
        public MetadataList<ElseIf<TData>> ElseIfs
        {
            get => _elseIfs;

            private set
            {
                if (value == null)
                {
                    throw new InvalidOperationException();
                }

                if (_elseIfs != null)
                {
                    _elseIfs.MetadataIdResolver = null;
                }

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "if"}.elseifs";

                _elseIfs = value;
            }
        }

        [JsonProperty("else")]
        public Else<TData> Else
        {
            get => _else;

            set
            {
                if (_else != null)
                {
                    _else.MetadataIdResolver = null;
                }

                if (value != null)
                {
                    value.MetadataIdResolver = _ => $"{this.MetadataIdResolver?.Invoke(this) ?? "if"}.else";
                }

                _else = value;
            }
        }

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.ConditionExpression) && this.ConditionFunction == null)
            {
                errors.Add("One of ConditionExpression or ConditionFunction must be set.");
            }

            foreach (var action in this.Actions)
            {
                action.Validate(errorMap);
            }

            foreach (var elseif in this.ElseIfs)
            {
                elseif.Validate(errorMap);
            }

            this.Else?.Validate(errorMap);

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        bool IIfMetadata.EvalCondition(dynamic data) => _condition.Value(data);

        IEnumerable<IElseIfMetadata> IIfMetadata.GetElseIfs() => this.ElseIfs ?? Enumerable.Empty<IElseIfMetadata>();

        IEnumerable<IExecutableContentMetadata> IIfMetadata.GetExecutableContent() =>
            this.Actions ?? Enumerable.Empty<IExecutableContentMetadata>();

        IElseMetadata IIfMetadata.GetElse() => this.Else;
    }
}
