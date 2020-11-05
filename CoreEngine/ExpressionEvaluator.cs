using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Text;

namespace CoreEngine
{
    internal class ExpressionEvaluator
    {
        private readonly ExecutionContext _context;

        public ExpressionEvaluator(ExecutionContext context)
        {
            _context = context;
        }

        public object Eval(string expression)
        {
            var globals = new ScriptGlobals
            {
                data = _context.ScriptData
            };

            var decodedExpr = WebUtility.HtmlDecode(expression);

            var options = ScriptOptions.Default
                                       .AddReferences(typeof(DynamicObject).Assembly,
                                                      typeof(CSharpArgumentInfo).Assembly);

            return CSharpScript.EvaluateAsync<object>(decodedExpr, options, globals).Result;
        }
    }

    public class ScriptGlobals
    {
        public dynamic data { get; internal set; }
    }
}
