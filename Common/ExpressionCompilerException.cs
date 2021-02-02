using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DSM.Common
{
    public sealed class ExpressionCompilerException : ApplicationException
    {
        private readonly string[] _errors;

        internal ExpressionCompilerException(IEnumerable<string> errors)
            : base("Unable to compile expression.")
        {
            _errors = errors.ToArray();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("__errors", _errors);
        }
    }
}
