using MSCLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace LightspeedModLoader
{
    public class ModLoader : MonoBehaviour
    {
        public static ModLoader Instance;

        public static string ModsFolder;

        internal static string AssetsFolder;
        internal static string ConfigFolder;

        internal static bool loaderPrepared = false;

        internal bool allModsLoaded;

        public static bool PreLoadPhaseComplete;
        public static bool OnLoadPhaseComplete;
        public static bool PostLoadPhaseComplete;

        public List<Mod> loadedMods = new List<Mod>();

        internal List<Mod> A_OnNewGameMods = new List<Mod>();
        internal List<Mod> A_OnMenuLoadMods = new List<Mod>();
        internal List<Mod> A_PreLoadMods = new List<Mod>();
        internal List<Mod> A_OnLoadMods = new List<Mod>();
        internal List<Mod> A_PostLoadMods = new List<Mod>();
        internal List<Mod> A_UpdateMods = new List<Mod>();
        internal List<Mod> A_FixedUpdateMods = new List<Mod>();
        internal List<Mod> A_OnGUIMods = new List<Mod>();
        internal List<Mod> A_OnSaveMods = new List<Mod>();
        internal List<Mod> A_OnModSettingsMods = new List<Mod>();

        public MSCLoaderModsLoader mscloadermodsloader;
        public SaveLoad saveLoad;

        public static bool Profiling;
        public static bool LogNullReferenceExceptions;

        internal static Profiler profiler;

        internal bool firstTimeMainMenuLoad = true;

        public static void PreInit()
        {
            ModsFolder = Path.GetFullPath("mods");

            if (!Directory.Exists(ModsFolder))
            {
                Directory.CreateDirectory(ModsFolder);
            }

            AssetsFolder = Path.Combine(ModsFolder, "Assets");
            ConfigFolder = Path.Combine(ModsFolder, "Config");

            if (!Directory.Exists(AssetsFolder))
            {
                Directory.CreateDirectory(AssetsFolder);
            }

            if (!Directory.Exists(ConfigFolder))
            {
                Directory.CreateDirectory(ConfigFolder);
            }

            try
            {
                Process.Start("/mysummercar_Data/Managed/LML_AutoUpdater.exe", "--mscpath=" + Directory.GetCurrentDirectory());
            }
            catch (Exception ex)
            {
                LML_Debug.Log(ex.Message);
            }
            Prepare();
        }

        private static void Prepare()
        {
            if (!loaderPrepared)
            {
                loaderPrepared = true;
                GameObject gameObject = new GameObject("LML", new Type[]
                {
                    typeof(ModLoader)
                });
                Instance = gameObject.GetComponent<ModLoader>();
                Instance.mscloadermodsloader = gameObject.AddComponent<MSCLoaderModsLoader>();
                Instance.saveLoad = gameObject.AddComponent<SaveLoad>();
                DontDestroyOnLoad(gameObject);

                if (Environment.GetCommandLineArgs().Contains("-LML-Profile"))
                {
                    profiler = new Profiler();
                    Profiling = true;
                    LML_Debug.Log("Internal LML Profiler enabled");
                }

                if (Environment.GetCommandLineArgs().Contains("-LML-LogNullReferenceExceptions"))
                {
                    LogNullReferenceExceptions = true;
                }

                Instance.PreLoadMods();
            }
        }

        private void PreLoadMods()
        {
            if (Profiling)
                profiler.Start("Load DLLs");

            string[] files = Directory.GetFiles(ModLoader.ModsFolder);

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].EndsWith(".dll"))
                {
                    LoadDLL(files[i]);
                }
            }

            if (Profiling)
                profiler.Stop("Load DLLs");

            LML_Debug.Log("Mod DLLs loaded");

            if (Profiling)
                profiler.Start("Load Mod Actions");

            LoadModsActions();
            mscloadermodsloader.LoadModsActions();

            if (Profiling)
                profiler.Stop("Load Mod Actions");

            LML_Debug.Log("Mods actions/methods loaded");

            saveLoad.Load();
        }

        private void LoadDLL(string file)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(file);

                Type[] types = assembly.GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    if (types[j].IsSubclassOf(typeof(Mod)))
                    {
                        Mod mod = (Mod)Activator.CreateInstance(types[j]);
                        if (string.IsNullOrEmpty(mod.ID.Trim()))
                        {
                            LML_Debug.Log("Empty mod ID");
                        }
                        else
                        {
                            LoadMod(mod);
                            return;
                        }
                    }

                    if (types[j].IsSubclassOf(typeof(MSCLoader.Mod)))
                    {
                        MSCLoader.Mod mod = (MSCLoader.Mod)Activator.CreateInstance(types[j]);
                        if (string.IsNullOrEmpty(mod.ID.Trim()))
                        {
                            LML_Debug.Log("Empty mod ID");
                        }
                        else
                        {
                            LML_Debug.Log("Attempting to load MSCLoader mod: " + mod.ID);
                            mscloadermodsloader.LoadMod(mod);
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in <b>{2}</b>", Environment.NewLine, e.Message, new StackTrace(e, true).GetFrame(0).GetMethod()));
            }
        }

        private void LoadMod(Mod mod)
        {
            try
            {
                if (!loadedMods.Contains(mod))
                {
                    if (Profiling)
                        profiler.Start(mod.ID + " ModSetup");

                    mod.ModSetup();

                    if (Profiling)
                        profiler.Stop(mod.ID + " ModSetup");

                    loadedMods.Add(mod);
                }
            }
            catch (Exception ex)
            {
                LML_Debug.Log(ex.Message);
            }
        }

        private void LoadModsActions()
        {
            foreach (Mod mod in loadedMods)
            {
                if (mod.A_OnNewGame != null)
                {
                    A_OnNewGameMods.Add(mod);
                }

                if (mod.A_OnMenuLoad != null)
                {
                    A_OnMenuLoadMods.Add(mod);
                }

                if (mod.A_PreLoad != null)
                {
                    A_PreLoadMods.Add(mod);
                }

                if (mod.A_OnLoad != null)
                {
                    A_OnLoadMods.Add(mod);
                }

                if (mod.A_PostLoad != null)
                {
                    A_PostLoadMods.Add(mod);
                }

                if (mod.A_Update != null)
                {
                    A_UpdateMods.Add(mod);
                }

                if (mod.A_FixedUpdate != null)
                {
                    A_FixedUpdateMods.Add(mod);
                }

                if (mod.A_OnSave != null)
                {
                    A_OnSaveMods.Add(mod);
                }

                if (mod.A_OnGUI != null)
                {
                    A_OnGUIMods.Add(mod);
                }

                if (mod.A_ModSettings != null)
                {
                    A_OnModSettingsMods.Add(mod);
                }
            }
        }

        internal void OnGUI()
        {
            foreach (Mod mod in A_OnGUIMods)
            {
                if (!mod.isDisabled)
                {
                    try
                    {
                        if (allModsLoaded || mod.LoadInMenu)
                        {
                            mod.A_OnGUI();
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                    }
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.A_OnGUIMods)
            {
                LML_Debug.Log("running gui on: " + mod.ID);
                if (!mod.isDisabled)
                {
                    try
                    {
                        if (allModsLoaded || mod.LoadInMenu)
                        {
                            mod.A_OnGUI();
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                    }
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
            {
                if (!mod.isDisabled)
                {
                    try
                    {
                        if (allModsLoaded || mod.LoadInMenu)
                        {
                            mod.OnGUI();
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                    }
                }
            }
        }

        internal void Update()
        {
            foreach (Mod mod in A_UpdateMods)
            {
                if (!mod.isDisabled)
                {
                    try
                    {
                        if (allModsLoaded || mod.LoadInMenu)
                        {
                            mod.A_Update();
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                    }
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.A_UpdateMods)
            {
                if (!mod.isDisabled)
                {
                    try
                    {
                        if (allModsLoaded || mod.LoadInMenu)
                        {
                            mod.A_Update();
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                    }
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
            {
                if (!mod.isDisabled)
                {
                    try
                    {
                        if (allModsLoaded || mod.LoadInMenu)
                        {
                            mod.Update();
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                    }
                }
            }
        }

        internal void FixedUpdate()
        {
            foreach (Mod mod in A_FixedUpdateMods)
            {
                if (!mod.isDisabled)
                {
                    try
                    {
                        if (allModsLoaded || mod.LoadInMenu)
                        {
                            mod.A_FixedUpdate();
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                    }
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.A_FixedUpdateMods)
            {
                if (!mod.isDisabled)
                {
                    try
                    {
                        if (allModsLoaded || mod.LoadInMenu)
                        {
                            mod.A_FixedUpdate();
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                    }
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
            {
                if (!mod.isDisabled)
                {
                    try
                    {
                        if (allModsLoaded || mod.LoadInMenu)
                        {
                            mod.FixedUpdate();
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                    }
                }
            }
        }

        internal void OnLevelWasLoaded(int level)
        {
            string loadedLevelName = Application.loadedLevelName;
            if (loadedLevelName == "MainMenu")
            {
                GameObject.Find("Quit").SetActive(false);
                GameObject.Find("Interface/Buttons/ButtonQuit").GetComponent<PlayMakerFSM>().FsmStates[0].RemoveAction(2);
                allModsLoaded = false;

                if (firstTimeMainMenuLoad)
                {
                    LoadReferences();
                    AssetBundle ab = LoadAssets.LoadBundle("LightspeedModLoader.Assets.lml.unity3d");
                    Text vLabel = Instantiate(ab.LoadAsset<GameObject>("Info")).transform.Find("Version Label").GetComponent<Text>();
                    vLabel.text = "Lightspeed Mod Loader\n" + (File.Exists("LML_VERSION") ? File.ReadAllText("LML_VERSION") : "Unknown???");
                    ab.Unload(true);
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }

                foreach (Mod mod in A_OnMenuLoadMods)
                {
                    try
                    {
                        if (!mod.isDisabled)
                        {
                            if (Profiling)
                                profiler.Start(mod.ID + " OnMenuLoad");

                            mod.A_OnMenuLoad();

                            if (Profiling)
                                profiler.Stop(mod.ID + " OnMenuLoad");
                        }
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }

                foreach (MSCLoader.Mod mod in mscloadermodsloader.A_OnMenuLoadMods)
                {
                    try
                    {
                        if (!mod.isDisabled)
                        {
                            mod.A_OnMenuLoad();
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                    }
                }

                foreach (Mod mod in A_OnModSettingsMods)
                {
                    try
                    {
                        if (!mod.isDisabled)
                        {
                            if (firstTimeMainMenuLoad)
                            {
                                mod.A_ModSettings();
                            }
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                    }
                }

                foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
                {
                    try
                    {
                        if (!mod.isDisabled)
                        {
                            if (firstTimeMainMenuLoad)
                            {
                                if (mod.A_ModSettings != null)
                                {
                                    LML_Debug.Log("Attempting to load mod settings for " + mod.ID);
                                    mod.A_ModSettings();
                                }
                                else if (CheckEmptyMethod(mod, "ModSettings"))
                                {
                                    LML_Debug.Log("Attempting to load deprecated mod settings for " + mod.ID);
                                    mod.ModSettings();
                                }
                            }
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                    }
                }

                if (firstTimeMainMenuLoad)
                {
                    MSCLoader.ModLoader.LoadModsSettings();
                    firstTimeMainMenuLoad = false;
                }
            }

            if (loadedLevelName == "GAME")
            {
                LML_Debug.Log("\nGAME Level loaded. Running mods load methods\n");
                StartCoroutine(LoadModsAsync());
            }

            if (loadedLevelName == "Intro")
            {
                foreach (Mod mod in A_OnNewGameMods)
                {
                    mod.A_OnNewGame();
                }

                foreach (MSCLoader.Mod mod in mscloadermodsloader.A_OnNewGameMods)
                {
                    mod.A_OnNewGame();
                }

                foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
                {
                    mod.OnNewGame();
                }
            }
        }

        internal IEnumerator LoadModsAsync()
        {
            foreach (Mod mod in A_PreLoadMods)
            {
                yield return null;
                try
                {
                    if (!mod.isDisabled)
                    {
                        if (Profiling)
                        {
                            profiler.Start(mod.ID + " PreLoad");
                        }

                        mod.A_PreLoad();

                        if (Profiling)
                            profiler.Stop(mod.ID + " PreLoad");
                    }
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                }
                catch (Exception e)
                {
                    LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.A_PreLoadMods)
            {
                yield return null;
                try
                {
                    if (!mod.isDisabled)
                        mod.A_PreLoad();
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                }
                catch (Exception e)
                {
                    LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
            {
                yield return null;
                try
                {
                    if (!mod.isDisabled)
                        mod.PreLoad();
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                }
                catch (Exception e)
                {
                    LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                }
            }

            LML_Debug.Log("PreLoad Phase Complete");

            LML_Debug.Log("Waiting for game to finish loading");

            while (GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera") == null)
            {
                yield return null;
            }

            foreach (Mod mod in A_OnLoadMods)
            {
                yield return null;
                try
                {
                    if (!mod.isDisabled)
                    {
                        if (Profiling)
                            profiler.Start(mod.ID + " OnLoad");

                        mod.A_OnLoad();

                        if (Profiling)
                            profiler.Stop(mod.ID + " OnLoad");
                    }
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                }
                catch (Exception e)
                {
                    LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.A_OnLoadMods)
            {
                yield return null;
                try
                {
                    if (!mod.isDisabled)
                        mod.A_OnLoad();
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                }
                catch (Exception e)
                {
                    LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
            {
                yield return null;
                try
                {
                    if (!mod.isDisabled)
                        mod.OnLoad();
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                }
                catch (Exception e)
                {
                    LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                }
            }

            LML_Debug.Log("OnLoad Phase Complete");

            foreach (Mod mod in A_PostLoadMods)
            {
                yield return null;
                try
                {
                    if (!mod.isDisabled)
                    {
                        if (Profiling)
                            profiler.Start(mod.ID + " PostLoad");

                        mod.A_PostLoad();

                        if (Profiling)
                            profiler.Stop(mod.ID + " PreLoad");
                    }
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                }
                catch (Exception e)
                {
                    LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.A_PostLoadMods)
            {
                yield return null;
                try
                {
                    if (!mod.isDisabled)
                        mod.A_PostLoad();
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                }
                catch (Exception e)
                {
                    LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
            {
                yield return null;
                try
                {
                    if (!mod.isDisabled)
                        mod.SecondPassOnLoad();
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                }
                catch (Exception e)
                {
                    LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                }
            }

            LML_Debug.Log("PostLoad Phase Complete");

            GameObject.Find("ITEMS").FsmInject("Save game", new Action(this.SaveMods));

            allModsLoaded = true;

            gameObject.AddComponent<GameOptimizations>();
        }

        internal void SaveMods()
        {
            foreach (Mod mod in A_OnSaveMods)
            {
                try
                {
                    if (!mod.isDisabled)
                    {
                        mod.A_OnSave();
                    }
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                }
                catch (Exception e)
                {
                    LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.A_OnSaveMods)
            {
                try
                {
                    if (!mod.isDisabled)
                    {
                        mod.A_OnSave();
                    }
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                }
                catch (Exception e)
                {
                    LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
            {
                try
                {
                    if (!mod.isDisabled)
                    {
                        mod.OnSave();
                    }
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in method <b>{3}</b> in object <b>{4}</b>. StackTrace: <b>{5}</b>", Environment.NewLine, e.Message, mod.ID, e.TargetSite, e.Source, e.StackTrace));
                }
                catch (Exception e)
                {
                    LML_Debug.Log(string.Format("{0}<b>Details: </b>{1} in Mod <b>{2}</b> in <b>{3}</b>", Environment.NewLine, e.Message, mod.ID, new StackTrace(e, true).GetFrame(0).GetMethod()));
                }
            }
        }

        public static object GetMod(string modID)
        {
            foreach (Mod mod in Instance.loadedMods)
            {
                if (mod.ID.ToLower() == modID.ToLower())
                {
                    return mod;
                }
            }

            foreach (MSCLoader.Mod mod in Instance.mscloadermodsloader.loadedMods)
            {
                if (mod.ID.ToLower() == modID.ToLower())
                {
                    return mod;
                }
            }

            return null;
        }

        public static bool IsModPresent(string modID)
        {
            return GetMod(modID) != null;
        }

        public static string GetModAssetsFolder(Mod mod)
        {
            if (!Directory.Exists(Path.Combine(ModLoader.AssetsFolder, mod.ID)))
            {
                Directory.CreateDirectory(Path.Combine(ModLoader.AssetsFolder, mod.ID));
            }
            return Path.Combine(ModLoader.AssetsFolder, mod.ID);
        }

        public static string GetModAssetsFolder(MSCLoader.Mod mod)
        {
            if (!Directory.Exists(Path.Combine(ModLoader.AssetsFolder, mod.ID)))
            {
                Directory.CreateDirectory(Path.Combine(ModLoader.AssetsFolder, mod.ID));
            }
            return Path.Combine(ModLoader.AssetsFolder, mod.ID);
        }

        public static string GetModConfigFolder(Mod mod)
        {
            if (!Directory.Exists(Path.Combine(ModLoader.ConfigFolder, mod.ID)))
            {
                Directory.CreateDirectory(Path.Combine(ModLoader.ConfigFolder, mod.ID));
            }
            return Path.Combine(ModLoader.ConfigFolder, mod.ID);
        }

        internal List<string> references = new List<string>();

        internal void LoadReferences()
        {
            if (Directory.Exists(Path.Combine(ModsFolder, "References")))
            {
                string[] files = Directory.GetFiles(Path.Combine(ModsFolder, "References"), "*.dll");

                foreach (string dll in files)
                {
                    Assembly asm = Assembly.LoadFrom(dll);

                    references.Add(asm.GetName().Name);
                }
            }
        }

        public static bool IsReferencePresent(string assemblyID)
        {
            return Instance.references.Contains(assemblyID);
        }

        public static string GetModConfigFolder(MSCLoader.Mod mod)
        {
            if (!Directory.Exists(Path.Combine(ModLoader.ConfigFolder, mod.ID)))
            {
                Directory.CreateDirectory(Path.Combine(ModLoader.ConfigFolder, mod.ID));
            }
            return Path.Combine(ModLoader.ConfigFolder, mod.ID);
        }

        internal static bool CheckEmptyMethod(MSCLoader.Mod mod, string methodName)
        {
            MethodInfo method = mod.GetType().GetMethod(methodName);
            return method.IsVirtual && method.DeclaringType == mod.GetType() && method.GetMethodBody().GetILAsByteArray().Length > 2;
        }

        List<MonoBehaviour> monoBehavioursToProfile = new List<MonoBehaviour>();

        Dictionary<MonoBehaviour, List<float>> monoBehavioursProfiled = new Dictionary<MonoBehaviour, List<float>>();

        public static void DeepProfileAllMods()
        {
            Instance.StartCoroutine(Instance.deepProfileCoroutine());

            foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
            {
                foreach (MonoBehaviour script in obj.GetComponents<MonoBehaviour>())
                {
                    Type scriptType = script.GetType();

                    if (Instance.IsModdedClass(scriptType))
                    {
                        Instance.monoBehavioursToProfile.Add(script);

                        if (!Instance.monoBehavioursProfiled.ContainsKey(script))
                        {
                            Instance.monoBehavioursProfiled[script] = new List<float>();
                        }
                    }
                }
            }
        }

        internal IEnumerator deepProfileCoroutine()
        {
            LML_Debug.Log("Warning! Deep profiling all mods could cause your game to become unstable or freeze.");
            LML_Debug.Log("Deep profiling will start in 5 seconds...");
            yield return new WaitForSeconds(5f);

            Stopwatch stopwatch = new Stopwatch();

            float endTime = Time.time + 10f;

            while (Time.time < endTime)
            {
                foreach (MonoBehaviour script in monoBehavioursToProfile)
                {
                    yield return null;
                    Type scriptType = script.GetType();

                    if (IsModdedClass(scriptType))
                    {
                        MethodInfo updateMethod = scriptType.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (updateMethod != null)
                        {
                            stopwatch.Start();
                            try
                            {
                                updateMethod.Invoke(script, null);
                            }
                            catch { }
                            stopwatch.Stop();
                            monoBehavioursProfiled[script].Add((float)stopwatch.Elapsed.TotalMilliseconds);
                            stopwatch.Reset();
                        }
                    }
                }

                yield return null;
            }

            LML_Debug.Log("Deep profiling completed.");

            foreach (KeyValuePair<MonoBehaviour, List<float>> entry in monoBehavioursProfiled)
            {
                float totalTime = 0f;
                float highestTime = 0f;

                if (entry.Value.Count == 0)
                {
                    continue;
                }

                foreach (float f in entry.Value)
                {
                    totalTime += f;
                    if (f > highestTime) highestTime = f;
                }

                float averageTime = totalTime / entry.Value.Count;

                LML_Debug.Log($"{entry.Key.GetType()} Update method took avg: {averageTime:F3} ms, highest: {highestTime:F3} ms");
            }

        }

        private bool IsModdedClass(Type type)
        {
            Type[] types = type.Assembly.GetTypes();
            for (int j = 0; j < types.Length; j++)
            {
                if (types[j].IsSubclassOf(typeof(Mod)))
                {
                    return true;
                }

                if (types[j].IsSubclassOf(typeof(MSCLoader.Mod)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
