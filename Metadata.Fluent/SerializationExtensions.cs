using Newtonsoft.Json;
using StateChartsDotNet.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;

namespace StateChartsDotNet.Metadata.Fluent
{
    internal static class SerializationExtensions
    {
        const string NullValue = "__nil";

        public static void WriteObject(this BinaryWriter writer, object obj)
        {
            if (obj == null)
            {
                writer.Write(NullValue);
            }
            else
            {
                writer.Write(string.Empty); // "not null"
                writer.Write(JsonConvert.SerializeObject(obj));
            }
        }

        public static void WriteNullableString(this BinaryWriter writer, string s)
        {
            if (s == null)
            {
                writer.Write(NullValue);
            }
            else
            {
                writer.Write(s);
            }
        }

        public static void Write(this BinaryWriter writer, Expression expr)
        {
            if (expr == null)
            {
                writer.Write(NullValue);
            }
            else
            {
                writer.Write(string.Empty); // "not null"

                var serializer = new ExpressionTreeBinarySerializer(writer);

                serializer.Visit(expr);
            }
        }

        public static void Write<T>(this BinaryWriter writer,
                                    T metadata,
                                    Action<T, BinaryWriter> write)
        {
            if (metadata == null)
            {
                writer.Write(NullValue);
            }
            else
            {
                writer.Write(string.Empty); // "not null"
                write(metadata, writer);
            }
        }

        public static void WriteMany<T>(this BinaryWriter writer,
                                        ICollection<T> collection,
                                        Action<T, BinaryWriter> writeItem)
        {
            writer.Write(collection.Count);

            foreach (var item in collection)
            {
                writer.Write(item, writeItem);
            }
        }

        public static object ReadObject(this BinaryReader reader)
        {
            var obj = reader.ReadString();

            if (obj != NullValue)
            {
                return JsonConvert.DeserializeObject(reader.ReadString());
            }
            else
            {
                return null;
            }
        }

        public static string ReadNullableString(this BinaryReader reader)
        {
            var s = reader.ReadString();

            return s != NullValue ? s : null;
        }

        public static TFunc Read<TFunc>(this BinaryReader reader) where TFunc : LambdaExpression
        {
            var null_indicator = reader.ReadString();

            if (null_indicator == NullValue)
            {
                return null;
            }

            var deserializer = new ExpressionTreeBinaryDeserializer(reader);

            return (TFunc) deserializer.Visit();
        }

        public static TMetadata Read<TMetadata>(this BinaryReader reader,
                                                Func<BinaryReader, TMetadata> readItem,
                                                Action<TMetadata> initializer)
        {
            var null_indicator = reader.ReadString();

            if (null_indicator != NullValue)
            {
                var metadata = readItem(reader);

                Debug.Assert(metadata != null);

                initializer(metadata);

                return metadata;
            }

            return default;
        }

        public static IEnumerable<TMetadata> ReadMany<TMetadata>(this BinaryReader reader,
                                                                 Func<BinaryReader, TMetadata> readItem,
                                                                 Action<TMetadata> initializer)
        {
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                yield return reader.Read(readItem, initializer);
            }
        }
    }
}
