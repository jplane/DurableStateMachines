using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace StateChartsDotNet.Common
{
    internal class ExpressionTreeJsonSerializer : ExpressionVisitor
    {
        private readonly JsonWriter _writer;

        public ExpressionTreeJsonSerializer(JsonWriter writer)
        {
            _writer = writer;
            _writer.Formatting = Formatting.Indented;
        }

        public override Expression Visit(Expression node)
        {
            _writer.WriteStartObject();
            base.Visit(node);
            _writer.WriteEndObject();
            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var lambda = (LambdaExpression)node;

            _writer.WritePropertyName("parametercount");
            _writer.WriteValue(lambda.Parameters.Count);

            _writer.WritePropertyName("parameters");
            _writer.WriteStartArray();

            foreach (var parm in lambda.Parameters)
            {
                Visit(parm);
            }

            _writer.WriteEndArray();

            _writer.WritePropertyName("body");
            Visit(lambda.Body);

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("name");
            _writer.WriteValue(node.Name);
            _writer.WritePropertyName("type");
            _writer.WriteValue(node.Type.AssemblyQualifiedName);
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("type");
            _writer.WriteValue(node.Type.AssemblyQualifiedName);
            _writer.WritePropertyName("value");
            _writer.WriteValue(JsonConvert.SerializeObject(node.Value));
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("left");
            Visit(node.Left);
            _writer.WritePropertyName("right");
            Visit(node.Right);
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("type");
            _writer.WriteValue(node.Type.AssemblyQualifiedName);
            _writer.WritePropertyName("operand");
            Visit(node.Operand);
            return node;
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("indexer");
            _writer.WriteValue(node.Indexer.Name);

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

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("member");
            _writer.WriteValue(node.Member.Name);
            _writer.WritePropertyName("instance");
            Visit(node.Expression);
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _writer.WritePropertyName("nodetype");
            _writer.WriteValue(node.NodeType.ToString().ToLowerInvariant());
            _writer.WritePropertyName("method");
            _writer.WriteValue(node.Method.Name);

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

            return node;
        }
    }
}
