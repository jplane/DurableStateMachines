using System;
using System.Collections.Generic;
using System.Text;

namespace CoreEngine
{
    internal static class ValidationExtensions
    {
        public static void CheckArgNull(this object argument, string name)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
