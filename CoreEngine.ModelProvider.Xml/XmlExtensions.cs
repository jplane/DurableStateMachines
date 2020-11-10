using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml
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

        private static long GetDocumentPosition(this XObject xobj)
        {
            Debug.Assert(xobj != null);

            var metadata = xobj.Annotation<XObjectMetadata>();

            if (metadata == null)
            {
                var idx = 0L;

                Action<XObject> addAnnotation = xo =>
                {
                    var md = new XObjectMetadata
                    {
                        DocumentPosition = idx++
                    };

                    xo.AddAnnotation(md);

                    if (xo == xobj)
                    {
                        metadata = md;
                    }
                };

                foreach (var element in xobj.Document.Descendants())
                {
                    addAnnotation(element);

                    foreach (var attribute in element.Attributes())
                    {
                        addAnnotation(attribute);
                    }
                }
            }

            Debug.Assert(metadata != null);

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
    }

    internal class XObjectMetadata
    {
        public long DocumentPosition;
    }
}
