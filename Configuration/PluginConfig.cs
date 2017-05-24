//   PluginConfig.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace AT_Utils
{
	public abstract class CustomConfig : ConfigNodeObject
	{
		public static string GameDataRoot
		{ get { return Path.Combine(KSPUtil.ApplicationRootPath, "GameData"); } }

		public static string GameDataFolder(params string[] path)
		{ return Path.Combine(GameDataRoot, Utils.PathChain(path)); }

		public string AssemblyName
		{ get { return Assembly.GetAssembly(GetType()).GetName().Name; } }

		public string AssemblyLocation
		{ get { return Assembly.GetAssembly(GetType()).Location; } }

		public string AssemblyFolder
		{ get { return Path.GetDirectoryName(AssemblyLocation); } }

		public string PluginFolder(string filename = "")
		{ return Utils.PathChain(AssemblyFolder, "..", filename); }

		public string PluginData(string filename = "")
		{ return Utils.PathChain(AssemblyFolder, "PluginData", AssemblyName, filename); }


		public static ConfigNode LoadNode(string filepath, bool with_message = false)
		{
			var node = ConfigNode.Load(filepath);
			if(node == null && with_message) 
				Utils.Log("Unable to read {}", filepath);
			return node;
		}

		public static bool SaveNode(ConfigNode node, string filepath)
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(filepath));
				node.Save(filepath);
				return true;
			}
			catch(Exception ex) 
			{ 
				Utils.Log("Error writing {} file:\n{}", filepath, ex); 
				return false;
			}
		}

		public bool Create(string filepath)
		{
			var node = new ConfigNode();
			Save(node);
			return SaveNode(node, filepath);
		}
	}

	public abstract class PluginConfig : CustomConfig
	{
		public string DefaultFile { get { return PluginData(AssemblyName+".glob"); } }
		public string DefaultOverride { get { return PluginFolder(AssemblyName+".user"); } }
		public bool   DefaultFileExists { get { return File.Exists(DefaultFile); } }
		public virtual List<string> AllConfigFiles { get { return new List<string>{DefaultFile, DefaultOverride}; } }

		public virtual void Init() {}

		public void Load(params string[] files)
		{
			//this allows for cascading options overriding
			foreach(var f in files)
			{
				var gnode = LoadNode(f);
				if(gnode != null) Load(gnode);
			}
			Init();
		}
		public void LoadDefaultFile() { Load(DefaultFile); }
		public void CreateDefaultFile() { Create(DefaultFile); }
        public void CreateDefaultOverride() { Create(DefaultOverride); }
	}


	public abstract class PluginGlobals<T> : PluginConfig where T : PluginConfig, new()
	{
		static T instance { get; set; }
		public static T Instance 
		{
			get
			{
				if(instance == null) Load();
				return instance;
			}
		}

		public static void Load()
		{
			instance = new T();
			if(!instance.DefaultFileExists)
				instance.CreateDefaultFile();
			instance.Load(instance.AllConfigFiles.ToArray());
		}
	}

	public class AT_UtilsGlobals : PluginGlobals<AT_UtilsGlobals>
	{
		[Persistent] public Styles.Config StylesConfig = new Styles.Config();
	}
}

