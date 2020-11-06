using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine
{
    internal static class XmlExtensions
    {
        const string xmlns = "{http://www.w3.org/2005/07/scxml}";

        public static XElement ScxmlElement(this XElement element, string name)
        {
            element.CheckArgNull(nameof(element));

            return element.Element($"{xmlns}{name}");
        }

        public static IEnumerable<XElement> ScxmlElements(this XElement element, string name)
        {
            element.CheckArgNull(nameof(element));

            return element.Elements($"{xmlns}{name}");
        }

        public static bool ScxmlNameEquals(this XElement element, string localName)
        {
            element.CheckArgNull(nameof(element));

            return element.Name == $"{xmlns}{localName}";
        }

        public static bool ScxmlNameIn(this XElement element, params string[] localNames)
        {
            element.CheckArgNull(nameof(element));

            return localNames.Any(n => element.ScxmlNameEquals(n));
        }

        private static XObject[] GetElementsAndAttributes(this XDocument document)
        {
            Debug.Assert(document != null);

            var result = document.Annotation<XObject[]>();

            if (result == null)
            {
                var xobjs = new List<XObject>();

                foreach (var element in document.Descendants())
                {
                    xobjs.Add(element);

                    foreach (var attribute in element.Attributes())
                    {
                        xobjs.Add(attribute);
                    }
                }

                result = xobjs.ToArray();

                document.AddAnnotation(result);
            }

            return result;
        }

        private static int GetDocumentPosition(this XObject xobj)
        {
            Debug.Assert(xobj != null);

            var metadata = xobj.Annotation<XObjectMetadata>();

            if (metadata == null)
            {
                var items = xobj.Document.GetElementsAndAttributes();

                var idx = Array.IndexOf(items, xobj);

                metadata = new XObjectMetadata { DocumentPosition = idx };

                xobj.AddAnnotation(metadata);
            }

            return metadata.DocumentPosition;
        }

        public static int GetDocumentOrder(XObject x1, XObject x2)
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
                var x1Pos = x1.GetDocumentPosition();

                var x2Pos = x2.GetDocumentPosition();

                return x1Pos == x2Pos ? 0 : x1Pos > x2Pos ? 1 : -1;
            }
        }

        public static int GetReverseDocumentOrder(XObject x1, XObject x2)
        {
            var order = GetDocumentOrder(x1, x2);

            return order * -1;
        }
    }

    internal class XObjectMetadata
    {
        public int DocumentPosition;
    }
}
