using Microsoft.CodeAnalysis.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Linq.Expressions;

namespace StateChartsDotNet.Common.ExpressionTrees
{
    internal class ExpressionTreeJsonSerializer : ExpressionVisitor
    {
        private readonly JsonWriter _writer;

        public ExpressionTreeJsonSerializer(JsonWriter writer)
        {
            _writer = writer;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var lambda = (LambdaExpression)node;

            _writer.WriteStartObject();

            _writer.WritePropertyName("parametercount");
            _writer.WriteValue(lambda.Parameters.Count);

            _writer.WritePropertyName("parameters");
            _writer.WriteStartArray();

            foreach (var parm in lambda.Parameters)
            {
                _writer.WriteStartObject();

                _writer.WritePropertyName("name");
                _writer.WriteValue(parm.Name);
                _writer.WritePropertyName("type");
                _writer.WriteValue(parm.Type.FullName);
                
                _writer.WriteEndObject();
            }

            _writer.WriteEndArray();

            _writer.WritePropertyName("body");
            Visit(lambda.Body);

            _writer.WriteEndObject();

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("name");
            _writer.WriteValue(node.Name);

            _writer.WriteEndObject();

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("type");
            _writer.WriteValue(node.Type.FullName);
            _writer.WritePropertyName("value");
            _writer.WriteValue(JsonConvert.SerializeObject(node.Value));

            _writer.WriteEndObject();

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("left");
            Visit(node.Left);
            _writer.WritePropertyName("right");
            Visit(node.Right);

            _writer.WriteEndObject();

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("type");
            _writer.WriteValue(node.Type.FullName);
            _writer.WritePropertyName("operand");
            Visit(node.Operand);

            _writer.WriteEndObject();

            return node;
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("type");
            _writer.WriteValue(node.Indexer.DeclaringType.FullName);
            _writer.WritePropertyName("indexer");
            _writer.WriteValue(node.Indexer.MetadataToken);
            _writer.WritePropertyName("argumentcount");
            _writer.WriteValue(node.Arguments.Count);

            _writer.WritePropertyName("arguments");
            _writer.WriteStartArray();

            foreach (var arg in node.Arguments)
            {
                Visit(arg);
            }

            _writer.WriteEndArray();

            _writer.WritePropertyName("instance");
            Visit(node.Object);

            _writer.WriteEndObject();

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("type");
            _writer.WriteValue(node.Member.DeclaringType.FullName);
            _writer.WritePropertyName("member");
            _writer.WriteValue(node.Member.MetadataToken);

            _writer.WritePropertyName("isInstance");
            _writer.WriteValue(node.Expression != null);

            if (node.Expression != null)
            {
                _writer.WritePropertyName("instance");
                Visit(node.Expression);
            }

            _writer.WriteEndObject();

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("type");
            _writer.WriteValue(node.Method.DeclaringType.FullName);
            _writer.WritePropertyName("method");
            _writer.WriteValue(node.Method.MetadataToken);

            _writer.WritePropertyName("argumentcount");
            _writer.WriteValue(node.Arguments.Count);

            _writer.WritePropertyName("arguments");
            _writer.WriteStartArray();

            foreach (var arg in node.Arguments)
            {
                Visit(arg);
            }

            _writer.WriteEndArray();

            _writer.WritePropertyName("isInstance");
            _writer.WriteValue(node.Object != null);

            if (node.Object != null)
            {
                _writer.WritePropertyName("instance");
                Visit(node.Object);
            }

            _writer.WriteEndObject();

            return node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());

            _writer.WritePropertyName("bindingscount");
            _writer.WriteValue(node.Bindings.Count);

            _writer.WritePropertyName("bindings");
            _writer.WriteStartArray();
            
            foreach (var binding in node.Bindings)
            {
                VisitMemberBinding(binding);
            }

            _writer.WriteEndArray();

            _writer.WritePropertyName("ctor");
            Visit(node.NewExpression);

            _writer.WriteEndObject();

