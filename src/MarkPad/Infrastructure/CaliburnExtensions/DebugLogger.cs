using System;
using System.Diagnostics;

namespace MarkPad.Infrastructure.CaliburnExtensions
{
    internal class DebugLogger : Caliburn.Micro.ILog
    {
        private readonly string typeName;

        public DebugLogger(Type type)
        {
            typeName = type.FullName;

            if (type.Namespace == "Caliburn.Micro")
                typeName = "CM." + type.Name;

            if (typeName != null) typeName = typeName.PadRight(20) + " ";
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
