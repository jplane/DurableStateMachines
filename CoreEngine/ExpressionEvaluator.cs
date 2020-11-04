using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreEngine
{
    internal class ExpressionEvaluator
    {
        private readonly ExecutionState _state;

        public ExpressionEvaluator(ExecutionState state)
        {
            _state = state;
        }

        public object Eval(string expression)
        {
            var globals = new
            {
                data = _state.ScriptData
            };

            return CSharpScript.EvaluateAsync(expression, globals: globals).Result;
        }
    }
}
