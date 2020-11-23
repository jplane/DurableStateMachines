using System;

namespace StateChartsDotNet.Metadata.Xml
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
