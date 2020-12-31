using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace StateChartsDotNet.Metadata.Fluent
{
    internal static class SerializationExtensions
    {
        const string NullValue = "__nil";

        public static void WriteObject(this BinaryWriter writer, object obj)
        {
            if (obj == null)
                writer.Write(NullValue);
            else
                writer.Write(JsonConvert.SerializeObject(obj));
        }

        public static void WriteNullableString(this BinaryWriter writer, string s)
        {
            if (s == null)
                writer.Write(NullValue);
            else
                writer.Write(s);
        }

        public static void Write(this BinaryWriter writer, Delegate del)
        {
            if (del == null)
            {
                writer.Write(NullValue);
            }
            else
            {
                if (!del.Method.IsStatic)
                {
                    throw new SerializationException("Cannot serialize instance method references.");
                }

                writer.Write(del.Method.DeclaringType.AssemblyQualifiedName);
                writer.Write(del.Method.Name);
                writer.Write(del.Method.IsPublic);
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
                writer.Write(true);
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
                return JsonConvert.DeserializeObject(obj);
            else
                return null;
        }

        public static string ReadNullableString(this BinaryReader reader)
        {
            var s = reader.ReadString();

            return s != NullValue ? s : null;
        }

        public static TFunc Read<TFunc>(this BinaryReader reader) where TFunc : Delegate
        {
            var null_indidcator = reader.ReadString();

            if (null_indidcator == NullValue)
                return null;

            var aqtn = reader.ReadString();
            var methodName = reader.ReadString();
            var isPublic = reader.ReadBoolean();

            Debug.Assert(!string.IsNullOrWhiteSpace(aqtn));
            Debug.Assert(!string.IsNullOrWhiteSpace(methodName));

            var type = Type.GetType(aqtn);

            if (type == null)
            {
                throw new SerializationException("Unable to load .NET type: " + aqtn);
            }

            var flags = isPublic ? BindingFlags.Static | BindingFlags.Public :
                                   BindingFlags.Static | BindingFlags.NonPublic;

            var method = type.GetMethod(methodName, flags);

            if (method == null)
            {
                throw new SerializationException($"Unable to resolve method '{methodName}' on .NET type '{aqtn}'.");
            }

            return (TFunc) method.CreateDelegate(typeof(TFunc));
        }

        public static TMetadata Read<TMetadata>(this BinaryReader reader,
                                                Func<BinaryReader, TMetadata> readItem,
                                                Action<TMetadata> initializer)
        {
            var null_indidcator = reader.ReadString();

            if (null_indidcator != NullValue)
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
