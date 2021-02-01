using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DSM.Common
{
    internal static class MemberResolutionExtensions
    {
        public static MemberInfo ExtractMember<TData, TValue>(this Expression<Func<TData, TValue>> expr, string name)
        {
            if (expr.Body is MemberExpression me)
            {
                return me.Member;
            }
            else if (expr.Body is UnaryExpression ue &&
                     ue.NodeType == ExpressionType.Convert &&
                     ue.Operand is MemberExpression me2)
            {
                return me2.Member;
            }
            else
            {
                throw new InvalidOperationException($"Unable to resolve public instance field/property lookup for '{name}'.");
            }
        }
    }
}
