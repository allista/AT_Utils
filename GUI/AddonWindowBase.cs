//   AddonWindowBase.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using System.Reflection;
using UnityEngine;

namespace AT_Utils
{
	abstract public class AddonWindowBase<T> : GUIWindowBase where T : AddonWindowBase<T>
	{
		public string Title;

		protected static T instance { get; private set; }
		public static bool window_enabled { get; protected set; } = false;
		public static bool do_show { get { return window_enabled && HUD_enabled; } }

		readonly ActionDamper save_timer = new ActionDamper(10);

		protected virtual void show(bool show)
		{
			window_enabled = show;
			update_content();
		}

		public static void Show(bool show) 
		{ if(instance != null) instance.show(show); }

		public static void Toggle() 
		{ 
			if(instance != null)
				instance.show(!window_enabled);
		}

		public override void Awake()
		{
			base.Awake();
			if(instance != null)
			{ Destroy(gameObject); return; }
			instance = (T)this;
			LoadConfig();
			var assembly = Assembly.GetAssembly(typeof(T)).GetName();
			Title = string.Concat(assembly.Name, " - ", assembly.Version);
			GameEvents.onGameStateSave.Add(onGameStateSave);
			save_timer.action = SaveConfig;
		}

		public override void OnDestroy()
		{
			SaveConfig();
			GameEvents.onGameStateSave.Remove(onGameStateSave);
			if(this == instance) instance = null;
			base.OnDestroy();
		}

		void onGameStateSave(ConfigNode node) { SaveConfig(); }

		//settings
		public override void LoadConfig()
		{
			base.LoadConfig();
			window_enabled = GetConfigValue<bool>(Utils.PropertyName(new {window_enabled}), window_enabled);
		}

		public override void SaveConfig()
		{
			SetConfigValue(Utils.PropertyName(new {window_enabled}), window_enabled);
			base.SaveConfig();
		}

		protected abstract bool can_draw();
		protected abstract void draw_gui();

		public virtual void OnGUI()
		{
			if(Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint) return;
			if(do_show && can_draw()) 
			{
				Styles.Init();
				draw_gui();
				save_timer.Run();
			}
			else UnlockControls();
		}
	}
}

