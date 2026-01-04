#if !Mini
using LightspeedModLoader;
using System;
using System.IO;
using System.Linq;

namespace MSCLoader;

/// <summary>
/// List of possible scenes
/// </summary>
public enum CurrentScene
{
    /// <summary>
    /// Main Menu
    /// </summary>
    MainMenu,
    /// <summary>
    /// Game Scene
    /// </summary>
    Game,
    /// <summary>
    /// Intro for new game
    /// </summary>
    NewGameIntro,
    /// <summary>
    /// End game scene
    /// </summary>
    Ending
}

public partial class ModLoader
{
    /// <summary>b
    /// Current scene
    /// </summary>
    public static CurrentScene CurrentScene { get; set; }

    /// <summary>
    /// Check if steam is present
    /// </summary>
    /// <returns>Valid steam detected.</returns>
    public static bool CheckSteam()
    {
        if (!string.IsNullOrEmpty(steamID) && steamID != "0")
            return true;
        else
            return false;
    }

    /// <summary>
    /// Check if steam release is from experimental branch
    /// </summary>
    /// <returns>Experimental detected.</returns>
    public static bool CheckIfExperimental()
    {
#if !Mini
        if (!CheckSteam())
        {
            System.Console.WriteLine("Cannot check if the experimental branch is being used or not because no valid steam installation was detected");
            return false;
        }
        bool ret = Steamworks.SteamApps.GetCurrentBetaName(out string Name, 128);
        if (ret)
        {
            if (!Name.StartsWith("default_")) //default is NOT experimental branch
                return true;
        }
#endif
        return false;
    }

    /// <summary>
    /// Get Current Game Scene
    /// </summary>
    /// <returns>CurrentScene enum</returns>
    public static CurrentScene GetCurrentScene()
    {
        return CurrentScene;
    }

    /// <summary>
    /// Get Mod class of modID
    /// </summary>
    /// <param name="modID">Mod ID of other mod to check (Case sensitive)</param>
    /// <param name="ignoreEnabled">Include disabled mods [yes it's DUMB proloader variable name]</param>
    /// <returns>Mod class</returns>
    [Obsolete("Proloader BS", true)]
    public static Mod GetMod(string modID, bool ignoreEnabled = false)
    {
        return LightspeedModLoader.ModLoader.GetMod(modID) as Mod;
    }

    internal static Mod GetModByID(string modID, bool includeDisabled = false)
    {
        Mod m = LoadedMods.Where(x => x.ID.Equals(modID)).FirstOrDefault();
        if (includeDisabled) return m; //if include disabled is true then just return (can be null)
        if (!m.isDisabled) return m; //if include disabled is false we go here to check if mod is not disabled and return it.
        return null; //null if any above if is false
    }
    /// <summary>
    /// Check if Reference of specified AssemblyID is present
    /// </summary>
    /// <param name="AssemblyID">AssemblyID of reference to check (Case sensitive)</param>
    /// <returns>true if AssemblyID is present</returns>
    public static bool IsReferencePresent(string AssemblyID)
    {
        return LightspeedModLoader.ModLoader.IsReferencePresent(AssemblyID);
    }

    /// <summary>
    /// Check if other ModID is present and enabled
    /// </summary>
    /// <param name="ModID">Mod ID of other mod to check (Case sensitive)</param>
    /// <returns>true if mod ID is present</returns>
    public static bool IsModPresent(string ModID)
    {
        return LightspeedModLoader.ModLoader.IsModPresent(ModID);
    }

    /// <summary>
    /// Check if other ModID is present
    /// </summary>
    /// <param name="ModID">Mod ID of other mod to check (Case sensitive)</param>
    /// <param name="includeDisabled">Include disabled mods</param>
    /// <returns>true if mod ID is present</returns>
    public static bool IsModPresent(string ModID, bool includeDisabled)
    {
        return IsModPresent(ModID);
    }
    /// <summary>
    /// [compatibility only]
    /// </summary>
    /// <param name="mod">Your mod Class.</param>
    /// <param name="create">DOES NOTHING</param>
    /// <returns></returns>
    [Obsolete("This overload is compatibility only", true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string GetModSettingsFolder(Mod mod, bool create = true) => LightspeedModLoader.ModLoader.GetModConfigFolder(mod);
    /// <summary>
    /// Mod settings folder, use this if you want save something. 
    /// </summary>
    /// <returns>Path to your mod settings folder</returns>
    /// <param name="mod">Your mod Class.</param>
    public static string GetModSettingsFolder(Mod mod) => LightspeedModLoader.ModLoader.GetModConfigFolder(mod);

    /// <summary>
    /// [Obsolete] Change to GetModSettingsFolder()
    /// </summary>
    /// <returns>Path to your mod config folder</returns>
    /// <param name="mod">Your mod Class.</param>
    [Obsolete("Rename to GetModSettingsFolder(), config is old unused name", true)]
    public static string GetModConfigFolder(Mod mod)
    {
        return LightspeedModLoader.ModLoader.GetModConfigFolder(mod);
    }

    /// <summary>
    /// Mod assets folder, use this if you want load custom content. 
    /// </summary>
    /// <returns>Path to your mod assets folder</returns>
    /// <param name="mod">Your mod Class.</param>
    public static string GetModAssetsFolder(Mod mod)
    {
        return LightspeedModLoader.ModLoader.GetModAssetsFolder(mod);
    }

    /// <summary>
    /// [compatibility only]
    /// </summary>
    /// <param name="mod">Your mod Class.</param>
    /// <param name="create">DOES NOTHING</param>
    /// <returns></returns>
    [Obsolete("This overload is compatibility only", true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string GetModAssetsFolder(Mod mod, bool create = true) => GetModAssetsFolder(mod);
}
#endif