using NLog;
using System;

namespace ElsaQuickstarts.Server.DashboardAndServer.Common
{
    public class Logger
    {
        NLog.Logger _logger;

        private Logger(NLog.Logger logger)
        {
            _logger = logger;
        }

        public Logger(string name) : this(LogManager.GetLogger(name))
        {

        }

        public static Logger Default { get; private set; }
        static Logger()
        {
            Default = new Logger(NLog.LogManager.GetCurrentClassLogger());
        }

        #region Debug
        public void Debug(string msg, string logName, params object[] args)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, msg);
            logEvent.Properties["mcs"] = logName;
            _logger.Debug(logEvent);
            //_logger.Debug(msg, args);
        }

        public void Debug(string msg, Exception err)
        {
            _logger.Debug(err, msg);
        }
        #endregion

        #region Info
        public void Info(string msg, string logName, params object[] args)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, msg);
            logEvent.Properties["mcs"] = logName;
            _logger.Info(logEvent);
            //_logger.Info(msg, args);
        }

        public void Info(string msg, Exception err)
        {
            _logger.Info(err, msg);
        }
        #endregion

        #region Warn
        public void Warn(string msg, string logName, params object[] args)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, msg);
            logEvent.Properties["mcs"] = logName;
            _logger.Warn(logEvent);
            //_logger.Warn(msg, args);
        }

        public void Warn(string msg, Exception err)
        {
            _logger.Warn(err, msg);
        }
        #endregion

        #region Trace
        public void Trace(string msg, string logName, params object[] args)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, msg);
            logEvent.Properties["mcs"] = logName;
            _logger.Trace(logEvent);
            //_logger.Trace(msg, args);
        }

        public void Trace(string msg, Exception err)
        {
            _logger.Trace(err, msg);
        }
        #endregion

        #region Error
        public void Error(string msg, string logName, params object[] args)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, msg);
            logEvent.Properties["mcs"] = logName;
            _logger.Error(logEvent);
            //_logger.Error(msg, args);
        }

        public void Error(string msg, Exception err)
        {
            _logger.Error(err, msg);
        }
        #endregion

        #region Fatal
        public void Fatal(string msg, string logName, params object[] args)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, msg);
            logEvent.Properties["mcs"] = logName;
            _logger.Fatal(logEvent);
            //_logger.Fatal(msg, args);
        }

        public void Fatal(string msg, Exception err)
        {
            _logger.Fatal(err, msg);
        }
        #endregion

        #region Custom

        //public void Process(Models.Log log)
        //{
        //    var level = LogLevel.Info;
        //    if (log.Level == Models.EFLogLevel.Trace)
        //        level = LogLevel.Trace;
        //    else if (log.Level == Models.EFLogLevel.Debug)
        //        level = LogLevel.Debug;
        //    else if (log.Level == Models.EFLogLevel.Info)
        //        level = LogLevel.Info;
        //    else if (log.Level == Models.EFLogLevel.Warn)
        //        level = LogLevel.Warn;
        //    else if (log.Level == Models.EFLogLevel.Error)
        //        level = LogLevel.Error;
        //    else if (log.Level == Models.EFLogLevel.Fatal)
        //        level = LogLevel.Fatal;

        //    var ei = new MyLogEventInfo(level, _logger.Name, log.Message);
        //    ei.TimeStamp = log.Timestamp;
        //    ei.Properties["Action"] = log.Action;
        //    ei.Properties["Amount"] = log.Amount;

        //    _logger.Log(level, ei);
        //}

        #endregion

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public void Flush(int? timeoutMilliseconds = null)
        {
            if (timeoutMilliseconds != null)
                NLog.LogManager.Flush(timeoutMilliseconds.Value);

            NLog.LogManager.Flush();
        }
    }

    public class MyLogEventInfo : LogEventInfo
    {
        public MyLogEventInfo() { }
        public MyLogEventInfo(LogLevel level, string loggerName, string message) : base(level, loggerName, message)
        { }

        public override string ToString()
        {
            //Message format
            //Log Event: Logger='XXX' Level=Info Message='XXX' SequenceID=5
            return FormattedMessage;
        }
    }

}
