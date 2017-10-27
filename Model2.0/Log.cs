using System;
using SmartFormat;

namespace TinyLima.Tools
{
    public static class Log
    {
        public static MLogger log = new MLogger("client");
        public static void Debug(string message, params object[] args){ log.Debug(message, args); }
        public static void Info(string message, params object[] args){ log.Info(message, args); }
        public static void Warn(string message, params object[] args){ log.Warn(message, args); }
        public static void Error(string message, params object[] args){ log.Error(message, args); }

        public static void Error(Exception e, string message, params object[] args)
        {
             log.Error(e,message,args);
        }

        public static void Error(Exception e)
        {
            log.Error(e);
        }
    }


    public class MLogger
    {
        private string logerName = "client";

        public static bool ShowDebug = false;
        public static string LogFormat = "{date:dd.MM HH:mm:ss zzz} [{name}] {message}";
        
        public MLogger(string loggerName)
        {
            logerName = loggerName;
        }
        
        string getFormat(string message, params object[] args)
        {
            var logerObj = new
            {
                date = DateTime.Now,
                name = logerName, 
                message = Smart.Format(message, args)
            };
            
            return Smart.Format(LogFormat, logerObj);
        }

        public void Debug(string message, params object[] args)
        {
            if (ShowDebug)
                Info(message, args);
        }

        public void Info(string message, params object[] args)
        {
            
            UnityEngine.Debug.Log(getFormat(message, args));
        }

        public void Warn(string message, params object[] args)
        {
            UnityEngine.Debug.LogWarning(getFormat(message, args));
        }

        public void Error(string message, params object[] args)
        {
            UnityEngine.Debug.LogError(getFormat(message, args));
        }

        public void Error(Exception e, string message, params object[] args)
        {
            UnityEngine.Debug.LogError(string.Format("{0}\n{1}\n{2}", getFormat(message, args), e.Message, e.StackTrace));
        }

        public void Error(Exception e)
        {
            UnityEngine.Debug.LogError(string.Format("{0}{1}\n{2}", getFormat(""), e.Message, e.StackTrace));
        }       
    }
}