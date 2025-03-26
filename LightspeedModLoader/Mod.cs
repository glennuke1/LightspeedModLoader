using System;

namespace LightspeedModLoader
{
    public abstract class Mod
    {
        public abstract string ID { get; }

        public virtual string Name
        {
            get
            {
                return this.ID;
            }
        }

        public abstract string Version { get; }

        public abstract string Author { get; }

        public virtual byte[] Icon { get; set; }

        public virtual string Description { get; set; }

        public virtual void ModSetup()
        {
        }

        public bool isDisabled { get; set; }

        public virtual bool LoadInMenu
        {
            get
            {
                return false;
            }
        }

        internal Action A_OnMenuLoad;

        internal Action A_OnNewGame;

        internal Action A_PreLoad;

        internal Action A_OnLoad;

        internal Action A_PostLoad;

        internal Action A_OnSave;

        internal Action A_OnGUI;

        internal Action A_Update;

        internal Action A_FixedUpdate;

        public Action A_OnModEnabled;

        public Action A_OnModDisabled;

        internal Action A_ModSettings;

        public enum Setup
        {
            OnNewGame,
            OnMenuLoad,
            PreLoad,
            OnLoad,
            PostLoad,
            OnSave,
            OnGUI,
            Update,
            FixedUpdate,
            OnModEnabled,
            OnModDisabled,
            ModSettings,
        }

        public void SetupFunction(Mod.Setup functionType, Action function)
        {
            switch (functionType)
            {
                case Mod.Setup.OnNewGame:
                    if (this.A_OnNewGame == null)
                    {
                        this.A_OnNewGame = function;
                        return;
                    }
                    LML_Debug.Log("SetupMod() Log for <b>" + this.ID + "</b>. You already created <b>OnNewGame</b> function type.");
                    return;
                case Mod.Setup.OnMenuLoad:
                    if (this.A_OnMenuLoad == null)
                    {
                        this.A_OnMenuLoad = function;
                        return;
                    }
                    LML_Debug.Log("SetupMod() Log for <b>" + this.ID + "</b>. You already created <b>OnMenuLoad</b> function type.");
                    return;
                case Mod.Setup.PreLoad:
                    if (this.A_PreLoad == null)
                    {
                        this.A_PreLoad = function;
                        return;
                    }
                    LML_Debug.Log("SetupMod() Log for <b>" + this.ID + "</b>. You already created <b>PreLoad</b> function type.");
                    return;
                case Mod.Setup.OnLoad:
                    if (this.A_OnLoad == null)
                    {
                        this.A_OnLoad = function;
                        return;
                    }
                    LML_Debug.Log("SetupMod() Log for <b>" + this.ID + "</b>. You already created <b>OnLoad</b> function type.");
                    return;
                case Mod.Setup.PostLoad:
                    if (this.A_PostLoad == null)
                    {
                        this.A_PostLoad = function;
                        return;
                    }
                    LML_Debug.Log("SetupMod() Log for <b>" + this.ID + "</b>. You already created <b>PostLoad</b> function type.");
                    return;
                case Mod.Setup.OnSave:
                    if (this.A_OnSave == null)
                    {
                        this.A_OnSave = function;
                        return;
                    }
                    LML_Debug.Log("SetupMod() Log for <b>" + this.ID + "</b>. You already created <b>OnSave</b> function type.");
                    return;
                case Mod.Setup.OnGUI:
                    if (this.A_OnGUI == null)
                    {
                        this.A_OnGUI = function;
                        return;
                    }
                    LML_Debug.Log("SetupMod() Log for <b>" + this.ID + "</b>. You already created <b>OnGUI</b> function type.");
                    return;
                case Mod.Setup.Update:
                    if (this.A_Update == null)
                    {
                        this.A_Update = function;
                        return;
                    }
                    LML_Debug.Log("SetupMod() Log for <b>" + this.ID + "</b>. You already created <b>Update</b> function type.");
                    return;
                case Mod.Setup.FixedUpdate:
                    if (this.A_FixedUpdate == null)
                    {
                        this.A_FixedUpdate = function;
                        return;
                    }
                    LML_Debug.Log("SetupMod() Log for <b>" + this.ID + "</b>. You already created <b>FixedUpdate</b> function type.");
                    return;
                case Mod.Setup.OnModEnabled:
                    if (this.A_OnModEnabled == null)
                    {
                        this.A_OnModEnabled = function;
                        return;
                    }
                    LML_Debug.Log("SetupMod() Log for <b>" + this.ID + "</b>. You already created <b>OnModEnabled</b> function type.");
                    return;
                case Mod.Setup.OnModDisabled:
                    if (this.A_OnModDisabled == null)
                    {
                        this.A_OnModDisabled = function;
                        return;
                    }
                    LML_Debug.Log("SetupMod() Log for <b>" + this.ID + "</b>. You already created <b>OnModDisabled</b> function type.");
                    return;
                case Mod.Setup.ModSettings:
                    if (this.A_ModSettings == null)
                    {
                        this.A_ModSettings = function;
                        return;
                    }
                    LML_Debug.Log("SetupMod() Log for <b>" + this.ID + "</b>. You already created <b>ModSettings</b> function type.");
                    return;
                default:
                    LML_Debug.Log("???");
                    return;
            }
        }
    }
}
