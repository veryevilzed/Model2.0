using System;

namespace TinyLima.Tools
{
    public delegate void DLog(string message);
    
    /// <summary>
    /// Коллбэк для логов
    /// </summary>
    public static class LogCallback
    {
        public static DLog OnError;
        public static DLog OnWarn;
        public static DLog OnInfo;
        public static DLog OnDebug;

        public static void Debug(string message, params object[] args)
        {
            OnDebug?.Invoke(string.Format(message, args));
        }

        public static void Info(string message, params object[] args)
        {
            OnInfo?.Invoke(string.Format(message, args));
        }
        
        public static void Warn(string message, params object[] args)
        {
            OnWarn?.Invoke(string.Format(message, args));
        }

        public static void Error(string message, params object[] args)
        {
            OnError?.Invoke(string.Format(message, args));
        }

        public static void Error(Exception e)
        {
            OnError?.Invoke(string.Format("{0}\n{1}", e.Message, e.StackTrace));
        }

        public static void Error(Exception e, string message, params object[] args)
        {
            OnError?.Invoke(string.Format("{0}\n{1}\n{2}", string.Format(message, args), e.Message, e.StackTrace));
        }
    }
}