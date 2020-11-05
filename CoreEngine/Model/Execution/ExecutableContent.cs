using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.Model.Execution
{
    internal abstract class ExecutableContent
    {
        private readonly XElement _element;

        protected ExecutableContent(XElement element)
        {
            _element = element;
        }

        public static XObject GetXObject(ExecutableContent content)
        {
            return content._element;
        }

        public static ExecutableContent Create(XElement element)
        {
            ExecutableContent content = null;

            switch (element.Name.LocalName)
            {
                case "if":
                    content = new If(element);
                    break;

                case "raise":
                    content = new Raise(element);
                    break;

                case "script":
                    content = new Script(element);
                    break;

                case "foreach":
                    content = new Foreach(element);
                    break;

                case "log":
                    content = new Log(element);
                    break;

                case "send":
                    content = new Send(element);
                    break;

                case "cancel":
                    content = new Cancel(element);
                    break;

                case "assign":
                    content = new Assign(element);
                    break;
            }

            return content;
        }

        public abstract void Execute(ExecutionContext context);
    }
}
