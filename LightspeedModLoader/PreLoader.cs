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

        private static void InjectModLoader()
        {
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