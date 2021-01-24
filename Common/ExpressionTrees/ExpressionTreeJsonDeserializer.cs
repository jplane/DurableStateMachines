using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace StateChartsDotNet.Common.ExpressionTrees
{
    internal class ExpressionTreeJsonDeserializer
    {
        private readonly JsonReader _reader;

        private Dictionary<string, ParameterExpression> _parms;

        public ExpressionTreeJsonDeserializer(JsonReader reader)
        {
            _reader = reader;
            _parms = new Dictionary<string, ParameterExpression>();
        }

        public LambdaExpression Visit()
        {
            _reader.Read(); // prop name
            var parmCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            _parms = Enumerable.Range(0, parmCount.Value)
                               .Select(_ =>
                               {
                                   _reader.Read(); // start obj

                                   _reader.Read(); // prop name
                                   var name = _reader.ReadAsString();

                                   _reader.Read(); // prop name
                                   var type = Type.GetType(_reader.ReadAsString());

                                   _reader.Read(); // end obj

                                   return Expression.Parameter(type, name);
                               })
                               .ToDictionary(p => p.Name, p => p);

            _reader.Read(); // end array

            _reader.Read(); // prop name
            var body = VisitExpression();

            _reader.Read(); // end obj

            return Expression.Lambda(body, _parms.Values);
        }

        private Expression VisitExpression()
        {
            Expression expr = null;

            _reader.Read(); // start obj

            _reader.Read(); // prop name
            var nodeType = Enum.Parse<ExpressionType>(_reader.ReadAsString(), true);

            switch (nodeType)
            {
                case ExpressionType.Parameter:
                    expr = VisitParameterReference();
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

                case ExpressionType.New:
                    expr = VisitNewExpression();
                    break;

                case ExpressionType.ListInit:
                    expr = VisitListInit();
                    break;

                case ExpressionType.NewArrayInit:
                    expr = VisitNewArray();
                    break;

                case ExpressionType.Conditional:
                    expr = VisitConditional();
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

            _reader.Read(); // end obj

            return expr;
        }

        private ParameterExpression VisitParameterReference()
        {
            _reader.Read(); // prop name
            var name = _reader.ReadAsString();
            return _parms[name];
        }

        private ConstantExpression VisitConstant()
        {
            _reader.Read(); // prop name
            var type = Type.GetType(_reader.ReadAsString());
            _reader.Read(); // prop name
            var value = JsonConvert.DeserializeObject(_reader.ReadAsString(), type);
            return Expression.Constant(value);
        }

        private BinaryExpression VisitBinary(ExpressionType nodeType)
        {
            _reader.Read(); // prop name
            var left = VisitExpression();
            _reader.Read(); // prop name
            var right = VisitExpression();
            return Expression.MakeBinary(nodeType, left, right);
        }

        private UnaryExpression VisitUnary(ExpressionType nodeType)
        {
            _reader.Read(); // prop name
            var type = Type.GetType(_reader.ReadAsString());
            _reader.Read(); // prop name
            var operand = VisitExpression();
            return Expression.MakeUnary(nodeType, operand, type);
        }

        private IndexExpression VisitIndex()
        {
            _reader.Read(); // prop name
            var type = Type.GetType(_reader.ReadAsString());
            _reader.Read(); // prop name
            var token = _reader.ReadAsInt32();
            _reader.Read(); // prop name
            var argCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            var args = Enumerable.Range(0, argCount.Value)
                                 .Select(_ => VisitExpression())
                                 .ToArray();

            _reader.Read();   // end array

            _reader.Read(); // prop name
            var obj = VisitExpression();

            var prop = type.GetProperties().Single(p => p.MetadataToken == token);

            return Expression.MakeIndex(obj, prop, args);
        }

        private MemberExpression VisitMember()
        {
            _reader.Read(); // prop name
            var type = Type.GetType(_reader.ReadAsString());
            _reader.Read(); // prop name
            var token = _reader.ReadAsInt32();

            var member = type.GetMembers().Single(m => m.MetadataToken == token);

            _reader.Read(); // prop name
            var isInstance = _reader.ReadAsBoolean();

            Expression obj = null;

            if (isInstance.Value)
            {
                _reader.Read(); // prop name
                obj = VisitExpression();
            }

            return Expression.MakeMemberAccess(obj, member);
        }

        private MethodCallExpression VisitMethodCall()
        {
            _reader.Read(); // prop name
            var type = Type.GetType(_reader.ReadAsString());
            _reader.Read(); // prop name
            var token = _reader.ReadAsInt32();

            var method = type.GetMethods().Single(m => m.MetadataToken == token);

            _reader.Read(); // prop name
            var argCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            var args = Enumerable.Range(0, argCount.Value)
                                 .Select(_ => VisitExpression())
                                 .ToArray();

            _reader.Read();   // end array

            _reader.Read(); // prop name
            var isInstance = _reader.ReadAsBoolean();

            if (isInstance.Value)
            {
                _reader.Read(); // prop name
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
            _reader.Read(); // prop name
            var bindingsCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            var bindings = Enumerable.Range(0, bindingsCount.Value)
                                     .Select(_ => VisitMemberBinding())
                                     .ToArray();

            _reader.Read();   // end array

            _reader.Read(); // prop name
            var newExpr = (NewExpression) VisitExpression();

            return Expression.MemberInit(newExpr, bindings);
        }

        private MemberBinding VisitMemberBinding()
        {
            _reader.Read(); // start obj

            _reader.Read(); // prop name
            var bindingType = Enum.Parse<MemberBindingType>(_reader.ReadAsString());
            _reader.Read(); // prop name
            var type = Type.GetType(_reader.ReadAsString());
            _reader.Read(); // prop name
            var token = _reader.ReadAsInt32();

            var member = type.GetMembers().Single(m => m.MetadataToken == token);

            MemberBinding binding = null;

            switch (bindingType)
            {
                case MemberBindingType.Assignment:
                    binding = VisitAssignmentBinding(member);
                    break;

                case MemberBindingType.MemberBinding:
                    binding = VisitMemberMemberBinding(member);
                    break;

                case MemberBindingType.ListBinding:
                    binding = VisitMemberListBinding(member);
                    break;
            }

            _reader.Read(); // end obj

            return binding;
        }

        private MemberAssignment VisitAssignmentBinding(MemberInfo member)
        {
            _reader.Read(); // prop name
            var value = VisitExpression();
            return Expression.Bind(member, value);
        }

        private MemberMemberBinding VisitMemberMemberBinding(MemberInfo member)
        {
            _reader.Read(); // prop name
            var bindingsCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            var bindings = Enumerable.Range(0, bindingsCount.Value)
                                     .Select(_ => VisitMemberBinding())
                                     .ToArray();

            _reader.Read();   // end array

            return Expression.MemberBind(member, bindings);
        }

        private MemberListBinding VisitMemberListBinding(MemberInfo member)
        {
            _reader.Read(); // prop name
            var initializersCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            var initializers = Enumerable.Range(0, initializersCount.Value)
                                         .Select(_ => VisitElementInit())
                                         .ToArray();

            _reader.Read();   // end array

            return Expression.ListBind(member, initializers);
        }

        private ElementInit VisitElementInit()
        {
            _reader.Read(); // start obj

            _reader.Read(); // prop name
            var type = Type.GetType(_reader.ReadAsString());
            _reader.Read(); // prop name
            var token = _reader.ReadAsInt32();

            var addMethod = type.GetMethods().Single(m => m.MetadataToken == token);

            _reader.Read(); // prop name
            var argCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            var args = Enumerable.Range(0, argCount.Value)
                                 .Select(_ => VisitExpression())
                                 .ToArray();

            _reader.Read();   // end array

            _reader.Read(); // end obj

            return Expression.ElementInit(addMethod, args);
        }

        private NewExpression VisitNewExpression()
        {
            _reader.Read(); // prop name
            var type = Type.GetType(_reader.ReadAsString());
            _reader.Read(); // prop name
            var token = _reader.ReadAsInt32();

            var ctor = type.GetConstructors().Single(m => m.MetadataToken == token);

            _reader.Read(); // prop name
            var membersCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            var members = Enumerable.Range(0, membersCount.Value)
                                    .Select(_ => _reader.ReadAsInt32())
                                    .Select(token => type.GetMembers().Single(m => m.MetadataToken == token))
                                    .ToArray();

            _reader.Read();   // end array

            _reader.Read(); // prop name
            var argCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            var args = Enumerable.Range(0, argCount.Value)
                                 .Select(_ => VisitExpression())
                                 .ToArray();

            _reader.Read();   // end array

            return Expression.New(ctor, args, members);
        }

        private ListInitExpression VisitListInit()
        {
            _reader.Read(); // prop name
            var initializersCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            var initializers = Enumerable.Range(0, initializersCount.Value)
                                         .Select(_ => VisitElementInit())
                                         .ToArray();

            _reader.Read();   // end array

            _reader.Read(); // prop name
            var newExpr = (NewExpression) VisitExpression();

            return Expression.ListInit(newExpr, initializers);
        }

        private NewArrayExpression VisitNewArray()
        {
            _reader.Read(); // prop name
            var type = Type.GetType(_reader.ReadAsString());

            _reader.Read(); // prop name
            var elementsCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            var elements = Enumerable.Range(0, elementsCount.Value)
                                     .Select(_ => VisitExpression())
                                     .ToArray();

            _reader.Read();   // end array

            return Expression.NewArrayInit(type, elements);
        }

        private ConditionalExpression VisitConditional()
        {
            _reader.Read(); // prop name
            var type = Type.GetType(_reader.ReadAsString());

            _reader.Read(); // prop name
            var test = VisitExpression();

            _reader.Read(); // prop name
            var ifTrue = VisitExpression();

            _reader.Read(); // prop name
            var ifFalse = VisitExpression();

            return Expression.Condition(test, ifTrue, ifFalse, type);
        }
    }
}
