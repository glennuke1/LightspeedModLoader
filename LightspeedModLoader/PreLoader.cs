using Harmony;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LightspeedModLoader.PreLoader
{
    public static class PreLoader
    {
        static bool introSkipped;

        public static void Main()
        {
            if (File.Exists("LML_Preloader.txt"))
            {
                File.Delete("LML_Preloader.txt");
            }

            AppDomain.CurrentDomain.AssemblyLoad += AssemblyWatcher;
        }

        internal static void SkipIntro()
        {
            if (!introSkipped)
            {
                introSkipped = true;
                Application.LoadLevel("MainMenu");
            }
        }

        private static bool IsUpdated()
        {
            string onlineVersion = "";
            using (var wc = new System.Net.WebClient())
            {
                onlineVersion = wc.DownloadString("https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/LightspeedModLoader/Builds/VERSION");
            }

            if (onlineVersion == File.ReadAllText("LML_VERSION"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void InjectModLoader()
        {
            if (!IsUpdated())
            {
                Process.Start("LML_AutoUpdater.exe", "--mscpath=" + Path.GetDirectoryName(Path.GetFullPath("mysummercar.exe")));
            }

            try
            {
                LML_Debug.Log("Patching LML Methods");
                HarmonyInstance.Create("LML.Main").PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                LML_Debug.Log("Error occured while patching LML methods.");
                LML_Debug.Log(ex.Message + " " + ex.StackTrace);
            }
        }

        private static void AssemblyWatcher(object sender, AssemblyLoadEventArgs args)
        {
            if (args.LoadedAssembly.GetName().Name == "System")
            {
                AppDomain.CurrentDomain.AssemblyLoad -= AssemblyWatcher;
                InjectModLoader();
            }
        }
    }

    [HarmonyPatch(typeof(PlayMakerFSM))]
    [HarmonyPatch("Awake")]
    public class InjectIntroSkip
    {
        // Token: 0x06000013 RID: 19 RVA: 0x000028EB File Offset: 0x00000AEB
        private static void Prefix()
        {
            PreLoader.SkipIntro();
        }
    }

    [HarmonyPatch(typeof(PlayMakerArrayListProxy))]
    [HarmonyPatch("Awake")]
    public class InjectLML
    {
        // Token: 0x06000011 RID: 17 RVA: 0x000028D7 File Offset: 0x00000AD7
        private static void Prefix()
        {
            ModLoader.PreInit();
        }
    }

    [HarmonyPatch(typeof(HutongGames.PlayMaker.Actions.MousePickEvent))]
    [HarmonyPatch("DoMousePickEvent")]
    class InjectClickthroughFix
    {
        static bool Prefix()
        {
            if (UnityEngine.GUIUtility.hotControl != 0)
            {
                return false;
            }
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    return false;
                }
            }
            return true;

        }
    }
}