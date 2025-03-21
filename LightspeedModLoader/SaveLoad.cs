using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace LightspeedModLoader
{
    [System.Serializable]
    public class ModSaveData
    {
        public string modID;
        public bool isDisabled;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<ModSaveData> modSaveDatas = new List<ModSaveData>();
    }

    public class SaveLoad : MonoBehaviour
    {
        public void Save()
        {
            SaveData saveData = new SaveData();

            List<ModSaveData> modSaveDatas = new List<ModSaveData>();
            foreach (Mod mod in ModLoader.Instance.loadedMods)
            {
                ModSaveData modSaveData = new ModSaveData
                {
                    modID = mod.ID,
                    isDisabled = mod.isDisabled
                };
                modSaveDatas.Add(modSaveData);
            }

            foreach (MSCLoader.Mod mod in ModLoader.Instance.mscloadermodsloader.loadedMods)
            {
                ModSaveData modSaveData = new ModSaveData
                {
                    modID = mod.ID,
                    isDisabled = mod.isDisabled
                };
                modSaveDatas.Add(modSaveData);
            }

            saveData.modSaveDatas = modSaveDatas;

            string json = JsonConvert.SerializeObject(saveData);

            string path = Path.Combine(Application.dataPath, "LML_Save.json");
            File.WriteAllText(path, json);
        }

        public void Load()
        {
            string path = Path.Combine(Application.dataPath, "LML_Save.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                SaveData saveData = JsonConvert.DeserializeObject<SaveData>(json);

                foreach (ModSaveData modSaveData in saveData.modSaveDatas)
                {
                    foreach (Mod mod in ModLoader.Instance.loadedMods)
                    {
                        if (mod.ID == modSaveData.modID)
                        {
                            mod.isDisabled = modSaveData.isDisabled;
                        }
                    }

                    foreach (MSCLoader.Mod mod in ModLoader.Instance.mscloadermodsloader.loadedMods)
                    {
                        if (mod.ID == modSaveData.modID)
                        {
                            mod.isDisabled = modSaveData.isDisabled;
                        }
                    }
                }
            }
        }
    }
}
