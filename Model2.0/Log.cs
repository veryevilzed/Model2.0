using System;
using SmartFormat;

namespace Novolot.Tools
{

    public class ILog
    {
        public void Debug(string s)
        {
            UnityEngine.Debug.Log(s);
        }
        
        public void Info(string s)
        {
            UnityEngine.Debug.Log(s);
        }
        
        public void Warn(string s)
        {
            UnityEngine.Debug.LogWarning(s);
        }

        public void Error(string s)
        {
            UnityEngine.Debug.LogError(s);
        }

        public void Error(Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        
    }
    
    public class MLog
    {
        private ILog log;
        public void Debug(string message, params object[] args){ log.Debug(Smart.Format(message, args)); }
        public void Info(string message, params object[] args){ log.Info(Smart.Format(message, args)); }
        public void Warn(string message, params object[] args){ log.Warn(Smart.Format(message, args)); }
        public void Error(string message, params object[] args){ log.Error(Smart.Format(message, args)); }

        public void Error(Exception e, string message, params object[] args)
        {
            log.Error(Smart.Format(message,args) +"\n"+string.Format("{0}\n{1}",e.Message, e.StackTrace) );
        }

        public void Error(Exception e)
        {
            log.Error(string.Format("{0}\n{1}",e.Message, e.StackTrace));
        }

        public MLog(Type type)
        {
            log = new ILog();
        }

        public MLog(string name)
        {
            log = new ILog();
        }
    }
    
    
//    public class UnityAppender : AppenderSkeleton
//    {
//        /// <inheritdoc />
//        protected override void Append(LoggingEvent loggingEvent)
//        {
//            var message = RenderLoggingEvent(loggingEvent);
//            if (Level.Compare(loggingEvent.Level, Level.Error) >= 0)
//            {
//                Debug.LogError(message);
//            }
//            else if (Level.Compare(loggingEvent.Level, Level.Warn) >= 0)
//            {
//                Debug.LogWarning(message);
//            }
//            else
//            {
//                Debug.Log(message);
//            }
//        }
//    }
    
//    public static class Log
//    {
//        public static MLogger log = new MLogger("client");
//        public static void Debug(string message, params object[] args){ log.Debug(message, args); }
//        public static void Info(string message, params object[] args){ log.Info(message, args); }
//        public static void Warn(string message, params object[] args){ log.Warn(message, args); }
//        public static void Error(string message, params object[] args){ log.Error(message, args); }
//
//        public static void Error(Exception e, string message, params object[] args)
//        {
//             log.Error(e,message,args);
//        }
//
//        public static void Error(Exception e)
//        {
//            log.Error(e);
//        }
//    }
//
//
//    public class MLogger
//    {
//        private string logerName = "client";
//
//        public static bool ShowDebug = false;
//        public static string LogFormat = "{date:dd.MM HH:mm:ss zzz} [{name}] {message}";
//        
//        public MLogger(string loggerName)
//        {
//            logerName = loggerName;
//        }
//        
//        string getFormat(string message, params object[] args)
//        {
//            var logerObj = new
//            {
//                date = DateTime.Now,
//                name = logerName, 
//                message = Smart.Format(message, args)
//            };
//            
//            return Smart.Format(LogFormat, logerObj);
//        }
//
//        public void Debug(string message, params object[] args)
//        {
//            if (ShowDebug)
//                Info(message, args);
//        }
//
//        public void Info(string message, params object[] args)
//        {
//            
//            UnityEngine.Debug.Log(getFormat(message, args));
//        }
//
//        public void Warn(string message, params object[] args)
//        {
//            UnityEngine.Debug.LogWarning(getFormat(message, args));
//        }
//
//        public void Error(string message, params object[] args)
//        {
//            UnityEngine.Debug.LogError(getFormat(message, args));
//        }
//
//        public void Error(Exception e, string message, params object[] args)
//        {
//            UnityEngine.Debug.LogError(string.Format("{0}\n{1}\n{2}", getFormat(message, args), e.Message, e.StackTrace));
//        }
//
//        public void Error(Exception e)
//        {
//            UnityEngine.Debug.LogError(string.Format("{0}{1}\n{2}", getFormat(""), e.Message, e.StackTrace));
//        }       
//    }
}