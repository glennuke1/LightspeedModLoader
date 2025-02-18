using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LightspeedModLoader
{
    public class MSCLoaderModsLoader : MonoBehaviour
    {
        public List<MSCLoader.Mod> loadedMods = new List<MSCLoader.Mod>();

        internal List<MSCLoader.Mod> A_OnNewGameMods = new List<MSCLoader.Mod>();
        internal List<MSCLoader.Mod> A_OnMenuLoadMods = new List<MSCLoader.Mod>();
        internal List<MSCLoader.Mod> A_PreLoadMods = new List<MSCLoader.Mod>();
        internal List<MSCLoader.Mod> A_OnLoadMods = new List<MSCLoader.Mod>();
        internal List<MSCLoader.Mod> A_PostLoadMods = new List<MSCLoader.Mod>();
        internal List<MSCLoader.Mod> A_UpdateMods = new List<MSCLoader.Mod>();
        internal List<MSCLoader.Mod> A_FixedUpdateMods = new List<MSCLoader.Mod>();
        internal List<MSCLoader.Mod> A_OnGUIMods = new List<MSCLoader.Mod>();
        internal List<MSCLoader.Mod> A_OnSaveMods = new List<MSCLoader.Mod>();

        public void LoadMod(MSCLoader.Mod mod)
        {
            try
            {
                if (!loadedMods.Contains(mod))
                {
                    LML_Debug.Log("Loading MSCLoader mod: " + mod.ID);

                    mod.ModSetup();

                    loadedMods.Add(mod);
                }
            }
            catch (Exception ex)
            {
                LML_Debug.Log(ex.Message);
            }
        }

        public void LoadModsActions()
        {
            foreach (MSCLoader.Mod mod in loadedMods)
            {
                LML_Debug.Log("Loading MSCLoader Mod Actions/Methods for " + mod.ID);
                if (!mod.isDisabled)
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
                }
            }
        }
    }
}
