using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace StateChartsDotNet.Common
{
    internal class ExpressionTreeBinarySerializer : ExpressionVisitor
    {
        private readonly BinaryWriter _writer;

        public ExpressionTreeBinarySerializer(BinaryWriter writer)
        {
            _writer = writer;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var lambda = (LambdaExpression)node;

            _writer.Write(lambda.Parameters.Count);

            foreach (var parm in lambda.Parameters)
            {
                _writer.Write(parm.Name);
                _writer.Write(parm.Type.AssemblyQualifiedName);
            }

            Visit(lambda.Body);

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _writer.Write(node.NodeType.ToString().ToLowerInvariant());
            _writer.Write(node.Name);
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _writer.Write(node.NodeType.ToString().ToLowerInvariant());
            _writer.Write(node.Type.AssemblyQualifiedName);
            _writer.Write(JsonConvert.SerializeObject(node.Value));
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _writer.Write(node.NodeType.ToString().ToLowerInvariant());
            Visit(node.Left);
            Visit(node.Right);
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            _writer.Write(node.NodeType.ToString().ToLowerInvariant());
            _writer.Write(node.Type.AssemblyQualifiedName);
            Visit(node.Operand);
            return node;
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            _writer.Write(node.NodeType.ToString().ToLowerInvariant());
            _writer.Write(node.Indexer.DeclaringType.AssemblyQualifiedName);
            _writer.Write(node.Indexer.MetadataToken);
            _writer.Write(node.Arguments.Count);

            foreach (var arg in node.Arguments)
            {
                Visit(arg);
            }

            Visit(node.Object);

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _writer.Write(node.NodeType.ToString().ToLowerInvariant());
            _writer.Write(node.Member.DeclaringType.AssemblyQualifiedName);
            _writer.Write(node.Member.MetadataToken);

            _writer.Write(node.Expression != null);

            if (node.Expression != null)
            {
                Visit(node.Expression);
            }

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _writer.Write(node.NodeType.ToString().ToLowerInvariant());
            _writer.Write(node.Method.DeclaringType.AssemblyQualifiedName);
            _writer.Write(node.Method.MetadataToken);
            _writer.Write(node.Arguments.Count);

            foreach (var arg in node.Arguments)
            {
                Visit(arg);
            }

            _writer.Write(node.Object != null);

            if (node.Object != null)
            {
                Visit(node.Object);
            }

            return node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            _writer.Write(node.NodeType.ToString().ToLowerInvariant());

            VisitBindings(node.Bindings);

            Visit(node.NewExpression);

            return node;
        }

        private void VisitBindings(IReadOnlyCollection<MemberBinding> bindings)
        {
            _writer.Write(bindings.Count);

            foreach (var binding in bindings)
            {
                _writer.Write(binding.BindingType.ToString().ToLowerInvariant());
                _writer.Write(binding.Member.DeclaringType.AssemblyQualifiedName);
                _writer.Write(binding.Member.MetadataToken);

                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        {
                            var assignmentBinding = (MemberAssignment)binding;
                            Visit(assignmentBinding.Expression);
                        }
                        break;

                    case MemberBindingType.MemberBinding:
                        {
                            var memberMemberBinding = (MemberMemberBinding)binding;
                            VisitBindings(memberMemberBinding.Bindings);
                        }
                        break;

                    case MemberBindingType.ListBinding:
                        {
                            var listBinding = (MemberListBinding)binding;
                            VisitInitializers(listBinding.Initializers);
                        }
                        break;
                }
            }
        }

        private void VisitInitializers(IReadOnlyCollection<ElementInit> initializers)
        {
            _writer.Write(initializers.Count);

            foreach (var initializer in initializers)
            {
                _writer.Write(initializer.AddMethod.DeclaringType.AssemblyQualifiedName);

                _writer.Write(initializer.AddMethod.MetadataToken);

                _writer.Write(initializer.Arguments.Count);

                foreach (var arg in initializer.Arguments)
                {
                    Visit(arg);
                }
            }
        }

        protected override Expression VisitNew(NewExpression node)
        {
            _writer.Write(node.NodeType.ToString().ToLowerInvariant());
            _writer.Write(node.Type.AssemblyQualifiedName);
            _writer.Write(node.Constructor.MetadataToken);

            _writer.Write(node.Members.Count);

            foreach (var member in node.Members)
            {
                _writer.Write(member.MetadataToken);
            }

            _writer.Write(node.Arguments.Count);

            foreach (var arg in node.Arguments)
            {
                Visit(arg);
            }

            return node;
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            _writer.Write(node.NodeType.ToString().ToLowerInvariant());

            VisitInitializers(node.Initializers);

            Visit(node.NewExpression);

            return node;
        }
    }
}
