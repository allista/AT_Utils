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
	public abstract class PluginConfig : ConfigNodeObject
	{
		public string PluginFolder(string filename = "")
		{ return Utils.PathChain(AssemblyFolder, "..", filename); }

		public string PluginData(string filename = "")
		{ return Utils.PathChain(AssemblyFolder, "PluginData", AssemblyName, filename); }

		public String AssemblyName
		{ get { return Assembly.GetAssembly(GetType()).GetName().Name; } }

		public String AssemblyLocation
		{ get { return Assembly.GetAssembly(GetType()).Location; } }

		public String AssemblyFolder
		{ get { return Path.GetDirectoryName(AssemblyLocation); } }

		public string DefaultFile { get { return PluginData(AssemblyName+".glob"); } }
		public string DefaultOverride { get { return PluginFolder(AssemblyName+".user"); } }
		public bool   DefaultFileExists { get { return File.Exists(DefaultFile); } }
		public virtual List<string> AllConfigFiles { get { return new List<string>{DefaultFile, DefaultOverride}; } }

		public static ConfigNode LoadNode(string filepath)
		{
			var node = ConfigNode.Load(filepath);
			if(node == null) Utils.Log("Unable to read {}", filepath);
			return node;
		}

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

		public void Create(string filename)
		{
			var node = new ConfigNode();
			Save(node);
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(filename));
				using(var file = new StreamWriter(filename)) file.Write(node);
			}
			catch(Exception ex) 
			{ Utils.Log("Error writing {} file:\n{}\n{}", filename, ex.Message, ex.StackTrace); }
		}
		public void CreateDefaultFile() { Create(DefaultFile); }
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

