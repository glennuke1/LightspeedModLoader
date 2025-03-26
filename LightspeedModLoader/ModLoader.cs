﻿using MSCLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public static bool LogNullReferenceExceptions = true;

        public int batchSize = 5;

        internal Slider modFinishedSlider;

        internal static Profiler profiler;

        public bool useAsyncUpdate = false;

        internal bool firstTimeMainMenuLoad = true;

        internal static bool firstTimePreInitDone = true;

        public static void PreInit()
        {
            if (firstTimePreInitDone)
            {
                ModsFolder = Path.GetFullPath("mods");

                LML_Debug.Log("Mods Folder: " + ModsFolder);

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

                LML_Debug.Log("Done checking directories");

                try
                {
                    /*if (File.Exists(Path.GetFullPath(Directory.GetCurrentDirectory() + "/mysummercar_Data/Managed/LML_AutoUpdater.exe")))
                    {
                        Process.Start(Path.GetFullPath(Directory.GetCurrentDirectory() + "/mysummercar_Data/Managed/LML_AutoUpdater.exe"), "--mscpath=\"" + Directory.GetCurrentDirectory() + "\"");
                    }*/
                }
                catch (Exception ex)
                {
                    LML_Debug.Error(ex);
                }
                LML_Debug.Log("Starting prepare");
                firstTimePreInitDone = false;
                Prepare();
            }
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

                if (Environment.GetCommandLineArgs().Contains("-LML-DisableLogNullReferenceExceptions"))
                {
                    LogNullReferenceExceptions = false;
                }

                if (Environment.GetCommandLineArgs().Contains("-LML-DisableLogging"))
                {
                    LML_Debug.enableLogging = false;
                }

                LML_Debug.Init();

                gameObject.AddComponent<UnityMainThreadDispatcher>();

                LML_Debug.Log("Preparing done");
                LML_Debug.Log("Starting PreLoadMods");

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
                    try
                    {
                        LoadDLL(files[i]);
                    }
                    catch (Exception ex)
                    {
                        LML_Debug.Error(ex);
                        continue;
                    }
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

            if (useAsyncUpdate)
            {
                StartCoroutine(UpdateAsync());
            }
        }

        private void LoadDLL(string file)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(file);
            }
            catch (Exception ex)
            {
                LML_Debug.Error(ex);
                return;
            }
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
                LML_Debug.Error(ex);
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
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.A_OnGUIMods)
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
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
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
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }
            }
        }

        internal void Update()
        {
            if (!useAsyncUpdate)
            {
                foreach (Mod mod in A_UpdateMods)
                {
                    if (mod.isDisabled)
                    {
                        continue;
                    }

                    try
                    {
                        if (allModsLoaded || mod.LoadInMenu)
                        {
                            if (Profiling)
                                profiler.Start(mod.ID + "Update");

                            mod.A_Update();

                            if (Profiling)
                                profiler.Stop(mod.ID + "Update");
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }

                foreach (MSCLoader.Mod mod in mscloadermodsloader.A_UpdateMods)
                {
                    if (mod.isDisabled)
                    {
                        continue;
                    }

                    try
                    {
                        if (allModsLoaded || mod.LoadInMenu)
                        {
                            if (Profiling)
                                profiler.Start(mod.ID + "Update");

                            mod.A_Update();

                            if (Profiling)
                                profiler.Stop(mod.ID + "Update");
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }

                foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
                {
                    if (!mod.isDisabled)
                    {
                        continue;
                    }

                    try
                    {
                        if (allModsLoaded || mod.LoadInMenu)
                        {
                            if (Profiling)
                                profiler.Start(mod.ID + "Update");

                            mod.Update();

                            if (Profiling)
                                profiler.Stop(mod.ID + "Update");
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }
            }
        }

        internal IEnumerator UpdateAsync()
        {
            int counter = 0;

            while (true)
            {
                foreach (Mod mod in A_UpdateMods)
                {
                    if (counter++ >= 5)
                    {
                        yield return null;
                        counter = 0;
                    }
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
                    if (counter++ >= 5)
                    {
                        yield return null;
                        counter = 0;
                    }
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
                    if (counter++ >= 5)
                    {
                        yield return null;
                        counter = 0;
                    }
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
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
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
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
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
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }
            }
        }

        Text progressText;

        internal void OnLevelWasLoaded(int level)
        {
            string loadedLevelName = Application.loadedLevelName;
            if (loadedLevelName == "MainMenu")
            {
                MSCLoader.ModLoader.CurrentScene = MSCLoader.CurrentScene.MainMenu;
                GameObject.Find("Quit").SetActive(false);
                GameObject.Find("Interface/Buttons/ButtonQuit").GetComponent<PlayMakerFSM>().FsmStates[0].RemoveAction(2);
                allModsLoaded = false;

                if (firstTimeMainMenuLoad)
                {
                    LoadReferences();
                    AssetBundle ab = LoadAssets.LoadBundle("LightspeedModLoader.Assets.lml.unity3d");
                    GameObject info = Instantiate(ab.LoadAsset<GameObject>("Info"));
                    Text vLabel = info.transform.Find("Version Label").GetComponent<Text>();
                    progressText = info.transform.Find("Progress Label").GetComponent<Text>();
                    vLabel.text = "Lightspeed Mod Loader\n" + (File.Exists("LML_VERSION") ? File.ReadAllText("LML_VERSION") : "Unknown???");
                    GameObject.DontDestroyOnLoad(vLabel.transform.parent.gameObject);
                    modFinishedSlider = vLabel.transform.parent.Find("Slider").GetComponent<Slider>();
                    modFinishedSlider.maxValue = loadedMods.Count + mscloadermodsloader.loadedMods.Count;
                    modFinishedSlider.value = 0;
                    ab.Unload(true);
                }

                modFinishedSlider.gameObject.SetActive(true);
                modFinishedSlider.transform.parent.gameObject.SetActive(true);

                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                foreach (Mod mod in A_OnMenuLoadMods)
                {
                    try
                    {
                        modFinishedSlider.value++;
                        progressText.text = mod.ID;
                        if (!mod.isDisabled)
                        {
                            if (Profiling)
                                profiler.Start(mod.ID + " OnMenuLoad");

                            mod.A_OnMenuLoad();

                            if (Profiling)
                                profiler.Stop(mod.ID + " OnMenuLoad");
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Error(e);
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
                        modFinishedSlider.value++;
                        progressText.text = mod.ID;
                        if (!mod.isDisabled)
                        {
                            mod.A_OnMenuLoad();
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }

                foreach (Mod mod in A_OnModSettingsMods)
                {
                    try
                    {
                        modFinishedSlider.value++;
                        progressText.text = mod.ID;
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
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }

                foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
                {
                    try
                    {
                        modFinishedSlider.value++;
                        progressText.text = mod.ID;
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
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }

                modFinishedSlider.gameObject.SetActive(false);
                progressText.text = "";

                if (firstTimeMainMenuLoad)
                {
                    MSCLoader.ModLoader.LoadModsSettings();
                    firstTimeMainMenuLoad = false;
                }
            }

            if (loadedLevelName == "GAME")
            {
                MSCLoader.ModLoader.CurrentScene = MSCLoader.CurrentScene.Game;
                LML_Debug.Log("\nGAME Level loaded. Running mods load methods\n");
                modFinishedSlider.gameObject.SetActive(true);
                progressText.text = "";
                modFinishedSlider.value = 0;
                modFinishedSlider.maxValue = loadedMods.Count + mscloadermodsloader.loadedMods.Count;
                StartCoroutine(LoadModsAsync());
            }

            if (loadedLevelName == "Intro")
            {
                MSCLoader.ModLoader.CurrentScene = MSCLoader.CurrentScene.NewGameIntro;
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
                progressText.text = mod.ID;
                yield return null;
                try
                {
                    if (!mod.isDisabled)
                    {
                        if (Profiling)
                            profiler.Start(mod.ID + " PreLoad");

                        mod.A_PreLoad();

                        if (Profiling)
                            profiler.Stop(mod.ID + " PreLoad");
                    }
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Error(e);
                }
                catch (Exception e)
                {
                    LML_Debug.Error(e);
                }

                modFinishedSlider.value++;
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.A_PreLoadMods)
            {
                progressText.text = mod.ID;
                yield return null;
                try
                {
                    if (!mod.isDisabled)
                        mod.A_PreLoad();
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Error(e);
                }
                catch (Exception e)
                {
                    LML_Debug.Error(e);
                }

                modFinishedSlider.value++;
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
            {
                progressText.text = mod.ID;
                yield return null;
                try
                {
                    if (!mod.isDisabled)
                        mod.PreLoad();
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LML_Debug.Error(e);
                }
                catch (Exception e)
                {
                    LML_Debug.Error(e);
                }

                modFinishedSlider.value++;
            }

            LML_Debug.Log("PreLoad Phase Complete");

            LML_Debug.Log("Waiting for game to finish loading");

            modFinishedSlider.value = 0;

            while (GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera") == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            int counter = 0;

            foreach (Mod mod in A_OnLoadMods)
            {
                progressText.text = mod.ID;
                if (!mod.isDisabled)
                {
                    if (counter++ >= batchSize)
                    {
                        yield return null;
                        counter = 0;
                    }

                    try
                    {
                        if (Profiling)
                            profiler.Start(mod.ID + " OnLoad");

                        mod.A_OnLoad();

                        if (Profiling)
                            profiler.Stop(mod.ID + " OnLoad");
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }

                modFinishedSlider.value++;
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.A_OnLoadMods)
            {
                progressText.text = mod.ID;
                if (!mod.isDisabled)
                {
                    if (counter++ >= batchSize)
                    {
                        yield return null;
                        counter = 0;
                    }

                    try
                    {
                        mod.A_OnLoad();
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }

                modFinishedSlider.value++;
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
            {
                progressText.text = mod.ID;
                if (!mod.isDisabled)
                {
                    if (counter++ >= batchSize)
                    {
                        yield return null;
                        counter = 0;
                    }

                    try
                    {
                        mod.OnLoad();
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }

                modFinishedSlider.value++;
            }

            LML_Debug.Log("OnLoad Phase Complete");

            modFinishedSlider.value = 0;

            foreach (Mod mod in A_PostLoadMods)
            {
                progressText.text = mod.ID;
                if (!mod.isDisabled)
                {
                    if (counter++ >= batchSize)
                    {
                        yield return null;
                        counter = 0;
                    }

                    try
                    {
                        if (Profiling)
                            profiler.Start(mod.ID + " PostLoad");

                        mod.A_PostLoad();

                        if (Profiling)
                            profiler.Stop(mod.ID + " PreLoad");
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }

                modFinishedSlider.value++;
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.A_PostLoadMods)
            {
                progressText.text = mod.ID;
                if (!mod.isDisabled)
                {
                    if (counter++ >= batchSize)
                    {
                        yield return null;
                        counter = 0;
                    }

                    try
                    {
                        mod.A_PostLoad();
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }

                modFinishedSlider.value++;
            }

            foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
            {
                progressText.text = mod.ID;
                if (!mod.isDisabled)
                {
                    if (counter++ >= batchSize)
                    {
                        yield return null;
                        counter = 0;
                    }

                    try
                    {
                        mod.SecondPassOnLoad();
                    }
                    catch (NullReferenceException e)
                    {
                        if (LogNullReferenceExceptions)
                            LML_Debug.Error(e);
                    }
                    catch (Exception e)
                    {
                        LML_Debug.Error(e);
                    }
                }

                modFinishedSlider.value++;
            }


            LML_Debug.Log("PostLoad Phase Complete");

            GameObject.Find("ITEMS").FsmInject("Save game", new Action(this.SaveMods));

            modFinishedSlider.gameObject.SetActive(false);
            modFinishedSlider.transform.parent.gameObject.SetActive(false);

            gameObject.AddComponent<GameOptimizations>();

            allModsLoaded = true;
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

        /// <summary>
        /// Returns if mods present
        /// </summary>
        /// <param name="modID">Mod ID</param>
        /// <returns>Returns if a mods present</returns>
        public static bool IsModPresent(string modID)
        {
            return GetMod(modID) != null;
        }


        /// <summary>
        /// Returns a mods asset folder path
        /// </summary>
        /// <param name="mod">Mod</param>
        /// <returns>Returns mod asset path</returns>
        public static string GetModAssetsFolder(Mod mod)
        {
            if (!Directory.Exists(Path.Combine(ModLoader.AssetsFolder, mod.ID)))
            {
                Directory.CreateDirectory(Path.Combine(ModLoader.AssetsFolder, mod.ID));
            }
            return Path.Combine(ModLoader.AssetsFolder, mod.ID);
        }

        /// <summary>
        /// Returns a mods asset folder
        /// </summary>
        /// <param name="mod">MSCLoader mod</param>
        /// <returns>Returns mod asset path</returns>
        public static string GetModAssetsFolder(MSCLoader.Mod mod)
        {
            if (!Directory.Exists(Path.Combine(ModLoader.AssetsFolder, mod.ID)))
            {
                Directory.CreateDirectory(Path.Combine(ModLoader.AssetsFolder, mod.ID));
            }
            return Path.Combine(ModLoader.AssetsFolder, mod.ID);
        }

        /// <summary>
        /// Returns a mods config folder
        /// </summary>
        /// <param name="mod">Mod</param>
        /// <returns>Returns config folder path</returns>
        public static string GetModConfigFolder(Mod mod)
        {
            if (!Directory.Exists(Path.Combine(ModLoader.ConfigFolder, mod.ID)))
            {
                Directory.CreateDirectory(Path.Combine(ModLoader.ConfigFolder, mod.ID));
            }
            return Path.Combine(ModLoader.ConfigFolder, mod.ID);
        }

        /// <summary>
        /// Returns a mods config folder
        /// </summary>
        /// <param name="mod">MSCLoader Mod</param>
        /// <returns>Returns config folder path</returns>
        public static string GetModConfigFolder(MSCLoader.Mod mod)
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

        /// <summary>
        /// Checks if a reference is present with the assemblyID
        /// </summary>
        /// <param name="assemblyID">assembly ID to check</param>
        /// <returns>Returns if a reference is present</returns>
        public static bool IsReferencePresent(string assemblyID)
        {
            return Instance.references.Contains(assemblyID);
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
                    Instance.monoBehavioursToProfile.Add(script);

                    if (!Instance.monoBehavioursProfiled.ContainsKey(script))
                    {
                        Instance.monoBehavioursProfiled[script] = new List<float>();
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

            float endTime = Time.time + 5f;

            int counter = 0;

            while (Time.time < endTime)
            {
                foreach (MonoBehaviour script in monoBehavioursToProfile)
                {
                    if (counter++ >= 50)
                    {
                        yield return null;
                        counter = 0;
                    }
                    Type scriptType = script.GetType();
                    MethodInfo updateMethod = scriptType.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (updateMethod != null)
                    {
                        if (script.enabled && script.gameObject.activeInHierarchy)
                        {
                            stopwatch.Start();
                            try
                            {
                                updateMethod.Invoke(script, null);
                            }
                            catch { }
                            stopwatch.Stop();
                            monoBehavioursProfiled[script].Add((float)stopwatch.Elapsed.TotalMilliseconds);
                            LML_Debug.Log("adding " + script.name);
                            stopwatch.Reset();
                        }
                    }
                }

                yield return null;
            }

            List<KeyValuePair<MonoBehaviour, float>> sortedProfiles = new List<KeyValuePair<MonoBehaviour, float>>();

            foreach (KeyValuePair<MonoBehaviour, List<float>> entry in monoBehavioursProfiled)
            {
                if (entry.Value.Count == 0)
                {
                    continue;
                }

                float totalTime = 0f;
                float highestTime = 0f;

                foreach (float f in entry.Value)
                {
                    totalTime += f;
                    if (f > highestTime) highestTime = f;
                }

                float averageTime = totalTime / entry.Value.Count;

                sortedProfiles.Add(new KeyValuePair<MonoBehaviour, float>(entry.Key, averageTime));
            }

            sortedProfiles.Sort((a, b) => b.Value.CompareTo(a.Value));

            string result = "";

            foreach (var entry in sortedProfiles)
            {
                MonoBehaviour behaviour = entry.Key;
                float averageTime = entry.Value;
                float highestTime = monoBehavioursProfiled[behaviour].Max();

                result += $"{behaviour.GetType()} On GameObject {behaviour.gameObject.name} Update method took avg: {averageTime:F3} ms ({averageTime / (1000 / (1 / Time.deltaTime)) * 100:F3}%), highest: {highestTime:F3} ms \n";
            }

            File.WriteAllText("ProfilerResult.txt", result);

            LML_Debug.Log("Deep profiling completed.");

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
