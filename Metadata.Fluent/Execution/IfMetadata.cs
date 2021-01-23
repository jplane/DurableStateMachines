using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class IfMetadata<TParent> : ExecutableContentMetadata, IIfMetadata where TParent : IModelMetadata
    {
        private readonly List<ExecutableContentMetadata> _executableContent;
        private readonly List<ElseIfMetadata<IfMetadata<TParent>>> _elseIfs;
        private readonly Lazy<Func<IDictionary<string, object>, bool>> _evalResolver;

        private ElseMetadata<IfMetadata<TParent>> _else;
        private Expression<Func<IDictionary<string, object>, bool>> _eval;

        internal IfMetadata()
        {
            _executableContent = new List<ExecutableContentMetadata>();
            _elseIfs = new List<ElseIfMetadata<IfMetadata<TParent>>>();
            _evalResolver = new Lazy<Func<IDictionary<string, object>, bool>>(() =>
                _eval == null ? _ => false : _eval.Compile());
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.Write(_eval);
            writer.WriteMany(_executableContent, (o, w) => o.Serialize(w));
            writer.WriteMany(_elseIfs, (o, w) => o.Serialize(w));
            writer.Write(_else, (o, w) => o.Serialize(w));
        }

        internal static IfMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new IfMetadata<TParent>();

            metadata.MetadataId = reader.ReadNullableString();
            metadata._eval = reader.Read<Expression<Func<IDictionary<string, object>, bool>>>();

            metadata._executableContent.AddRange(reader.ReadMany(ExecutableContentMetadata._Deserialize,
                                                    o => ((dynamic)o).Parent = metadata));

            metadata._elseIfs.AddRange(reader.ReadMany(ElseIfMetadata<IfMetadata<TParent>>.Deserialize,
                                                       o => o.Parent = metadata));

            metadata._else = reader.Read(ElseMetadata<IfMetadata<TParent>>.Deserialize,
                                         o => o.Parent = metadata);

            return metadata;
        }

        internal TParent Parent { get; set; }

        public TParent _ => this.Parent;

        public IfMetadata<TParent> Condition(Expression<Func<IDictionary<string, object>, bool>> condition)
        {
            _eval = condition;
            return this;
        }

        public ElseIfMetadata<IfMetadata<TParent>> ElseIf
        {
            get
            {
                var elseif = new ElseIfMetadata<IfMetadata<TParent>>();

                _elseIfs.Add(elseif);

                elseif.Parent = this;

                elseif.MetadataId = $"{((IModelMetadata)this).MetadataId}.ElseIf[{_elseIfs.Count}]";

                return elseif;
            }
        }

        public ElseMetadata<IfMetadata<TParent>> Else
        {
            get
            {
                _else = new ElseMetadata<IfMetadata<TParent>>();

                _else.Parent = this;

                _else.MetadataId = $"{((IModelMetadata)this).MetadataId}.Else";

                return _else;
            }
        }

        public IfMetadata<TParent> Assign(string location, object value)
        {
            var ec = new AssignMetadata<IfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            ec.Location(location).Value(value);

            return this;
        }

        public IfMetadata<TParent> Assign(string location, Expression<Func<IDictionary<string, object>, object>> getter)
        {
            var ec = new AssignMetadata<IfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            ec.Location(location).Value(getter);

            return this;
        }

        public CancelMetadata<IfMetadata<TParent>> Cancel
        {
            get
            {
                var ec = new CancelMetadata<IfMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public ForeachMetadata<IfMetadata<TParent>> Foreach
        {
            get
            {
                var ec = new ForeachMetadata<IfMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public IfMetadata<IfMetadata<TParent>> If
        {
            get
            {
                var ec = new IfMetadata<IfMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public IfMetadata<TParent> Log(string message)
        {
            var ec = new LogMetadata<IfMetadata<TParent>>();

            ec.Message(message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public IfMetadata<TParent> Log(Expression<Func<IDictionary<string, object>, string>> getter)
        {
            var ec = new LogMetadata<IfMetadata<TParent>>();

            ec.Message(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public IfMetadata<TParent> Raise(string messageName)
        {
            var ec = new RaiseMetadata<IfMetadata<TParent>>();

            ec.MessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public IfMetadata<TParent> Execute(Expression<Action<IDictionary<string, object>>> action)
        {
            var ec = new ScriptMetadata<IfMetadata<TParent>>();

            ec.Action(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        internal SendMessageMetadata<IfMetadata<TParent>> SendMessage
        {
            get
            {
                var ec = new SendMessageMetadata<IfMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        internal QueryMetadata<IfMetadata<TParent>> Query
        {
            get
            {
                var ec = new QueryMetadata<IfMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        bool IIfMetadata.EvalIfCondition(dynamic data) => _evalResolver.Value(data);

        IEnumerable<IExecutableContentMetadata> IIfMetadata.GetElseExecutableContent() => _else.GetExecutableContent();

        IEnumerable<Func<dynamic, bool>> IIfMetadata.GetElseIfConditions() => _elseIfs.Select(ei => ei.GetEvalCondition());

        IEnumerable<IEnumerable<IExecutableContentMetadata>> IIfMetadata.GetElseIfExecutableContent() => _elseIfs.Select(ei => ei.GetExecutableContent());

        IEnumerable<IExecutableContentMetadata> IIfMetadata.GetExecutableContent() => _executableContent;
    }
}