            return node;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment binding)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("bindingtype");
            _writer.WriteValue(binding.BindingType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("type");
            _writer.WriteValue(binding.Member.DeclaringType.FullName);
            _writer.WritePropertyName("member");
            _writer.WriteValue(binding.Member.MetadataToken);

            _writer.WritePropertyName("value");
            Visit(binding.Expression);

            _writer.WriteEndObject();

            return binding;
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("bindingtype");
            _writer.WriteValue(binding.BindingType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("type");
            _writer.WriteValue(binding.Member.DeclaringType.FullName);
            _writer.WritePropertyName("member");
            _writer.WriteValue(binding.Member.MetadataToken);

            _writer.WritePropertyName("bindingscount");
            _writer.WriteValue(binding.Bindings.Count);

            _writer.WritePropertyName("bindings");
            _writer.WriteStartArray();

            foreach (var childBinding in binding.Bindings)
            {
                VisitMemberBinding(childBinding);
            }

            _writer.WriteEndArray();

            _writer.WriteEndObject();

            return binding;
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("bindingtype");
            _writer.WriteValue(binding.BindingType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("type");
            _writer.WriteValue(binding.Member.DeclaringType.FullName);
            _writer.WritePropertyName("member");
            _writer.WriteValue(binding.Member.MetadataToken);

            _writer.WritePropertyName("initializerscount");
            _writer.WriteValue(binding.Initializers.Count);

            _writer.WritePropertyName("initializers");
            _writer.WriteStartArray();

            foreach (var initializer in binding.Initializers)
            {
                VisitElementInit(initializer);
            }

            _writer.WriteEndArray();

            _writer.WriteEndObject();

            return binding;
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("type");
            _writer.WriteValue(node.AddMethod.DeclaringType.FullName);
            _writer.WritePropertyName("addmethod");
            _writer.WriteValue(node.AddMethod.MetadataToken);
            _writer.WritePropertyName("argumentscount");
            _writer.WriteValue(node.Arguments.Count);

            _writer.WritePropertyName("arguments");
            _writer.WriteStartArray();

            foreach (var arg in node.Arguments)
            {
                Visit(arg);
            }

            _writer.WriteEndArray();

            _writer.WriteEndObject();

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("type");
            _writer.WriteValue(node.Type.FullName);
            _writer.WritePropertyName("ctor");
            _writer.WriteValue(node.Constructor.MetadataToken);

            _writer.WritePropertyName("memberscount");
            _writer.WriteValue(node.Members.Count);

            _writer.WriteStartArray();

            foreach (var member in node.Members)
            {
                _writer.WriteValue(member.MetadataToken);
            }

            _writer.WriteEndArray();

            _writer.WritePropertyName("argumentscount");
            _writer.WriteValue(node.Arguments.Count);

            _writer.WriteStartArray();

            foreach (var arg in node.Arguments)
            {
                Visit(arg);
            }

            _writer.WriteEndArray();

            _writer.WriteEndObject();

            return node;
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());

            _writer.WritePropertyName("initializerscount");
            _writer.WriteValue(node.Initializers.Count);

            _writer.WritePropertyName("initializers");
            _writer.WriteStartArray();

            foreach (var initializer in node.Initializers)
            {
                VisitElementInit(initializer);
            }

            _writer.WriteEndArray();

            _writer.WritePropertyName("ctor");
            Visit(node.NewExpression);

            _writer.WriteEndObject();

            return node;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("type");
            _writer.WriteValue(node.Type.FullName);

            _writer.WritePropertyName("elementscount");
            _writer.WriteValue(node.Expressions.Count);

            _writer.WritePropertyName("elements");
            _writer.WriteStartArray();

            foreach (var element in node.Expressions)
            {
                Visit(element);
            }

            _writer.WriteEndArray();

            _writer.WriteEndObject();

            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            _writer.WriteStartObject();

            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("type");
            _writer.WriteValue(node.Type.FullName);

            _writer.WritePropertyName("test");
            Visit(node.Test);

            _writer.WritePropertyName("iftrue");
            Visit(node.IfTrue);

            _writer.WritePropertyName("iffalse");
            Visit(node.IfFalse);

            _writer.WriteEndObject();

            return node;
        }
    }
}
