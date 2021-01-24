using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace StateChartsDotNet.Common.Exceptions
{
    public sealed class MetadataValidationException : StateChartException
    {
        private readonly IReadOnlyDictionary<string, string[]> _errors;

        public MetadataValidationException(IReadOnlyDictionary<string, string[]> errors)
            : base(FormatMessage(errors))
        {
            _errors = errors;
        }

        private static string FormatMessage(IReadOnlyDictionary<string, string[]> errors)
        {
            var buffer = new StringBuilder();

            buffer.AppendLine();

            foreach (var key in errors.Keys)
            {
                buffer.AppendLine($"{key}:");

                foreach (var message in errors[key])
                {
                    buffer.AppendLine($"\t{message}");
                }

                buffer.AppendLine();
            }

            return buffer.ToString();
        }

        public IEnumerable<string> Keys => _errors.Keys;

        public string[] this[string key] => _errors[key];
    }
}
