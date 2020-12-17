using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;

namespace StateChartsDotNet.Metadata.Json
{
    internal static class JsonExtensions
    {
        public static string GetUniqueElementPath(this JToken element)
        {
            return element.Path;
        }

        public static void InitDocumentPosition(this JObject xobj)
        {
            Debug.Assert(xobj != null);

            var idx = 0L;

            void addAnnotation(JObject xo)
            {
                var md = new JObjectMetadata
                {
                    DocumentPosition = idx++
                };

                xo.AddAnnotation(md);
            };

            void visit(JContainer obj)
            {
                if (obj is JObject jobj)
                {
                    addAnnotation(jobj);

                    foreach (var jprop in jobj.Properties())
                    {
                        visit(jprop);
                    }
                }
                else if (obj is JArray)
                {
                    foreach (var item in obj)
                    {
                        if (item is JContainer jc)
                        {
                            visit(jc);
                        }
                    }
                }
                else if (obj is JProperty jprop)
                {
                    var value = jprop.Value;

                    if (value is JContainer jc)
                    {
                        visit(jc);
                    }
                }
            }

            visit(xobj);
        }

        public static int GetDocumentOrder(JObject x1, JObject x2)
        {
            if (x1 == null && x2 == null)
            {
                return 0;
            }
            else if (x1 == null)
            {
                return -1;
            }
            else if (x2 == null)
            {
                return 1;
            }
            else
            {
                var x1Pos = x1.Annotation<JObjectMetadata>().DocumentPosition;

                var x2Pos = x2.Annotation<JObjectMetadata>().DocumentPosition;

                return x1Pos == x2Pos ? 0 : x1Pos > x2Pos ? 1 : -1;
            }
        }
    }

    internal class JObjectMetadata
    {
        public long DocumentPosition;
    }
}
