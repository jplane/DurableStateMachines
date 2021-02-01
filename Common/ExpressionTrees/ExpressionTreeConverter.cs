using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DSM.Common.ExpressionTrees
{
    public class ExpressionTreeConverter : JsonConverter<LambdaExpression>
    {
        public override void WriteJson(JsonWriter writer, LambdaExpression expression, JsonSerializer serializer)
        {
            Debug.Assert(writer != null);
            Debug.Assert(expression != null);

            var expressionTreeSerializer = new ExpressionTreeJsonSerializer(writer);

            expressionTreeSerializer.Visit(expression);
        }

        public override LambdaExpression ReadJson(JsonReader reader,
                                                  Type objectType,
                                                  LambdaExpression expression,
                                                  bool hasExistingValue,
                                                  JsonSerializer serializer)
        {
            Debug.Assert(reader != null);

            var expressionTreeDeserializer = new ExpressionTreeJsonDeserializer(reader);

            return expressionTreeDeserializer.Visit();
        }
    }
}
