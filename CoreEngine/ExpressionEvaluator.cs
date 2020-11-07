using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace CoreEngine
{
    internal class ExpressionEvaluator
    {
        private readonly ExecutionContext _context;

        public ExpressionEvaluator(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            _context = context;
        }

        public Task<T> Eval<T>(string expression)
        {
            expression.CheckArgNull(nameof(expression));

            var globals = new ScriptGlobals
            {
                data = _context.ScriptData
            };

            var decodedExpr = WebUtility.HtmlDecode(expression);

            var options = ScriptOptions.Default
                                       .AddReferences(typeof(DynamicObject).Assembly,
                                                      typeof(CSharpArgumentInfo).Assembly);

            return CSharpScript.EvaluateAsync<T>(decodedExpr, options, globals);
        }
    }

    public class ScriptGlobals
    {
        public dynamic data { get; internal set; }

        internal ExecutionContext _context { get; set; }

        public bool In(params string[] states)
        {
            return _context.Configuration.Any(s => states.Contains(s.Id));
        }
    }
}
