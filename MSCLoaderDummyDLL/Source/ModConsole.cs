#if !Mini
using MSCLoader.Commands;
using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using LightspeedModLoader;

namespace MSCLoader;

/// <summary>
/// MSCLoader console related functions.
/// </summary>
public class ModConsole
{
    internal static bool IsOpen;
    internal static ConsoleView console;
    internal static SettingsCheckBox typing;
    internal static SettingsSliderInt ConsoleFontSize;
    internal static SettingsText versionText, lastCheckText;

    /// <summary>
    /// Print a message to console.
    /// </summary>
    /// <param name="str">Text to print to console.</param>
    public static void Print(string str)
    {
        if (console != null)
            console.controller.AppendLogLine(str);
        LML_Debug.Log($"{Regex.Replace(str, "<.*?>", string.Empty)}");
    }
    /// <summary>
    /// Prints anything to console.
    /// </summary>
    /// <param name="obj">Text or object to print to console.</param>
    public static void Print(object obj)
    {
        LML_Debug.Log($"{obj}");
    }

    /// <summary>
    /// Print an error to the console.
    /// </summary>
    /// <param name="str">Text to print to error log.</param>
    public static void Error(string str)
    {
        LML_Debug.Log($"ERROR: {Regex.Replace(str, "<.*?>", string.Empty)}");
    }

    /// <summary>
    /// Print an warning to the console.
    /// </summary>
    /// <param name="str">Text to print to error log.</param>
    public static void Warning(string str)
    {
        LML_Debug.Log($"WARNING: {Regex.Replace(str, "<.*?>", string.Empty)}");
    }

    //compatibility layer with pro

    /// <summary>
    /// Same as ModConsole.Print(string);
    /// </summary>
    /// <param name="text">Text to print to console.</param>
    public static void Log(string text) => Print(text);

    /// <summary>
    /// Same as ModConsole.Print(obj);
    /// </summary>
    /// <param name="obj">object to print to console.</param>
    public static void Log(object obj) => Print(obj);

    /// <summary>
    /// Same as ModConsole.Error(string);
    /// </summary>
    /// <param name="text">Error to print to console.</param>
    public static void LogError(string text) => Error(text);

    /// <summary>
    /// Same as ModConsole.Warning(string);
    /// </summary>
    /// <param name="text">Warning to print to console.</param>
    public static void LogWarning(string text) => Warning(text);

    /// <summary>
    /// Logs a list (and optionally its elements) to the ModConsole and output_log.txt
    /// </summary>
    /// <param name="list">List to print.</param>
    /// <param name="printAllElements">(Optional) Should it log all elements of the list/array or should it only log the list/array itself. (default: true)</param>
    public static void Log(IList list, bool printAllElements = true)
    {
        // Check if it should print the elements or the list itself.
        if (printAllElements)
        {
            Log(list.ToString());
            for (int i = 0; i < list.Count; i++) Log(list[i]);
        }
        else Log(list.ToString());
    }
}

#endif