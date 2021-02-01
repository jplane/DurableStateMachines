using Newtonsoft.Json;
using DSM.Common;
using DSM.Common.Model;
using DSM.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DSM.Metadata.Execution
{
    public class Raise<TData> : ExecutableContent<TData>, IRaiseMetadata
    {
        private MemberInfo _target;
        private Lazy<Func<dynamic, string>> _messageGetter;

        public Raise()
        {
            _messageGetter = new Lazy<Func<dynamic, string>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.Location))
                {
                    return data => data[this.Location];
                }
                else if (_target != null)
                {
                    if (_target is PropertyInfo pi)
                    {
                        return data => (string) pi.GetValue((TData)data, null);
                    }
                    else if (_target is FieldInfo fi)
                    {
                        return data => (string) fi.GetValue((TData)data);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unable to resolve member into public property or field.");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(this.MessageExpression))
                {
                    return ExpressionCompiler.Compile<string>(this.MessageExpression);
                }
                else if (this.MessageFunction != null)
                {
                    return data => this.MessageFunction((TData) data);
                }
                else
                {
                    return _ => this.Message;
                }
            });
        }

        public Expression<Func<TData, object>> Target
        {
            set => _target = value.ExtractMember(nameof(Target));
        }

        [JsonProperty("location")]
        private string Location { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        public Func<TData, string> MessageFunction { get; set; }

        [JsonProperty("messageexpression")]
        private string MessageExpression { get; set; }

        string IRaiseMetadata.GetMessage(dynamic data) => _messageGetter.Value(data);

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.MessageExpression) &&
                this.MessageFunction == null &&
                this.Message == null)
            {
                errors.Add("One of Message, MessageExpression, or MessageFunction must be set.");
            }

            if (string.IsNullOrWhiteSpace(this.Location) && _target == null)
            {
                errors.Add("Location/target is invalid.");
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }
    }
}
