using System;
using Caliburn.Micro;
using NLog;

namespace MarkPad.Framework
{
    internal class NLogAdapter : ILog
    {
        private readonly Logger logger;

        public NLogAdapter(Type type)
        {
            logger = NLog.LogManager.GetLogger(type.FullName);
        }

        #region ILog Members

        public void Error(Exception exception)
        {
            logger.ErrorException(exception.Message, exception);
        }

        public void Info(string format, params object[] args)
        {
            logger.Info(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            logger.Warn(format, args);
        }

        #endregion
    }
}
