using System;

namespace DSM.Common
{
    public static class ValidationExtensions
    {
        public static void CheckArgNull(this string argument, string name)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void CheckArgNull(this object argument, string name)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
