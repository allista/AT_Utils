//   AddonWindowBase.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri

using System.Reflection;
using UnityEngine;
using KSP.IO;

namespace AT_Utils
{
	public static class TooltipManager
	{
		static string tooltip = "";

		//adapted from blizzy's Toolbar
		public static void DrawToolTip(Rect window) 
		{
			if(Event.current.type == EventType.Repaint)
				tooltip = GUI.tooltip.Trim();
			if(string.IsNullOrEmpty(tooltip)) return;
			var mousePos = Utils.GetMousePosition(window);
			var size = Styles.tooltip.CalcSize(new GUIContent(tooltip));
			var rect = new Rect(mousePos.x, mousePos.y + 20, size.x, size.y);
			Rect orig = rect;
			rect = rect.clampToWindow(window);
			//clamping moved the tooltip up -> reposition above mouse cursor
			if(rect.y < orig.y) 
			{
				rect.y = mousePos.y - size.y - 5;
				rect = rect.clampToScreen();
			}
			//clamping moved the tooltip left -> reposition lefto of the mouse cursor
			if(rect.x < orig.x)
			{
				rect.x = mousePos.x - size.x - 5;
				rect = rect.clampToScreen();
			}
			GUI.Label(rect, tooltip, Styles.tooltip);
		}
	}

	abstract public class GUIWindowBase : MonoBehaviour
	{
		public static bool HUD_enabled { get; protected set; } = true;

		public Rect WindowPos = new Rect(100, 50, Screen.width/4, Screen.height/4);
		protected static Rect drag_handle = new Rect(0,0, 10000, 20);
		protected int  width = 550, height = 100;
		protected PluginConfiguration GUI_CFG;
		public string LockName { get; protected set; }

		protected virtual void onShowUI() { HUD_enabled = true; update_content(); }
		protected virtual void onHideUI() { HUD_enabled = false; update_content(); }

		protected virtual void update_content() {}

		void CreateConfig()
		{
			if(GUI_CFG != null) return;
			var create_for_type = typeof(PluginConfiguration).GetMethod("CreateForType");
			create_for_type = create_for_type.MakeGenericMethod(new [] { GetType() });
			GUI_CFG = create_for_type.Invoke(null, new object[] { null }) as PluginConfiguration;
		}

		protected GUIWindowBase()
		{
			CreateConfig();
			LockName = GetType().FullName;
		}

		public virtual void Awake()
		{
			GameEvents.onHideUI.Add(onHideUI);
			GameEvents.onShowUI.Add(onShowUI);
		}

		public virtual void OnDestroy()
		{
			GameEvents.onHideUI.Remove(onHideUI);
			GameEvents.onShowUI.Remove(onShowUI);
		}

		//settings
		protected string mangleName(string name) { return GetType().Name+"-"+name; }

		protected void SetConfigValue(string key, object value)
		{ GUI_CFG.SetValue(mangleName(key), value); }

		protected V GetConfigValue<V>(string key, V _default)
		{ return GUI_CFG.GetValue<V>(mangleName(key), _default); }

		public virtual void LoadConfig()
		{
			GUI_CFG.load();
			WindowPos = GetConfigValue<Rect>(Utils.PropertyName(new {WindowPos}), new Rect(100, 50, width, height));
		}

		public virtual void SaveConfig()
		{
			SetConfigValue(Utils.PropertyName(new {WindowPos}), WindowPos);
			GUI_CFG.save();
		}

		public virtual void UnlockControls()
		{ Utils.LockIfMouseOver(LockName, WindowPos, false); }

		public virtual void LockControls()
		{ Utils.LockIfMouseOver(LockName, WindowPos); }

		public static void TooltipsAndDragWindow(Rect rect)
		{
			TooltipManager.DrawToolTip(rect);
			GUI.DragWindow(drag_handle);
		}
	}

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

