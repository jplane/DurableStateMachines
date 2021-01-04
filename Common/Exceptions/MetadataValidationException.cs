using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace StateChartsDotNet.Common.Exceptions
{
    public sealed class MetadataValidationException : StateChartException
    {
        private readonly Dictionary<string, string[]> _errors;

        public MetadataValidationException(Dictionary<string, string[]> errors)
        {
            _errors = errors;
        }

        public IEnumerable<string> Keys => _errors.Keys;

        public string[] this[string key] => _errors[key];
    }
}
