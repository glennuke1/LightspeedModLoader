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

        public static bool enableLogging = true;

        internal static void Init()
        {
            ts.Switch.Level = SourceLevels.All;
            ts.Listeners.Add(tw);
            Log("Lightspeed Preloader Log " + DateTime.Now.ToString("u"));
            Log(string.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version));
        }

        public static void Log(string message)
        {
            if (enableLogging)
            {
                tw.WriteLine($"[{DateTime.Now:HH:mm:ss}] - {message}");
                tw.Flush();
                MessageLogged?.Invoke(null, new LogEventArgs { Message = message });
            }
        }

        public static void Print(string message) => Log(message);
        public static void LogWarning(string message) => Warning(message);
        public static void LogError(string message) => Error(message);

        public static void Warning(string message)
        {
            if (enableLogging)
            {
                tw.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Warning] - <color=yellow>{message}</color>");
                tw.Flush();
                MessageLogged?.Invoke(null, new LogEventArgs { Message = message });
            }
        }

        public static void Error(string message)
        {
            if (enableLogging)
            {
                tw.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Warning] - <color=red>{message}</color>");
                tw.Flush();
                MessageLogged?.Invoke(null, new LogEventArgs { Message = message });
            }
        }

        public static void Error(Exception ex)
        {
            if (enableLogging)
            {
                StackTrace trace = new StackTrace(ex, true);

                Log("Exception: " + ex.Message);

                foreach (var frame in trace.GetFrames())
                {
                    var method = frame.GetMethod();
                    string methodName = $"{method.DeclaringType?.FullName}.{method.Name}";
                    int lineNumber = frame.GetFileLineNumber();

                    if (lineNumber > 0)
                    {
                        Log($"  at {methodName} in {frame.GetFileName()}:line {lineNumber}");
                    }
                    else
                    {
                        Log($"  at {methodName}");
                    }
                }
            }
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
