using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace StateChartsDotNet.Common
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
            _reader.Read(); // start obj

            _reader.Read(); // prop name
            var parmCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            _parms = Enumerable.Range(0, parmCount.Value)
                               .Select(_ => VisitParameter())
                               .ToDictionary(p => p.Name, p => p);

            _reader.Read(); // end array

            _reader.Read(); // prop name
            var body = VisitExpression();

            _reader.Read(); // end obj

            return Expression.Lambda(body, _parms.Values);
        }

        public Expression VisitExpression()
        {
            Expression expr = null;

            _reader.Read(); // start obj

            _reader.Read(); // prop name
            var nodeType = Enum.Parse<ExpressionType>(_reader.ReadAsString(), true);

            switch (nodeType)
            {
                case ExpressionType.Parameter:
                    _reader.Read();                     // prop name
                    var name = _reader.ReadAsString();
                    _reader.Read();                     // safely ignore
                    _reader.Read();                     // safely ignore
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

        public ParameterExpression VisitParameter()
        {
            _reader.Read();
            _reader.Read();   // safely ignore
            _reader.Read();   // safely ignore
            _reader.Read();   // safely ignore
            var name = _reader.ReadAsString();
            _reader.Read();   // safely ignore
            var type = Type.GetType(_reader.ReadAsString());
            _reader.Read();
            return Expression.Parameter(type, name);
        }

        public ConstantExpression VisitConstant()
        {
            _reader.Read();   // safely ignore
            var type = Type.GetType(_reader.ReadAsString());
            _reader.Read();   // safely ignore
            var value = JsonConvert.DeserializeObject(_reader.ReadAsString(), type);
            return Expression.Constant(value);
        }

        public BinaryExpression VisitBinary(ExpressionType nodeType)
        {
            _reader.Read();   // safely ignore
            var left = VisitExpression();
            _reader.Read();   // safely ignore
            var right = VisitExpression();
            return Expression.MakeBinary(nodeType, left, right);
        }

        public UnaryExpression VisitUnary(ExpressionType nodeType)
        {
            _reader.Read();   // safely ignore
            var type = Type.GetType(_reader.ReadAsString());
            _reader.Read();   // safely ignore
            var operand = VisitExpression();
            return Expression.MakeUnary(nodeType, operand, type);
        }

        public IndexExpression VisitIndex()
        {
            _reader.Read();   // safely ignore
            var name = _reader.ReadAsString();
            _reader.Read();   // safely ignore
            var argCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            var args = Enumerable.Range(0, argCount.Value)
                                 .Select(_ => VisitExpression())
                                 .ToArray();

            _reader.Read();   // end array

            _reader.Read();   // safely ignore
            var obj = VisitExpression();

            var prop = obj.Type.GetProperty(name);
            return Expression.MakeIndex(obj, prop, args);
        }

        public MemberExpression VisitMember()
        {
            _reader.Read();   // safely ignore
            var name = _reader.ReadAsString();
            _reader.Read();   // safely ignore
            var obj = VisitExpression();
            var member = obj.Type.GetMember(name).Single();
            return Expression.MakeMemberAccess(obj, member);
        }

        public MethodCallExpression VisitMethodCall()
        {
            _reader.Read();   // safely ignore
            var name = _reader.ReadAsString();
            _reader.Read();   // safely ignore
            var argCount = _reader.ReadAsInt32();

            _reader.Read(); // prop name
            _reader.Read(); // start array

            var args = Enumerable.Range(0, argCount.Value)
                                 .Select(_ => VisitExpression())
                                 .ToArray();

            _reader.Read();   // end array

            _reader.Read();   // safely ignore
            var obj = VisitExpression();

            var method = obj.Type.GetMethod(name);
            return Expression.Call(obj, method, args);
        }
    }
}
