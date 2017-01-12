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

		public static T Instance { get; private set; }

		public static bool InstanceEnabled 
		{ get { return Instance && Instance.WindowEnabled; } }

		public static void ShowInstance(bool show)
		{ if(Instance) Instance.Show(show); }

		public static void ToggleInstance()
		{ if(Instance) Instance.Show(!Instance.WindowEnabled); }

		readonly ActionDamper save_timer = new ActionDamper(10);

		public override void Awake()
		{
			base.Awake();
			if(Instance != null)
			{ Destroy(gameObject); return; }
			Instance = (T)this;
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
			if(this == Instance) Instance = null;
			base.OnDestroy();
		}

		void onGameStateSave(ConfigNode node) { SaveConfig(); }

		protected abstract void draw_gui();

		public virtual void OnGUI()
		{
			if(Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint) return;
			if(doShow && can_draw()) 
			{
				Styles.Init();
				draw_gui();
				save_timer.Run();
			}
			else UnlockControls();
		}
	}
}

