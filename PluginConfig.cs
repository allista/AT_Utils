//   PluginConfig.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System;
using System.IO;
using System.Reflection;

namespace AT_Utils
{
	public abstract class PluginConfig : ConfigNodeObject
	{
		#region From KSPPluginFramework
		static public string PluginFolder(string filename = "")
		{ return Utils.PathChain(AssemblyFolder, "..", filename); }
		//Combine the Location of the assembly and the provided string.
		//This means we can use relative or absolute paths.
		static public string PluginData(string filename = "")
		{ return Utils.PathChain(AssemblyFolder, "PluginData", AssemblyName, filename); }
		/// <summary>
		/// Name of the Assembly that is running this MonoBehaviour
		/// </summary>
		public static String AssemblyName
		{ get { return Assembly.GetExecutingAssembly().GetName().Name; } }
		/// <summary>
		/// Full Path of the executing Assembly
		/// </summary>
		public static String AssemblyLocation
		{ get { return Assembly.GetExecutingAssembly().Location; } }
		/// <summary>
		/// Folder containing the executing Assembly
		/// </summary>
		public static String AssemblyFolder
		{ get { return Path.GetDirectoryName(AssemblyLocation); } }
		#endregion

		public string DefaultFile { get { return PluginData(AssemblyName+".glob"); } }
		public bool DefaultFileExists { get { return File.Exists(DefaultFile); } }

		public static ConfigNode LoadNode(string filepath)
		{
			var node = ConfigNode.Load(filepath);
			if(node == null) Utils.Log("Unable to read "+filepath);
			return node;
		}

		public virtual void Init() {}

		public void Load(params string[] files)
		{
			//this allows for cascading options overriding
			foreach(var f in files)
			{
				var gnode = LoadNode(PluginData(f));
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
			{ Utils.Log("Error writing {0} file:\n{1}\n{2}", filename, ex.Message, ex.StackTrace); }
		}
		public void CreateDefaultFile() { Create(DefaultFile); }
	}
}

