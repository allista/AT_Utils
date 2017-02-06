//   SimpleTextureSwitcher.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AT_Utils
{
	public class TextureSwitcherServer : PartModule
	{
		/// <summary>
		/// The folder in which textures are located. 
		/// Relative to GameData folder.
		/// </summary>
		[KSPField] public string RootFolder = string.Empty;

		/// <summary>
		/// The names of the game objects which main texture should be replaced.
		/// Divided by commas. May be regular expressions.
		/// </summary>
		[KSPField] public string AffectedObjects = string.Empty;
		List<Regex> affected_objects;
		readonly List<Renderer> renderers = new List<Renderer>();

		/// <summary>
		/// Names of the textures to choose from. 
		/// </summary>
		[KSPField] public string Textures = string.Empty;
		protected readonly List<string> textures = new List<string>();

		/// <summary>
		/// The texture currently in use.
		/// </summary>
		[KSPField(isPersistant = true)]
		public string CurrentTexture = string.Empty;

		public void NextTexture()
		{
			if(textures.Count < 2) return;
			CurrentTexture = textures.Next(CurrentTexture);
			set_texture();
		}

		public void PrevTexture()
		{
			if(textures.Count < 2) return;
			CurrentTexture = textures.Prev(CurrentTexture);
			set_texture();
		}

		public void SetTexture(string texture)
		{
			var prev_texture = CurrentTexture;
			CurrentTexture = texture;
			if(!set_texture())
				CurrentTexture = prev_texture;
		}

		public void SetTexture(int index)
		{
			if(textures.Count == 0) return;
			index = index % textures.Count;
			CurrentTexture = textures[index];
			set_texture();
		}

		public override void OnStart(StartState state)
		{
			//prepare root folder path
			if(!string.IsNullOrEmpty(RootFolder))
				RootFolder = RootFolder.TrimEnd('/')+"/";
			setup_material();
			setup_textures();
			set_texture();
		}

		protected void setup_material()
		{
			renderers.Clear();
			if(string.IsNullOrEmpty(AffectedObjects)) return;
			affected_objects = Utils.ParseLine(AffectedObjects, Utils.Comma).Select(s => new Regex(s)).ToList();
			foreach(var r in part.FindModelComponents<Renderer>())
			{
				if(r == null || !r.enabled) continue;
				if(affected_objects.Any(exp => exp.IsMatch(r.name.Replace("(Instance)", "").Trim())))
					renderers.Add(r);
			}
			if(renderers.Count == 0)
				this.Log("None of the following objects were found in the model: {}", AffectedObjects);
		}

		protected void setup_textures()
		{
			textures.Clear();
			if(renderers.Count == 0 || string.IsNullOrEmpty(Textures)) return;
			//parse textures
			foreach(var t in Utils.ParseLine(Textures, Utils.Comma))
			{
				var tex = RootFolder+t;
				if(GameDatabase.Instance.ExistsTexture(tex))
				{
					try { textures.Add(t); }
					catch { this.Log("Duplicate texture in the replacement list: {}", t); }
				}
				else this.Log("No such texture: {}", tex);
			}
			if(textures.Count > 0 && 
			   (CurrentTexture == string.Empty || 
			    !textures.Contains(CurrentTexture)))
				CurrentTexture = textures[0];
		}

		protected bool set_texture()
		{
			var texture = GameDatabase.Instance.GetTexture(RootFolder+CurrentTexture, false);
			if(texture == null) return false;
			renderers.ForEach(r => r.material.mainTexture = texture);
			return true;
		}
	}

	public class SimpleTextureSwitcher : TextureSwitcherServer
	{
		/// <summary>
		/// The texture currently in use. Part menu display.
		/// </summary>
		[KSPField(guiActiveEditor = true, guiName = "Texture")]
		[UI_ChooseOption(scene = UI_Scene.Editor)]
		public string CurrentTextureDisplay = string.Empty;

		public override void OnStart(StartState state)
		{
			base.OnStart(state);
			//setup UI
			if(state == StartState.Editor && textures.Count > 1)
			{
				var _textures = textures.ToArray();
				Utils.SetupChooser(_textures, _textures, Fields["CurrentTextureDisplay"]);
				Utils.EnableField(Fields["CurrentTextureDisplay"]);
				CurrentTextureDisplay = CurrentTexture;
				StartCoroutine(slow_update());
			}
			else Utils.EnableField(Fields["CurrentTextureDisplay"], false);
		}

		IEnumerator<YieldInstruction> slow_update()
		{
			while(true)
			{
				if(textures.Count > 1 &&
				   CurrentTextureDisplay != CurrentTexture)
				{
					CurrentTexture = CurrentTextureDisplay;
					set_texture();
				}
				yield return new WaitForSeconds(0.1f);
			}
		}
	}
}

