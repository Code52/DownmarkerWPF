using System;
using System.Diagnostics;

namespace MarkPad.Framework
{
    internal class DebugLogger : Caliburn.Micro.ILog
    {
        private readonly string typeName;

        public DebugLogger(Type type)
        {
            this.typeName = type.FullName;

            if (type.Namespace == "Caliburn.Micro")
                this.typeName = "CM." + type.Name;

            this.typeName = this.typeName.PadRight(20) + " ";
        }

        public void Info(string format, params object[] args)
        {
            Debug.Write(typeName, "INFO");
            Debug.WriteLine(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Debug.Write(typeName, "WARN");
            Debug.WriteLine(format, args);
        }

        public void Error(Exception exception)
        {
            Debug.Write(typeName, "ERROR");
            Debug.WriteLine(exception);
        }
    }
}
