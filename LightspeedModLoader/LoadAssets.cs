using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace LightspeedModLoader
{
    public static class LoadAssets
    {
		internal static List<string> assetNames = new List<string>();

		public static void MakeGameObjectPickable(GameObject go)
        {
            go.layer = LayerMask.NameToLayer("Parts");
            go.tag = "PART";
        }

		public static Texture2D LoadTexture(Mod mod, string fileName, bool normalMap = false)
		{
			string text = Path.Combine(ModLoader.GetModAssetsFolder(mod), fileName);
			if (!File.Exists(text))
			{
				throw new FileNotFoundException("<b>LoadTexture() Error:</b> File not found: " + text + Environment.NewLine, text);
			}
			string a = Path.GetExtension(text).ToLower();
			if (a == ".png" || a == ".jpg")
			{
				Texture2D texture2D = new Texture2D(1, 1);
				texture2D.LoadImage(File.ReadAllBytes(text));
				return texture2D;
			}
			if (a == ".dds")
			{
				return LoadAssets.LoadDDS(text);
			}
			if (a == ".tga")
			{
				return LoadAssets.LoadTGA(text);
			}
			throw new NotSupportedException("<b>LoadTexture() Error:</b> Texture not supported: " + fileName + Environment.NewLine);
		}

		public static AssetBundle LoadBundle(Mod mod, string bundleName)
		{
			string text = Path.Combine(ModLoader.GetModAssetsFolder(mod), bundleName);
			if (File.Exists(text))
			{
				LML_Debug.Log("Loading Asset: " + bundleName + "...");
				AssetBundle assetBundle = AssetBundle.CreateFromMemoryImmediate(File.ReadAllBytes(text));
				string[] allAssetNames = assetBundle.GetAllAssetNames();
				for (int i = 0; i < allAssetNames.Length; i++)
				{
					LoadAssets.assetNames.Add(Path.GetFileNameWithoutExtension(allAssetNames[i]));
				}
				return assetBundle;
			}
			throw new FileNotFoundException("<b>LoadBundle() Error:</b> File not found: <b>" + text + "</b>" + Environment.NewLine, bundleName);
		}

		public static AssetBundle LoadBundle(byte[] assetBundleFromResources)
		{
			if (assetBundleFromResources != null)
			{
				AssetBundle assetBundle = AssetBundle.CreateFromMemoryImmediate(assetBundleFromResources);
				string[] allAssetNames = assetBundle.GetAllAssetNames();
				for (int i = 0; i < allAssetNames.Length; i++)
				{
					LoadAssets.assetNames.Add(Path.GetFileNameWithoutExtension(allAssetNames[i]));
				}
				return assetBundle;
			}
			throw new Exception("<b>LoadBundle() Error:</b> Resource doesn't exists" + Environment.NewLine);
		}

		public static AssetBundle LoadBundle(string assetBundleEmbeddedResources)
		{
			AssetBundle result;
			using (Stream manifestResourceStream = Assembly.GetCallingAssembly().GetManifestResourceStream(assetBundleEmbeddedResources))
			{
				if (manifestResourceStream == null)
				{
					throw new Exception("<b>LoadBundle() Error:</b> Resource doesn't exists" + Environment.NewLine);
				}
				byte[] array = new byte[manifestResourceStream.Length];
				manifestResourceStream.Read(array, 0, array.Length);
				AssetBundle assetBundle = AssetBundle.CreateFromMemoryImmediate(array);
				string[] allAssetNames = assetBundle.GetAllAssetNames();
				for (int i = 0; i < allAssetNames.Length; i++)
				{
					LoadAssets.assetNames.Add(Path.GetFileNameWithoutExtension(allAssetNames[i]));
				}
				result = assetBundle;
			}
			return result;
		}

		internal static Texture2D LoadTGA(string fileName)
		{
			Texture2D result;
			using (FileStream fileStream = File.OpenRead(fileName))
			{
				result = LoadAssets.LoadTGA(fileStream);
			}
			return result;
		}

		internal static Texture2D LoadDDS(string ddsPath)
		{
			Texture2D result;
			try
			{
				byte[] array = File.ReadAllBytes(ddsPath);
				if (array[4] != 124)
				{
					throw new Exception("Invalid DDS DXTn texture. Unable to read");
				}
				int height = (int)array[13] * 256 + (int)array[12];
				int width = (int)array[17] * 256 + (int)array[16];
				byte b = array[87];
				TextureFormat format = TextureFormat.DXT5;
				if (b == 49)
				{
					format = TextureFormat.DXT1;
				}
				if (b == 53)
				{
					format = TextureFormat.DXT5;
				}
				int num = 128;
				byte[] array2 = new byte[array.Length - num];
				Buffer.BlockCopy(array, num, array2, 0, array.Length - num);
				FileInfo fileInfo = new FileInfo(ddsPath);
				Texture2D texture2D = new Texture2D(width, height, format, false);
				texture2D.LoadRawTextureData(array2);
				texture2D.Apply();
				texture2D.name = fileInfo.Name;
				result = texture2D;
			}
			catch
			{
				LML_Debug.Log("<b>LoadTexture() Error:</b>" + Environment.NewLine + "Error: Could not load DDS texture");
				result = new Texture2D(8, 8);
			}
			return result;
		}

		private static Texture2D LoadTGA(Stream TGAStream)
		{
			Texture2D result;
			using (BinaryReader binaryReader = new BinaryReader(TGAStream))
			{
				binaryReader.BaseStream.Seek(12L, SeekOrigin.Begin);
				short num = binaryReader.ReadInt16();
				short num2 = binaryReader.ReadInt16();
				int num3 = (int)binaryReader.ReadByte();
				binaryReader.BaseStream.Seek(1L, SeekOrigin.Current);
				Texture2D texture2D = new Texture2D((int)num, (int)num2);
				Color32[] array = new Color32[(int)(num * num2)];
				if (num3 == 32)
				{
					for (int i = 0; i < (int)(num * num2); i++)
					{
						byte b = binaryReader.ReadByte();
						byte g = binaryReader.ReadByte();
						byte r = binaryReader.ReadByte();
						byte a = binaryReader.ReadByte();
						array[i] = new Color32(r, g, b, a);
					}
				}
				else
				{
					if (num3 != 24)
					{
						throw new Exception("<b>LoadTexture() Error:</b> TGA texture is not 32 or 24 bit depth." + Environment.NewLine);
					}
					for (int j = 0; j < (int)(num * num2); j++)
					{
						byte b2 = binaryReader.ReadByte();
						byte g2 = binaryReader.ReadByte();
						byte r2 = binaryReader.ReadByte();
						array[j] = new Color32(r2, g2, b2, 1);
					}
				}
				texture2D.SetPixels32(array);
				texture2D.Apply();
				result = texture2D;
			}
			return result;
		}
	}
}
