using Ionic.Zip;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace LML_AutoUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args[0] == "--mscpath=")
            {
                mscpath = args[0].Split('=')[1];
            }

            if (!IsUpdated())
            {
                if (Process.GetProcessesByName("mysummercar")[0] != null)
                {
                    Process.GetProcessesByName("mysummercar")[0].Kill();
                }

                InstallFiles();

                Process.Start(mscpath + "mysummercar.exe");
            }
        }

        static string mscpath;

        private static bool IsUpdated()
        {
            if (!File.Exists(mscpath + "LML_VERSION"))
            {
                return false;
            }

            string onlineVersion = "";
            using (var wc = new WebClient())
            {
                onlineVersion = wc.DownloadString("https://raw.githubusercontent.com/glennuke1/LightspeedModLoader/refs/heads/master/LightspeedModLoader/Builds/VERSION");
            }

            if (onlineVersion == File.ReadAllText(mscpath + "LML_VERSION"))
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
                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/LightspeedModLoader/Builds/doorstop.zip", mscpath + "doorstop.zip");
                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/LightspeedModLoader/Builds/References.zip", mscpath + "References.zip");
                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/LightspeedModLoader/Builds/NeededDLLS.zip", mscpath + "NeededDLLS.zip");
                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/LightspeedModLoader/Builds/VERSION", mscpath + "LML_VERSION");
            }

            ZipFile zip = ZipFile.Read("doorstop.zip");
            zip.ExtractAll(mscpath, ExtractExistingFileAction.OverwriteSilently);

            ZipFile zip1 = ZipFile.Read("References.zip");
            zip1.ExtractAll(Path.Combine(mscpath, "mysummercar_Data/Managed"), ExtractExistingFileAction.OverwriteSilently);

            ZipFile zip2 = ZipFile.Read("NeededDLLS.zip");
            zip2.ExtractAll(Path.Combine(mscpath, "mysummercar_Data/Managed"), ExtractExistingFileAction.OverwriteSilently);

            using (var client = new WebClient())
            {
                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/LightspeedModLoader/Builds/LightspeedModLoader.dll", mscpath + "/mysummercar_Data/Managed/LightspeedModLoader.dll");
                client.DownloadFile("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/LightspeedModLoader/Builds/MSCLoader.dll", mscpath + "/mysummercar_Data/Managed/MSCLoader.dll");
            }
        }
    }
}
