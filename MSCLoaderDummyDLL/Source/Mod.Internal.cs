#if !Mini
using System;
using System.Collections.Generic;

namespace MSCLoader;

public partial class Mod
{
    /// <summary>
    /// Type of Function to setup
    /// </summary>
    public enum Setup
    {
        /// <summary>
        /// OnNewGame - Called once when new game (not continue old save) is started
        /// </summary>
        OnNewGame,
        /// <summary>
        /// OnMenuLoad - Setup function that is executed once in MainMenu
        /// </summary>
        OnMenuLoad,
        /// <summary>
        /// PreLoad - Phase 1 of mod loading (executed once after GAME scene is loaded)
        /// </summary>
        PreLoad,
        /// <summary>
        /// OnLoad - Phase 2 of mod loading (executed once GAME scene is fully loaded)
        /// </summary>
        OnLoad,
        /// <summary>
        /// PostLoad - Phase 3 of mod loading (executed once after all mods finished with Phase 2)
        /// </summary>
        PostLoad,
        /// <summary>
        /// OnSave - Executed once after game is being saved.
        /// </summary>
        OnSave,
        /// <summary>
        /// OnGUI - Works same way as unity OnGUI
        /// </summary>
        OnGUI,
        /// <summary>
        /// Update - Works same way as unity Update
        /// </summary>
        Update,
        /// <summary>
        /// FixedUpdate - Works same way as unity FixedUpdate
        /// </summary>
        FixedUpdate,
        /// <summary>
        /// OnModEnabled - Called once when mod has been enabled in settings
        /// </summary>
        OnModEnabled,
        /// <summary>
        /// OnModDisabled - Called once when mod has been disabled in settings
        /// </summary>
        OnModDisabled,
        /// <summary>
        /// ModSettingsLoaded - Called after saved settings have been loaded from file.
        /// </summary>
        ModSettingsLoaded,
        /// <summary>
        /// ModSettings - All settings and Keybinds should be created here.
        /// </summary>
        ModSettings
    }

    /// <summary>
    /// true if mod is disabled
    /// </summary>
    public bool isDisabled { get; set; }
    public bool hasUpdate = false;
    public bool newFormat = false;
    public bool newEnDisFormat = false;
    public bool newSettingsFormat = false;
    public bool menuCallbacks = false;
    public bool hideResetAllSettings = false;
    public bool disableWarn = false;
    public int modErrors = 0;
    public string compiledVersion;
    public string fileName;
    internal MSCLData metadata;             //Local metadata
    internal MetaVersion UpdateInfo;        //Update info

    //Action list
    public Action A_OnMenuLoad;           //Load in main menu
    public Action A_OnNewGame;            //When New Game is started
    public Action A_PreLoad;              //Phase 1 (mod loading)
    public Action A_OnLoad;               //Phase 2 (mod loading)  
    public Action A_PostLoad;             //Phase 3 (mod loading)
    public Action A_OnSave;               //When game saves
    public Action A_OnGUI;                //Calls unity OnGUI
    public Action A_Update;               //Calls unity Update
    public Action A_FixedUpdate;          //Calls unity FixedUpdate
    public Action A_OnModEnabled;         //When mod has been enabled
    public Action A_OnModDisabled;        //When mod has been disabled
    public Action A_ModSettings;          //Create mod settings from here.
    public Action A_ModSettingsLoaded;    //When mod settings have been loaded from file

    public List<ModSetting> modSettingsList = new List<ModSetting>();
    public List<Keybind> Keybinds = new List<Keybind>();
    public List<Keybind> DefaultKeybinds = new List<Keybind>();
    public string[] AdditionalReferences = new string[0];
    public string asmGuid = "";
    /// <summary>
    /// Setup selected function for your mod
    /// </summary>
    /// <param name="functionType">Function type</param>
    /// <param name="function">Your own function to execute that type</param>
    public void SetupFunction(Setup functionType, Action function)
    {
        newFormat = true;
        switch (functionType)
        {
            case Setup.OnNewGame:
                if (A_OnNewGame == null)
                    A_OnNewGame = function;
                else
                    ModConsole.Error($"SetupMod() error for <b>{ID}</b>. You have already created <b>OnNewGame</b> function type.");
                break;
            case Setup.OnMenuLoad:
                menuCallbacks = true;
                if (A_OnMenuLoad == null)
                    A_OnMenuLoad = function;
                else
                    ModConsole.Error($"SetupMod() error for <b>{ID}</b>. You have already created <b>OnMenuLoad</b> function type.");
                break;
            case Setup.PreLoad:
                if (A_PreLoad == null)
                    A_PreLoad = function;
                else
                    ModConsole.Error($"SetupMod() error for <b>{ID}</b>. You have already created <b>PreLoad</b> function type.");
                break;
            case Setup.OnLoad:
                if (A_OnLoad == null)
                    A_OnLoad = function;
                else
                    ModConsole.Error($"SetupMod() error for <b>{ID}</b>. You have already created <b>OnLoad</b> function type.");
                break;
            case Setup.PostLoad:
                if (A_PostLoad == null)
                    A_PostLoad = function;
                else
                    ModConsole.Error($"SetupMod() error for <b>{ID}</b>. You have already created <b>PostLoad</b> function type.");
                break;
            case Setup.OnSave:
                if (A_OnSave == null)
                    A_OnSave = function;
                else
                    ModConsole.Error($"SetupMod() error for <b>{ID}</b>. You have already created <b>OnSave</b> function type.");
                break;
            case Setup.OnGUI:
                if (A_OnGUI == null)
                    A_OnGUI = function;
                else
                    ModConsole.Error($"SetupMod() error for <b>{ID}</b>. You have already created <b>OnGUI</b> function type.");
                break;
            case Setup.Update:
                if (A_Update == null)
                    A_Update = function;
                else
                    ModConsole.Error($"SetupMod() error for <b>{ID}</b>. You have already created <b>Update</b> function type.");
                break;
            case Setup.FixedUpdate:
                if (A_FixedUpdate == null)
                    A_FixedUpdate = function;
                else
                    ModConsole.Error($"SetupMod() error for <b>{ID}</b>. You have already created <b>FixedUpdate</b> function type.");
                break;
            case Setup.OnModEnabled:
                newEnDisFormat = true;
                if (A_OnModEnabled == null)
                    A_OnModEnabled = function;
                else
                    ModConsole.Error($"SetupMod() error for <b>{ID}</b>. You have already created <b>OnModEnabled</b> function type.");
                break;
            case Setup.OnModDisabled:
                newEnDisFormat = true;
                if (A_OnModDisabled == null)
                    A_OnModDisabled = function;
                else
                    ModConsole.Error($"SetupMod() error for <b>{ID}</b>. You have already created <b>OnModDisabled</b> function type.");
                break;
            case Setup.ModSettingsLoaded:
                newSettingsFormat = true;
                if (A_ModSettingsLoaded == null)
                    A_ModSettingsLoaded = function;
                else
                    ModConsole.Error($"SetupMod() error for <b>{ID}</b>. You have already created <b>ModSettingsLoaded</b> function type.");
                break;
            case Setup.ModSettings:
                newSettingsFormat = true;
                if (A_ModSettings == null)
                    A_ModSettings = function;
                else
                    ModConsole.Error($"SetupMod() error for <b>{ID}</b>. You have already created <b>ModSettings</b> function type.");
                break;
            default:
                ModConsole.Print("The hell happened here? That's impossible. (Don't bitwise Setup enum)");
                break;
        }

    }
}
#endif