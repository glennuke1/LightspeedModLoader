using Ionic.Zip;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace LML_AutoUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("--mscpath="))
                {
                    mscpath = arg.Substring(10).Trim('"');
                }
            }

            if (!IsUpdated())
            {
                if (Process.GetProcessesByName("mysummercar")[0] != null)
                {
                    Process.GetProcessesByName("mysummercar")[0].Kill();
                }

                InstallFiles();

                Process.Start(mscpath + "/mysummercar.exe");
            }

            Environment.Exit(0);
        }

        static string mscpath;

        private static bool IsUpdated()
        {
            if (!File.Exists(mscpath + "/LML_VERSION"))
            {
                return false;
            }

            string onlineVersion = "";
            using (var wc = new WebClient())
            {
                onlineVersion = wc.DownloadString("https://raw.githubusercontent.com/glennuke1/LightspeedModLoader/refs/heads/master/LightspeedModLoader/Builds/VERSION");
            }

            string localVersion = File.ReadAllText(mscpath + "/LML_VERSION");

            if (float.Parse(localVersion.Split(' ')[1]) >= float.Parse(onlineVersion.Split(' ')[1]))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static void InstallFiles()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/LightspeedModLoader/Builds/doorstop.zip", mscpath + "/doorstop.zip");
                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/LightspeedModLoader/Builds/NeededDLLS.zip", mscpath + "/NeededDLLS.zip");
                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/LightspeedModLoader/Builds/VERSION", mscpath + "/LML_VERSION");
            }

            ZipFile zip = ZipFile.Read("doorstop.zip");
            zip.ExtractAll(mscpath, ExtractExistingFileAction.OverwriteSilently);
            zip.Dispose();

            ZipFile zip2 = ZipFile.Read("NeededDLLS.zip");
            zip2.ExtractAll(Path.Combine(mscpath, "mysummercar_Data/Managed"), ExtractExistingFileAction.DoNotOverwrite);
            zip2.Dispose();

            using (var client = new WebClient())
            {
                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/LightspeedModLoader/Builds/LightspeedModLoader.dll", mscpath + "/mysummercar_Data/Managed/LightspeedModLoader.dll");
                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/LightspeedModLoader/Builds/MSCLoader.dll", mscpath + "/mysummercar_Data/Managed/MSCLoader.dll");

                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/Official%20Mods/LML_Default_Console.dll", mscpath + "/mods/LML_Default_Console.dll");
                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/Official%20Mods/LML_Default_ModSettings.dll", mscpath + "/mods/LML_Default_ModSettings.dll");
                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/Official%20Mods/LML_DevToolset.dll", mscpath + "/mods/LML_DevToolset.dll");
            }
        }
    }
}
