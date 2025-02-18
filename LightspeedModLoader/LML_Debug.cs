using System;
using System.Diagnostics;
using System.Reflection;

namespace LightspeedModLoader
{
    public static class LML_Debug
    {
        private static TraceSource ts = new TraceSource("LML");

        private static TextWriterTraceListener tw = new TextWriterTraceListener("LML_Preloader.txt");

        public static event EventHandler<LogEventArgs> MessageLogged;

        internal static void Init()
        {
            ts.Switch.Level = SourceLevels.All;
            ts.Listeners.Add(tw);
            Log("Lightspeed Preloader Log " + DateTime.Now.ToString("u"));
            Log(string.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version));
        }

        public static void Log(string message)
        {
            tw.WriteLine($"[{DateTime.Now:HH:mm:ss}] - {message}");
            tw.Flush();
            var eventArgs = new LogEventArgs();
            eventArgs.Message = message;
            MessageLogged?.Invoke(null, eventArgs);
        }

        public static void Print(string message)
        {
            Log(message);
        }

        public static void Warning(string message)
        {
            tw.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Warning] - <color=yellow>{message}</color>");
            tw.Flush();
            var eventArgs = new LogEventArgs();
            eventArgs.Message = message;
            MessageLogged?.Invoke(null, eventArgs);
        }

        public static void LogWarning(string message)
        {
            Warning(message);
        }

        public static void Error(string message)
        {
            tw.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Warning] - <color=red>{message}</color>");
            tw.Flush();
            var eventArgs = new LogEventArgs();
            eventArgs.Message = message;
            MessageLogged?.Invoke(null, eventArgs);
        }

        public static void LogError(string message)
        {
            Error(message);
        }

        public static void DetectNullFields(object obj)
        {
            if (obj == null)
            {
                Log("Object itself is null!");
                return;
            }

            Type type = obj.GetType();
            Log($"Scanning fields of {type.Name}...");

            foreach (var field in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public))
            {
                object value = field.GetValue(obj);
                if (value == null)
                {
                    Log($"[NULL FIELD] {field.Name} is null in {type.Name}");
                }
            }
        }

    }

    public class LogEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
