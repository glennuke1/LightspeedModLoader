using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		// Token: 0x040000A7 RID: 167
		internal Action A_OnNewGame;

		// Token: 0x040000A8 RID: 168
		internal Action A_PreLoad;

		// Token: 0x040000A9 RID: 169
		internal Action A_OnLoad;

		// Token: 0x040000AA RID: 170
		internal Action A_PostLoad;

		// Token: 0x040000AB RID: 171
		internal Action A_OnSave;

		// Token: 0x040000AC RID: 172
		internal Action A_OnGUI;

		// Token: 0x040000AD RID: 173
		internal Action A_Update;

		// Token: 0x040000AE RID: 174
		internal Action A_FixedUpdate;

		// Token: 0x040000AF RID: 175
		internal Action A_OnModEnabled;

		// Token: 0x040000B0 RID: 176
		internal Action A_OnModDisabled;

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
