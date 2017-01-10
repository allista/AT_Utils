//   AddonWindowBase.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using KSP.IO;

namespace AT_Utils
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true)]
	public class ConfigOption : Attribute {}

	abstract public class GUIWindowBase : MonoBehaviour
	{
		public static bool HUD_enabled { get; protected set; } = true;
		public static readonly Rect ScreenRect = new Rect(0,0,Screen.width,Screen.height);
		protected static Rect drag_handle = new Rect(0,0, 10000, 20);

		[ConfigOption]
		public Rect WindowPos = new Rect(100, 50, Screen.width/4, Screen.height/4);
		protected int width = 10, height = 10;

		#region Subwindows
		public string Name = "";
		List<GUIWindowBase> _subwindows;
		protected List<GUIWindowBase> subwindows 
		{
			get
			{
				if(_subwindows == null)
					_subwindows = GetType().GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)
						.Where(fi => typeof(GUIWindowBase).IsAssignableFrom(fi.FieldType))
						.Select(fi => fi.GetValue(this) as GUIWindowBase)
						.ToList();
				return _subwindows;
			}
		}

		void init_subwindows()
		{
			foreach(var sw in GetType()
			        .GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)
			        .Where(fi => typeof(GUIWindowBase).IsAssignableFrom(fi.FieldType)))
			{
				var obj = gameObject.AddComponent(sw.FieldType) as GUIWindowBase;
				obj.Name = Name+"-"+sw.Name;
				obj.SetConfig(GUI_CFG);
				sw.SetValue(this, obj);
			}
		}
		#endregion

		#region Config
		protected static Dictionary<string, PluginConfiguration> configs = new Dictionary<string, PluginConfiguration>();
		protected PluginConfiguration GUI_CFG;

		public void SetConfig(PluginConfiguration cfg)
		{
			if(cfg == null) return;
			GUI_CFG = cfg;
			subwindows.ForEach(sw => sw.SetConfig(cfg));
		}

		void create_config()
		{
			var config_path = AssemblyLoader.GetPathByType(GetType());
			if(configs.TryGetValue(config_path, out GUI_CFG)) return;
			var create_for_type = typeof(PluginConfiguration).GetMethod("CreateForType");
			create_for_type = create_for_type.MakeGenericMethod(new [] { GetType() });
			GUI_CFG = create_for_type.Invoke(null, new object[] { null }) as PluginConfiguration;
			configs[config_path] = GUI_CFG;
		}

		protected string mangleName(string name) { return Name+"-"+name; }

		protected void SetConfigValue(string key, object value)
		{ GUI_CFG.SetValue(mangleName(key), value); }

		protected V GetConfigValue<V>(string key, V _default)
		{ return GUI_CFG.GetValue<V>(mangleName(key), _default); }

		public virtual void LoadConfig()
		{
			GUI_CFG.load();
			var T = GetType();
			var get_val = T.GetMethod("GetConfigValue", BindingFlags.NonPublic|BindingFlags.Instance);
			var options = T.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy);
			foreach(var opt_fi in options)
			{
//				Utils.Log("Load: {}[{}].{} = {}", T, GetInstanceID(), opt_fi.Name, opt_fi.GetValue(this));//debug
				if(typeof(GUIWindowBase).IsAssignableFrom(opt_fi.FieldType))
				{
					try 
					{
						var opt = opt_fi.GetValue(this) as GUIWindowBase;
						opt.LoadConfig();
					} catch(NullReferenceException) {}
					continue;
				}
				if(opt_fi.GetCustomAttributes(typeof(ConfigOption), true).Length == 0) continue;
				var get_val_gen = get_val.MakeGenericMethod(new []{opt_fi.FieldType});
				var val = get_val_gen.Invoke(this, new []{opt_fi.Name, Activator.CreateInstance(opt_fi.FieldType)});
				if(val != null) opt_fi.SetValue(this, val);
			}
		}

		public virtual void SaveConfig()
		{
			var options = GetType().GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy);
			foreach(var opt_fi in options)
			{
//				Utils.Log("Save: {}[{}].{} = {}", GetType(), GetInstanceID(), opt_fi.Name, opt_fi.GetValue(this));//debug
				if(typeof(GUIWindowBase).IsAssignableFrom(opt_fi.FieldType))
				{
					try
					{
						var opt = opt_fi.GetValue(this) as GUIWindowBase;
						opt.SaveConfig();
					} catch(NullReferenceException) {}
					continue;
				}
				if(opt_fi.GetCustomAttributes(typeof(ConfigOption), true).Length == 0) continue;
				var val = opt_fi.GetValue(this);
				if(val != null) SetConfigValue(opt_fi.Name, val);
			}
			GUI_CFG.save();
		}
		#endregion

		protected virtual void onShowUI() { HUD_enabled = true; update_content(); }
		protected virtual void onHideUI() { HUD_enabled = false; update_content(); }

		protected virtual void update_content() {}

		public virtual void Awake()
		{
			LockName = GetType().FullName+GetInstanceID();
			Name = GetType().Name;
			create_config();
			init_subwindows();
			GameEvents.onHideUI.Add(onHideUI);
			GameEvents.onShowUI.Add(onShowUI);
		}

		public virtual void OnDestroy()
		{
			subwindows.ForEach(Destroy);
			GameEvents.onHideUI.Remove(onHideUI);
			GameEvents.onShowUI.Remove(onShowUI);
			UnlockControls();
		}

		#region GUI Lock
		public string LockName { get; protected set; }

		public virtual void UnlockControls()
		{ Utils.LockIfMouseOver(LockName, WindowPos, false); }

		public virtual void LockControls()
		{ Utils.LockIfMouseOver(LockName, WindowPos); }

		public static void TooltipsAndDragWindow()
		{
			TooltipManager.GetTooltip();
			GUI.DragWindow(drag_handle);
		}
		#endregion
	}


	//probably not needed after all
	public class DelayedSwitch : IEnumerator<YieldInstruction>
	{
		readonly YieldInstruction wait_for = new WaitForSeconds(1);
		public bool On { get; private set; }
		bool new_state;
		int ticks = -1;

		public DelayedSwitch(YieldInstruction wait_for = null)
		{ this.wait_for = wait_for; }

		public static implicit operator bool(DelayedSwitch sw) { return sw.On; }

		public DelayedSwitch Set(bool state) 
		{
			new_state = state;
			ticks = 1;
			return this;
		}

		public DelayedSwitch Toggle()
		{
			new_state = !On;
			ticks = 1;
			return this;
		}

		public bool MoveNext()
		{
			if(ticks-- >= 0) return true;
			On = new_state;
			return false;
		}

		public void Reset() { ticks = 1; }

		public void Dispose() {}

		public YieldInstruction Current
		{ get { return wait_for; } }

		object System.Collections.IEnumerator.Current
		{ get { return Current; } }
	}
}

