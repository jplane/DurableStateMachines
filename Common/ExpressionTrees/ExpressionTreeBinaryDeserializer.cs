using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace StateChartsDotNet.Common.ExpressionTrees
{
    internal class ExpressionTreeBinaryDeserializer
    {
        private readonly BinaryReader _reader;

        private Dictionary<string, ParameterExpression> _parms;

        public ExpressionTreeBinaryDeserializer(BinaryReader reader)
        {
            _reader = reader;
            _parms = new Dictionary<string, ParameterExpression>();
        }

        public LambdaExpression Visit()
        {
            var parmCount = _reader.ReadInt32();

            _parms = Enumerable.Range(0, parmCount)
                               .Select(_ => VisitParameter())
                               .ToDictionary(p => p.Name, p => p);

            var body = VisitExpression();

            return Expression.Lambda(body, _parms.Values);
        }

        private Expression VisitExpression()
        {
            Expression expr = null;

            var nodeType = Enum.Parse<ExpressionType>(_reader.ReadString(), true);

            switch (nodeType)
            {
                case ExpressionType.Parameter:
                    var name = _reader.ReadString();
                    expr = _parms[name];
                    break;

                case ExpressionType.Constant:
                    expr = VisitConstant();
                    break;

                case ExpressionType.MemberAccess:
                    expr = VisitMember();
                    break;

                case ExpressionType.Index:
                    expr = VisitIndex();
                    break;

                case ExpressionType.Call:
                    expr = VisitMethodCall();
                    break;

                case ExpressionType.MemberInit:
                    expr = VisitMemberInit();
                    break;

                case ExpressionType.ListInit:
                    expr = VisitListInit();
                    break;

                case ExpressionType.New:
                    expr = VisitNew();
                    break;

                case ExpressionType.Add:
                case ExpressionType.AddAssign:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractAssign:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.Divide:
                case ExpressionType.DivideAssign:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.AndAssign:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.OrAssign:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Modulo:
                case ExpressionType.ModuloAssign:
                case ExpressionType.Coalesce:
                    expr = VisitBinary(nodeType);
                    break;

                case ExpressionType.Decrement:
                case ExpressionType.Increment:
                case ExpressionType.Negate:
                case ExpressionType.Not:
                case ExpressionType.OnesComplement:
                case ExpressionType.PostDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.Convert:
                    expr = VisitUnary(nodeType);
                    break;

                default:
                    throw new NotSupportedException();
            }

            return expr;
        }

        private ParameterExpression VisitParameter()
        {
            var name = _reader.ReadString();
            var type = Type.GetType(_reader.ReadString());
            return Expression.Parameter(type, name);
        }

        private ConstantExpression VisitConstant()
        {
            var type = Type.GetType(_reader.ReadString());
            var value = JsonConvert.DeserializeObject(_reader.ReadString(), type);
            return Expression.Constant(value);
        }

        private BinaryExpression VisitBinary(ExpressionType nodeType)
        {
            var left = VisitExpression();
            var right = VisitExpression();
            return Expression.MakeBinary(nodeType, left, right);
        }

        private UnaryExpression VisitUnary(ExpressionType nodeType)
        {
            var type = Type.GetType(_reader.ReadString());
            var operand = VisitExpression();
            return Expression.MakeUnary(nodeType, operand, type);
        }

        private IndexExpression VisitIndex()
        {
            var type = Type.GetType(_reader.ReadString());

            var token = _reader.ReadInt32();

            var prop = type.GetProperties().Single(p => p.MetadataToken == token);

            var argCount = _reader.ReadInt32();

            var args = Enumerable.Range(0, argCount)
                                 .Select(_ => VisitExpression())
                                 .ToArray();

            var obj = VisitExpression();

            return Expression.MakeIndex(obj, prop, args);
        }

        private MemberExpression VisitMember()
        {
            var type = Type.GetType(_reader.ReadString());

            var token = _reader.ReadInt32();

            var member = type.GetMembers().Single(m => m.MetadataToken == token);

            Expression obj = null;

            var isInstance = _reader.ReadBoolean();

            if (isInstance)
            {
                obj = VisitExpression();
            }

            return Expression.MakeMemberAccess(obj, member);
        }

        private MethodCallExpression VisitMethodCall()
        {
            var type = Type.GetType(_reader.ReadString());
            var token = _reader.ReadInt32();
            var method = type.GetMethods().Single(m => m.MetadataToken == token);

            var argCount = _reader.ReadInt32();

            var args = Enumerable.Range(0, argCount)
                                 .Select(_ => VisitExpression())
                                 .ToArray();

            var isInstance = _reader.ReadBoolean();

            if (isInstance)
            {
                var obj = VisitExpression();
                return Expression.Call(obj, method, args);
            }
            else
            {
                return Expression.Call(method, args);
            }
        }

        private MemberInitExpression VisitMemberInit()
        {
            var bindings = VisitBindings();

            var newExpr = VisitNew();

            return Expression.MemberInit(newExpr, bindings);
        }

        private IReadOnlyCollection<MemberBinding> VisitBindings()
        {
            var count = _reader.ReadInt32();

            var bindings = Enumerable.Range(0, count).Select(_ =>
            {
                var bindingType = Enum.Parse<MemberBindingType>(_reader.ReadString(), true);

                var type = Type.GetType(_reader.ReadString());

                var token = _reader.ReadInt32();

                var member = type.GetMembers().Single(m => m.MetadataToken == token);

                MemberBinding binding = null;

                switch (bindingType)
                {
                    case MemberBindingType.Assignment:
                        {
                            var expr = VisitExpression();
                            binding = Expression.Bind(member, expr);
                        }
                        break;

                    case MemberBindingType.MemberBinding:
                        {
                            var bindings = VisitBindings();
                            binding = Expression.MemberBind(member, bindings);
                        }
                        break;

                    case MemberBindingType.ListBinding:
                        {
                            var initializers = VisitInitializers();
                            binding = Expression.ListBind(member, initializers);
                        }
                        break;
                }

                return binding;
            }).ToArray();

            return bindings;
        }

        private IReadOnlyCollection<ElementInit> VisitInitializers()
        {
            var count = _reader.ReadInt32();

            var initializers = Enumerable.Range(0, count).Select(_ =>
            {
                var type = Type.GetType(_reader.ReadString());

                var token = _reader.ReadInt32();

                var addmethod = type.GetMethods().Single(m => m.MetadataToken == token);

                var argCount = _reader.ReadInt32();

                var args = Enumerable.Range(0, argCount)
                                     .Select(_ => VisitExpression())
                                     .ToArray();

                return Expression.ElementInit(addmethod, args);
            }).ToArray();

            return initializers;
        }

        private NewExpression VisitNew()
        {
            var type = Type.GetType(_reader.ReadString());
            var token = _reader.ReadInt32();
            var ctor = type.GetConstructors().Single(c => c.MetadataToken == token);

            var count = _reader.ReadInt32();

            token = _reader.ReadInt32();

            var members = Enumerable.Range(0, count)
                                    .Select(_ => type.GetMembers().Single(m => m.MetadataToken == token))
                                    .ToArray();

            count = _reader.ReadInt32();

            var args = Enumerable.Range(0, count)
                                 .Select(_ => VisitExpression())
                                 .ToArray();

            return Expression.New(ctor, args, members);
        }

        private ListInitExpression VisitListInit()
        {
            var initializers = VisitInitializers();

            var newExpr = VisitNew();

            return Expression.ListInit(newExpr, initializers);
        }
    }
}
