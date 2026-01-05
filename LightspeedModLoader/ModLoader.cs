using MSCLoader;
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
        #region Singleton
        public static ModLoader Instance { get; private set; }
        #endregion

        #region Paths
        public static string ModsFolder { get; private set; }
        internal static string AssetsFolder { get; private set; }
        internal static string ConfigFolder { get; private set; }
        internal static string ReferencesFolder { get; private set; }
        #endregion

        #region Loader State
        private static bool loaderPrepared = false;
        private static bool firstTimePreInitDone = true;
        internal bool allModsLoaded;
        internal bool firstTimeMainMenuLoad = true;

        public static bool PreLoadPhaseComplete { get; private set; }
        public static bool OnLoadPhaseComplete { get; private set; }
        public static bool PostLoadPhaseComplete { get; private set; }
        #endregion

        #region Configuration
        public static bool Profiling { get; private set; }
        public static bool LogNullReferenceExceptions { get; private set; } = true;
        public bool useAsyncUpdate = false;
        public int batchSize = 5;
        #endregion

        #region Mod Collections
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
        #endregion

        #region Components
        public MSCLoaderModsLoader mscloadermodsloader;
        public SaveLoad saveLoad;
        private static Profiler profiler;
        #endregion

        #region UI Elements
        internal Slider modFinishedSlider;
        private Text progressText;
        #endregion

        #region References
        internal List<string> references = new List<string>();
        #endregion

        #region Initialization

        /// <summary>
        /// Pre-initialization phase that sets up directory structure and starts mod loading.
        /// </summary>
        public static void PreInit()
        {
            if (!firstTimePreInitDone)
                return;

            InitializeDirectories();
            ConfigureLogging();
            
            LML_Debug.Log("Starting prepare");
            firstTimePreInitDone = false;
            Prepare();
        }

        /// <summary>
        /// Initializes all required directories for mod loading.
        /// </summary>
        private static void InitializeDirectories()
        {
            ModsFolder = Path.GetFullPath("mods");
            LML_Debug.Log("Mods Folder: " + ModsFolder);

            string[] requiredDirectories = { ModsFolder };
            foreach (string dir in requiredDirectories)
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            AssetsFolder = Path.Combine(ModsFolder, "Assets");
            ConfigFolder = Path.Combine(ModsFolder, "Config");
            ReferencesFolder = Path.Combine(ModsFolder, "References");

            string[] subDirectories = { AssetsFolder, ConfigFolder, ReferencesFolder };
            foreach (string dir in subDirectories)
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            LML_Debug.Log("Directory structure initialized");
        }

        /// <summary>
        /// Configures logging based on command line arguments.
        /// </summary>
        private static void ConfigureLogging()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Contains("-LML-DisableLogNullReferenceExceptions"))
                LogNullReferenceExceptions = false;

            if (args.Contains("-LML-DisableLogging"))
                LML_Debug.enableLogging = false;

            LML_Debug.Init();
        }

        /// <summary>
        /// Prepares the mod loader by creating the main GameObject and initializing components.
        /// </summary>
        private static void Prepare()
        {
            if (loaderPrepared)
                return;

            loaderPrepared = true;

            GameObject loaderObject = new GameObject("LML", typeof(ModLoader));
            Instance = loaderObject.GetComponent<ModLoader>();
            Instance.mscloadermodsloader = loaderObject.AddComponent<MSCLoaderModsLoader>();
            Instance.saveLoad = loaderObject.AddComponent<SaveLoad>();
            DontDestroyOnLoad(loaderObject);

            InitializeProfiling();
            loaderObject.AddComponent<UnityMainThreadDispatcher>();

            LML_Debug.Log("Preparation complete");
            LML_Debug.Log("Loading references");
            Instance.LoadReferences();

            LML_Debug.Log("Pre-loading mods");
            Instance.PreLoadMods();
        }

        /// <summary>
        /// Initializes profiling if enabled via command line.
        /// </summary>
        private static void InitializeProfiling()
        {
            if (Environment.GetCommandLineArgs().Contains("-LML-Profile"))
            {
                profiler = new Profiler();
                Profiling = true;
                LML_Debug.Log("Internal LML Profiler enabled");
            }
        }

        #endregion

        #region Mod Loading

        /// <summary>
        /// Pre-loads all mods by discovering and loading DLL files.
        /// </summary>
        private void PreLoadMods()
        {
            if (Profiling)
                profiler.Start("Load DLLs");

            LoadModDLLs();

            if (Profiling)
                profiler.Stop("Load DLLs");

            LML_Debug.Log("Mod DLLs loaded");

            if (Profiling)
                profiler.Start("Load Mod Actions");

            LoadModsActions();
            mscloadermodsloader.LoadModsActions();

            if (Profiling)
                profiler.Stop("Load Mod Actions");

            LML_Debug.Log("Mod actions/methods loaded");

            saveLoad.Load();

            if (useAsyncUpdate)
                StartCoroutine(UpdateAsync());
        }

        /// <summary>
        /// Discovers and loads all DLL files from the mods folder.
        /// </summary>
        private void LoadModDLLs()
        {
            string[] files = Directory.GetFiles(ModsFolder, "*.dll");

            foreach (string file in files)
            {
                try
                {
                    LoadDLL(file);
                }
                catch (Exception ex)
                {
                    LML_Debug.Error(ex);
                }
            }
        }

        /// <summary>
        /// Loads a single DLL file and instantiates any mod classes found within.
        /// </summary>
        /// <param name="file">Path to the DLL file</param>
        private void LoadDLL(string file)
        {
            Assembly assembly;
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
            foreach (Type type in types)
            {
                if (TryLoadLightspeedMod(type))
                    return;

                if (TryLoadMSCLoaderMod(type))
                    return;
            }
        }

        /// <summary>
        /// Attempts to load a Lightspeed mod from the given type.
        /// </summary>
        /// <returns>True if a mod was loaded, false otherwise</returns>
        private bool TryLoadLightspeedMod(Type type)
        {
            if (!type.IsSubclassOf(typeof(Mod)))
                return false;

            Mod mod = (Mod)Activator.CreateInstance(type);
            if (string.IsNullOrEmpty(mod.ID?.Trim()))
            {
                LML_Debug.Log("Empty mod ID detected");
                return false;
            }

            LoadMod(mod);
            return true;
        }

        /// <summary>
        /// Attempts to load an MSCLoader mod from the given type.
        /// </summary>
        /// <returns>True if a mod was loaded, false otherwise</returns>
        private bool TryLoadMSCLoaderMod(Type type)
        {
            if (!type.IsSubclassOf(typeof(MSCLoader.Mod)))
                return false;

            MSCLoader.Mod mod = (MSCLoader.Mod)Activator.CreateInstance(type);
            if (string.IsNullOrEmpty(mod.ID?.Trim()))
            {
                LML_Debug.Log("Empty mod ID detected");
                return false;
            }

            LML_Debug.Log("Loading MSCLoader mod: " + mod.ID);
            mscloadermodsloader.LoadMod(mod);
            return true;
        }

        /// <summary>
        /// Loads and initializes a single mod.
        /// </summary>
        private void LoadMod(Mod mod)
        {
            try
            {
                if (loadedMods.Contains(mod))
                    return;

                if (Profiling)
                    profiler.Start(mod.ID + " ModSetup");

                mod.ModSetup();

                if (Profiling)
                    profiler.Stop(mod.ID + " ModSetup");

                loadedMods.Add(mod);
            }
            catch (Exception ex)
            {
                LML_Debug.Error(ex);
            }
        }

        /// <summary>
        /// Registers mod callbacks by populating action lists.
        /// </summary>
        private void LoadModsActions()
        {
            foreach (Mod mod in loadedMods)
            {
                RegisterModCallbacks(mod);
            }
        }

        /// <summary>
        /// Registers all callbacks for a specific mod.
        /// </summary>
        private void RegisterModCallbacks(Mod mod)
        {
            if (mod.A_OnNewGame != null) A_OnNewGameMods.Add(mod);
            if (mod.A_OnMenuLoad != null) A_OnMenuLoadMods.Add(mod);
            if (mod.A_PreLoad != null) A_PreLoadMods.Add(mod);
            if (mod.A_OnLoad != null) A_OnLoadMods.Add(mod);
            if (mod.A_PostLoad != null) A_PostLoadMods.Add(mod);
            if (mod.A_Update != null) A_UpdateMods.Add(mod);
            if (mod.A_FixedUpdate != null) A_FixedUpdateMods.Add(mod);
            if (mod.A_OnSave != null) A_OnSaveMods.Add(mod);
            if (mod.A_OnGUI != null) A_OnGUIMods.Add(mod);
            if (mod.A_ModSettings != null) A_OnModSettingsMods.Add(mod);
        }

        #endregion

        #region Unity Lifecycle

        internal void OnGUI()
        {
            ExecuteModCallbacks(A_OnGUIMods, mod => mod.A_OnGUI());
            ExecuteModCallbacks(mscloadermodsloader.A_OnGUIMods, mod => mod.A_OnGUI());
            ExecuteModCallbacks(mscloadermodsloader.loadedMods, mod => mod.OnGUI());
        }

        internal void Update()
        {
            if (useAsyncUpdate)
                return;

            ExecuteModCallbacks(A_UpdateMods, mod => mod.A_Update(), "Update");
            ExecuteModCallbacks(mscloadermodsloader.A_UpdateMods, mod => mod.A_Update(), "Update");
            ExecuteModCallbacks(mscloadermodsloader.loadedMods, mod => mod.Update(), "Update");
        }

        internal void FixedUpdate()
        {
            ExecuteModCallbacks(A_FixedUpdateMods, mod => mod.A_FixedUpdate());
            ExecuteModCallbacks(mscloadermodsloader.A_FixedUpdateMods, mod => mod.A_FixedUpdate());
            ExecuteModCallbacks(mscloadermodsloader.loadedMods, mod => mod.FixedUpdate());
        }

        #endregion

        #region Async Update

        internal IEnumerator UpdateAsync()
        {
            int counter = 0;

            while (true)
            {
                counter = yield return ExecuteModCallbacksAsync(A_UpdateMods, mod => mod.A_Update(), counter);
                counter = yield return ExecuteModCallbacksAsync(mscloadermodsloader.A_UpdateMods, mod => mod.A_Update(), counter);
                counter = yield return ExecuteModCallbacksAsync(mscloadermodsloader.loadedMods, mod => mod.Update(), counter);
            }
        }

        private IEnumerator ExecuteModCallbacksAsync<T>(List<T> mods, Action<T> callback, int counter) where T : MSCLoader.Mod
        {
            foreach (T mod in mods)
            {
                if (counter++ >= batchSize)
                {
                    yield return null;
                    counter = 0;
                }

                if (mod.isDisabled || (!allModsLoaded && !mod.LoadInMenu))
                    continue;

                try
                {
                    callback(mod);
                }
                catch (NullReferenceException e)
                {
                    if (LogNullReferenceExceptions)
                        LogModException(e, mod.ID);
                }
                catch (Exception e)
                {
                    LogModException(e, mod.ID);
                }
            }

            yield return counter;
        }

        #endregion

        #region Scene Management

        internal void OnLevelWasLoaded(int level)
        {
            string sceneName = Application.loadedLevelName;

            if (sceneName == "MainMenu")
            {
                HandleMainMenuLoad();
            }
            else if (sceneName == "GAME")
            {
                HandleGameLoad();
            }
        }

        /// <summary>
        /// Handles mod initialization when the main menu is loaded.
        /// </summary>
        private void HandleMainMenuLoad()
        {
            MSCLoader.ModLoader.CurrentScene = MSCLoader.CurrentScene.MainMenu;
            
            ConfigureMainMenu();
            InitializeModUI();
            
            ExecuteMenuLoadCallbacks();
            ExecuteModSettingsCallbacks();

            if (firstTimeMainMenuLoad)
            {
                MSCLoader.ModLoader.LoadModsSettings();
                firstTimeMainMenuLoad = false;
            }

            InjectNewGameCallback();
            
            allModsLoaded = false;
        }

        /// <summary>
        /// Configures main menu UI elements.
        /// </summary>
        private void ConfigureMainMenu()
        {
            GameObject.Find("Quit")?.SetActive(false);
            
            var quitButton = GameObject.Find("Interface/Buttons/ButtonQuit")?.GetComponent<PlayMakerFSM>();
            if (quitButton != null)
            {
                quitButton.FsmStates[0].RemoveAction(2);
            }

            var continueButton = GameObject.Find("Interface/Buttons/ButtonContinue")?.GetComponent<PlayMakerFSM>();
            if (continueButton != null)
            {
                var loadAction = PlayMakerExtensions.GetAction<HutongGames.PlayMaker.Actions.LoadLevel>(
                    PlayMakerExtensions.GetState(continueButton, "Load"), 0);
                if (loadAction != null)
                    loadAction.async = true;
            }

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        /// <summary>
        /// Initializes the mod loading UI.
        /// </summary>
        private void InitializeModUI()
        {
            if (firstTimeMainMenuLoad)
            {
                AssetBundle ab = LoadAssets.LoadBundle("LightspeedModLoader.Assets.lml.unity3d");
                GameObject info = Instantiate(ab.LoadAsset<GameObject>("Info"));
                
                Text vLabel = info.transform.Find("Version Label").GetComponent<Text>();
                progressText = info.transform.Find("Progress Label").GetComponent<Text>();
                
                string version = File.Exists("LML_VERSION") ? File.ReadAllText("LML_VERSION") : "Unknown";
                vLabel.text = $"Lightspeed Mod Loader\n{version}";
                
                DontDestroyOnLoad(vLabel.transform.parent.gameObject);
                
                modFinishedSlider = vLabel.transform.parent.Find("Slider").GetComponent<Slider>();
                modFinishedSlider.maxValue = loadedMods.Count + mscloadermodsloader.loadedMods.Count;
                modFinishedSlider.value = 0;
                
                ab.Unload(true);
            }

            modFinishedSlider.gameObject.SetActive(true);
            modFinishedSlider.transform.parent.gameObject.SetActive(true);
        }

        /// <summary>
        /// Executes OnMenuLoad callbacks for all mods.
        /// </summary>
        private void ExecuteMenuLoadCallbacks()
        {
            ExecuteModCallbacks(A_OnMenuLoadMods, mod => mod.A_OnMenuLoad(), "OnMenuLoad", true);
            ExecuteModCallbacks(mscloadermodsloader.A_OnMenuLoadMods, mod => mod.A_OnMenuLoad(), null, true);
        }

        /// <summary>
        /// Executes ModSettings callbacks for all mods on first menu load.
        /// </summary>
        private void ExecuteModSettingsCallbacks()
        {
            if (!firstTimeMainMenuLoad)
                return;

            ExecuteModCallbacks(A_OnModSettingsMods, mod => mod.A_ModSettings(), null, true);

            foreach (MSCLoader.Mod mod in mscloadermodsloader.loadedMods)
            {
                try
                {
                    UpdateProgressUI(mod.ID);
                    
                    if (mod.isDisabled)
                        continue;

                    if (mod.A_ModSettings != null)
                    {
                        LML_Debug.Log("Loading mod settings for " + mod.ID);
                        mod.A_ModSettings();
                    }
                    else if (CheckEmptyMethod(mod, "ModSettings"))
                    {
                        LML_Debug.Log("Loading deprecated mod settings for " + mod.ID);
                        mod.ModSettings();
                    }
                }
                catch (Exception e)
                {
                    HandleModException(e, mod.ID);
                }
            }

            HideProgressUI();
        }

        /// <summary>
        /// Injects the new game callback into the Begin button FSM.
        /// </summary>
        private void InjectNewGameCallback()
        {
            PlayMakerFSM newGameFSM = FindNewGameFSM();
            if (newGameFSM != null)
            {
                newGameFSM.FsmInject("State 2", OnNewGame);
            }
        }

        /// <summary>
        /// Finds the PlayMaker FSM for the new game button.
        /// </summary>
        private PlayMakerFSM FindNewGameFSM()
        {
            foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (obj.name == "ButtonBegin")
                {
                    return obj.GetComponent<PlayMakerFSM>();
                }
            }
            return null;
        }

        /// <summary>
        /// Handles the new game event and triggers relevant mod callbacks.
        /// </summary>
        private void OnNewGame()
        {
            MSCLoader.ModLoader.CurrentScene = MSCLoader.CurrentScene.NewGameIntro;
            
            ExecuteModCallbacks(A_OnNewGameMods, mod => mod.A_OnNewGame());
            ExecuteModCallbacks(mscloadermodsloader.A_OnNewGameMods, mod => mod.A_OnNewGame());
            ExecuteModCallbacks(mscloadermodsloader.loadedMods, mod => mod.OnNewGame());
        }

        /// <summary>
        /// Handles mod initialization when the game scene is loaded.
        /// </summary>
        private void HandleGameLoad()
        {
            MSCLoader.ModLoader.CurrentScene = MSCLoader.CurrentScene.Game;
            LML_Debug.Log("\nGAME scene loaded. Initializing mods\n");
            
            modFinishedSlider.gameObject.SetActive(true);
            progressText.text = "";
            modFinishedSlider.value = 0;
            modFinishedSlider.maxValue = loadedMods.Count + mscloadermodsloader.loadedMods.Count;
            
            StartCoroutine(LoadModsAsync());
        }

        #endregion

        #region Async Mod Loading

        internal IEnumerator LoadModsAsync()
        {
            yield return ExecuteLoadPhase("PreLoad", A_PreLoadMods, mod => mod.A_PreLoad());
            yield return ExecuteLoadPhase("PreLoad", mscloadermodsloader.A_PreLoadMods, mod => mod.A_PreLoad());
            yield return ExecuteLoadPhase("PreLoad", mscloadermodsloader.loadedMods, mod => mod.PreLoad());
            
            LML_Debug.Log("PreLoad phase complete");
            LML_Debug.Log("Waiting for game to finish loading");

            modFinishedSlider.value = 0;

            // Wait for player camera to be initialized
            while (GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera") == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            yield return ExecuteLoadPhase("OnLoad", A_OnLoadMods, mod => mod.A_OnLoad(), true);
            yield return ExecuteLoadPhase("OnLoad", mscloadermodsloader.A_OnLoadMods, mod => mod.A_OnLoad(), true);
            yield return ExecuteLoadPhase("OnLoad", mscloadermodsloader.loadedMods, mod => mod.OnLoad(), true);
            
            LML_Debug.Log("OnLoad phase complete");

            modFinishedSlider.value = 0;

            yield return ExecuteLoadPhase("PostLoad", A_PostLoadMods, mod => mod.A_PostLoad(), true);
            yield return ExecuteLoadPhase("PostLoad", mscloadermodsloader.A_PostLoadMods, mod => mod.A_PostLoad(), true);
            yield return ExecuteLoadPhase("SecondPassOnLoad", mscloadermodsloader.loadedMods, mod => mod.SecondPassOnLoad(), true);

            LML_Debug.Log("PostLoad phase complete");

            InjectSaveCallback();
            HideProgressUI();

            allModsLoaded = true;
        }

        /// <summary>
        /// Executes a loading phase for a collection of mods.
        /// </summary>
        private IEnumerator ExecuteLoadPhase<T>(string phaseName, List<T> mods, Action<T> callback, bool useBatching = false) where T : MSCLoader.Mod
        {
            int counter = 0;

            foreach (T mod in mods)
            {
                UpdateProgressUI(mod.ID);

                if (useBatching && counter++ >= batchSize)
                {
                    yield return null;
                    counter = 0;
                }
                else if (!useBatching)
                {
                    yield return null;
                }

                if (mod.isDisabled)
                {
                    modFinishedSlider.value++;
                    continue;
                }

                try
                {
                    if (Profiling)
                        profiler.Start($"{mod.ID} {phaseName}");

                    callback(mod);

                    if (Profiling)
                        profiler.Stop($"{mod.ID} {phaseName}");
                }
                catch (Exception e)
                {
                    HandleModException(e, mod.ID);
                }

                modFinishedSlider.value++;
            }
        }

        /// <summary>
        /// Injects the save callback into the game's save FSM.
        /// </summary>
        private void InjectSaveCallback()
        {
            var saveGameFSM = GameObject.Find("Systems/Setup Game")?.GetComponent<PlayMakerFSM>();
            if (saveGameFSM != null)
            {
                saveGameFSM.FsmInject("Save game", SaveMods);
            }
        }

        #endregion

        #region Save System

        /// <summary>
        /// Triggers save callbacks for all mods.
        /// </summary>
        internal void SaveMods()
        {
            ExecuteModCallbacks(A_OnSaveMods, mod => mod.A_OnSave());
            ExecuteModCallbacks(mscloadermodsloader.A_OnSaveMods, mod => mod.A_OnSave());
            ExecuteModCallbacks(mscloadermodsloader.loadedMods, mod => mod.OnSave());
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Executes callbacks for a collection of mods with error handling.
        /// </summary>
        private void ExecuteModCallbacks<T>(List<T> mods, Action<T> callback, string profilerLabel = null, bool updateUI = false) where T : MSCLoader.Mod
        {
            foreach (T mod in mods)
            {
                if (updateUI)
                    UpdateProgressUI(mod.ID);

                if (mod.isDisabled || (!allModsLoaded && !mod.LoadInMenu))
                    continue;

                try
                {
                    if (Profiling && profilerLabel != null)
                        profiler.Start($"{mod.ID} {profilerLabel}");

                    callback(mod);

                    if (Profiling && profilerLabel != null)
                        profiler.Stop($"{mod.ID} {profilerLabel}");
                }
                catch (Exception e)
                {
                    HandleModException(e, mod.ID);
                }

                if (updateUI)
                    modFinishedSlider.value++;
            }
        }

        /// <summary>
        /// Handles exceptions thrown by mods.
        /// </summary>
        private void HandleModException(Exception e, string modID)
        {
            if (e is NullReferenceException && !LogNullReferenceExceptions)
                return;

            LogModException(e, modID);
        }

        /// <summary>
        /// Logs a mod exception with detailed information.
        /// </summary>
        private void LogModException(Exception e, string modID)
        {
            var stackTrace = new StackTrace(e, true);
            var frame = stackTrace.GetFrame(0);
            var method = frame?.GetMethod();

            string message = $"{Environment.NewLine}<b>Details:</b> {e.Message} in Mod <b>{modID}</b>";
            
            if (method != null)
            {
                message += $" in <b>{method}</b>";
            }

            if (e is NullReferenceException)
            {
                message += $" in object <b>{e.Source}</b>. StackTrace: <b>{e.StackTrace}</b>";
            }

            LML_Debug.Log(message);
        }

        /// <summary>
        /// Updates the progress UI with the current mod being loaded.
        /// </summary>
        private void UpdateProgressUI(string modID)
        {
            if (progressText != null)
                progressText.text = modID;
            
            if (modFinishedSlider != null)
                modFinishedSlider.value++;
        }

        /// <summary>
        /// Hides the progress UI.
        /// </summary>
        private void HideProgressUI()
        {
            if (modFinishedSlider != null)
            {
                modFinishedSlider.gameObject.SetActive(false);
                modFinishedSlider.transform.parent.gameObject.SetActive(false);
            }
            
            if (progressText != null)
                progressText.text = "";
        }

        #endregion

        #region Public API

        /// <summary>
        /// Retrieves a loaded mod by its ID.
        /// </summary>
        /// <param name="modID">The mod's unique identifier</param>
        /// <returns>The mod instance, or null if not found</returns>
        public static object GetMod(string modID)
        {
            string lowerID = modID.ToLower();

            foreach (Mod mod in Instance.loadedMods)
            {
                if (mod.ID.ToLower() == lowerID)
                    return mod;
            }

            foreach (MSCLoader.Mod mod in Instance.mscloadermodsloader.loadedMods)
            {
                if (mod.ID.ToLower() == lowerID)
                    return mod;
            }

            return null;
        }

        /// <summary>
        /// Checks if a mod with the specified ID is loaded.
        /// </summary>
        /// <param name="modID">The mod's unique identifier</param>
        /// <returns>True if the mod is loaded, false otherwise</returns>
        public static bool IsModPresent(string modID)
        {
            return GetMod(modID) != null;
        }

        /// <summary>
        /// Gets the assets folder path for a Lightspeed mod.
        /// </summary>
        /// <param name="mod">The mod instance</param>
        /// <returns>The full path to the mod's assets folder</returns>
        public static string GetModAssetsFolder(Mod mod)
        {
            return EnsureDirectoryExists(Path.Combine(AssetsFolder, mod.ID));
        }

        /// <summary>
        /// Gets the assets folder path for an MSCLoader mod.
        /// </summary>
        /// <param name="mod">The mod instance</param>
        /// <returns>The full path to the mod's assets folder</returns>
        public static string GetModAssetsFolder(MSCLoader.Mod mod)
        {
            return EnsureDirectoryExists(Path.Combine(AssetsFolder, mod.ID));
        }

        /// <summary>
        /// Gets the config folder path for a Lightspeed mod.
        /// </summary>
        /// <param name="mod">The mod instance</param>
        /// <returns>The full path to the mod's config folder</returns>
        public static string GetModConfigFolder(Mod mod)
        {
            return EnsureDirectoryExists(Path.Combine(ConfigFolder, mod.ID));
        }

        /// <summary>
        /// Gets the config folder path for an MSCLoader mod.
        /// </summary>
        /// <param name="mod">The mod instance</param>
        /// <returns>The full path to the mod's config folder</returns>
        public static string GetModConfigFolder(MSCLoader.Mod mod)
        {
            return EnsureDirectoryExists(Path.Combine(ConfigFolder, mod.ID));
        }

        /// <summary>
        /// Ensures a directory exists, creating it if necessary.
        /// </summary>
        /// <param name="path">The directory path</param>
        /// <returns>The directory path</returns>
        private static string EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            return path;
        }

        /// <summary>
        /// Checks if a reference assembly is loaded.
        /// </summary>
        /// <param name="assemblyID">The assembly name to check</param>
        /// <returns>True if the reference is loaded, false otherwise</returns>
        public static bool IsReferencePresent(string assemblyID)
        {
            return Instance.references.Contains(assemblyID);
        }

        #endregion

        #region Reference Loading

        /// <summary>
        /// loads all reference assemblies from the References folder.
        /// </summary>
        internal void LoadReferences()
        {
            if (!Directory.Exists(ReferencesFolder))
                return;

            string[] dllFiles = Directory.GetFiles(ReferencesFolder, "*.dll");

            foreach (string dllPath in dllFiles)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(dllPath);
                    references.Add(assembly.GetName().Name);
                }
                catch (Exception ex)
                {
                    LML_Debug.Log($"{dllPath} reference failed to load");
                    LML_Debug.Error(ex);
                }
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// checks if a method is overridden in a derived class and contains implementation.
        /// </summary>
        /// <param name="mod">The mod instance</param>
        /// <param name="methodName">The method name to check</param>
        /// <returns>True if the method is overridden and implemented</returns>
        internal static bool CheckEmptyMethod(MSCLoader.Mod mod, string methodName)
        {
            MethodInfo method = mod.GetType().GetMethod(methodName);
            
            if (method == null)
                return false;

            bool isOverridden = method.IsVirtual && method.DeclaringType == mod.GetType();
            bool hasImplementation = method.GetMethodBody()?.GetILAsByteArray().Length > 2;

            return isOverridden && hasImplementation;
        }

        /// <summary>
        /// checks if a type belongs to a modded assembly.
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>true if the type is from a mod assembly</returns>
        private bool IsModdedClass(Type type)
        {
            Type[] assemblyTypes = type.Assembly.GetTypes();
            
            foreach (Type assemblyType in assemblyTypes)
            {
                if (assemblyType.IsSubclassOf(typeof(Mod)) || 
                    assemblyType.IsSubclassOf(typeof(MSCLoader.Mod)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// dumps PlayMaker global variables to a text file for debugging.
        /// </summary>
        void DumpGlobals()
        {
            var globals = PlayMakerGlobals.Instance.Variables;
            var output = new System.Text.StringBuilder();

            output.AppendLine("=== PLAYMAKER GLOBAL VARIABLES ===");
            
            AppendVariableSection(output, "Int Variables", globals.IntVariables);
            AppendVariableSection(output, "Float Variables", globals.FloatVariables);
            AppendVariableSection(output, "Bool Variables", globals.BoolVariables);
            AppendVariableSection(output, "String Variables", globals.StringVariables);
            AppendVariableSection(output, "Vector3 Variables", globals.Vector3Variables);

            File.WriteAllText("dumpedVariables.txt", output.ToString());
        }

        /// <summary>
        /// Appends a section of variables to the output string builder.
        /// </summary>
        private void AppendVariableSection<T>(System.Text.StringBuilder output, string sectionName, T[] variables) where T : HutongGames.PlayMaker.NamedVariable
        {
            output.AppendLine($"\n{sectionName}");
            
            foreach (var variable in variables)
            {
                output.AppendLine($"{variable.Name}  =  {variable.RawValue}");
            }
        }

        #endregion

        #region Profiling

        /// <summary>
        /// instability probably here
        /// </summary>
        public static void DeepProfileAllMods()
        {
            Instance.StartCoroutine(Instance.DeepProfileCoroutine());

            foreach (GameObject obj in GameObject.FindObjectsOfTypeAll<GameObject>())
            {
                foreach (MonoBehaviour script in obj.GetComponents<MonoBehaviour>())
                {
                    Instance.monoBehavioursToProfile.Add(script);

                    if (!Instance.monoBehavioursProfiled.ContainsKey(script))
                    {
                        Instance.monoBehavioursProfiled[script] = new List<float>();
                    }
                }
            }
        }

        private List<MonoBehaviour> monoBehavioursToProfile = new List<MonoBehaviour>();
        private Dictionary<MonoBehaviour, List<float>> monoBehavioursProfiled = new Dictionary<MonoBehaviour, List<float>>();

        /// <summary>
        /// perform deep profiling of all registered MonoBehaviours.
        /// </summary>
        internal IEnumerator DeepProfileCoroutine()
        {
            LML_Debug.Log("WARNING: Deep profiling may cause instability or freezing.");
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

                    if (!script.enabled || !script.gameObject.activeInHierarchy)
                        continue;

                    Type scriptType = script.GetType();
                    MethodInfo updateMethod = scriptType.GetMethod("Update", 
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    
                    if (updateMethod == null)
                        continue;

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

                yield return null;
            }

            GenerateProfilingReport();
            LML_Debug.Log("Deep profiling completed. Results saved to ProfilerResult.txt");
        }

        /// <summary>
        /// generates and saves the profiling report to a file.
        /// </summary>
        private void GenerateProfilingReport()
        {
            var sortedProfiles = new List<KeyValuePair<MonoBehaviour, ProfileStats>>();

            foreach (var entry in monoBehavioursProfiled)
            {
                if (entry.Value.Count == 0)
                    continue;

                float totalTime = entry.Value.Sum();
                float averageTime = totalTime / entry.Value.Count;
                float highestTime = entry.Value.Max();
                float percentageOfFrame = averageTime / (1000f / (1f / Time.deltaTime)) * 100f;

                sortedProfiles.Add(new KeyValuePair<MonoBehaviour, ProfileStats>(
                    entry.Key, 
                    new ProfileStats(averageTime, highestTime, percentageOfFrame)
                ));
            }

            sortedProfiles.Sort((a, b) => b.Value.AverageTime.CompareTo(a.Value.AverageTime));

            var output = new System.Text.StringBuilder();
            output.AppendLine("=== DEEP PROFILING RESULTS ===\n");

            foreach (var entry in sortedProfiles)
            {
                MonoBehaviour behaviour = entry.Key;
                ProfileStats stats = entry.Value;

                output.AppendLine($"{behaviour.GetType()} on GameObject '{behaviour.gameObject.name}'");
                output.AppendLine($"  Average: {stats.AverageTime:F3} ms ({stats.PercentageOfFrame:F3}%)");
                output.AppendLine($"  Highest: {stats.HighestTime:F3} ms\n");
            }

            File.WriteAllText("ProfilerResult.txt", output.ToString());
        }

        /// <summary>
        /// container for profiling statistics.
        /// </summary>
        private struct ProfileStats
        {
            public float AverageTime { get; }
            public float HighestTime { get; }
            public float PercentageOfFrame { get; }

            public ProfileStats(float average, float highest, float percentage)
            {
                AverageTime = average;
                HighestTime = highest;
                PercentageOfFrame = percentage;
            }
        }

        #endregion
    }
}
