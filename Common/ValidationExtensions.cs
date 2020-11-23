using System;

namespace StateChartsDotNet.Common
{
    public static class ValidationExtensions
    {
        public static void CheckArgNull(this object argument, string name)
        {
            if (argument is string s && string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentNullException(name);
            }
            else if (argument == null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
