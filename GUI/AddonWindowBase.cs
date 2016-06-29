//   AddonWindowBase.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri

using System;
using System.Reflection;
using UnityEngine;
using KSP.IO;

namespace AT_Utils
{
	abstract public class AddonWindowBase : MonoBehaviour
	{
		public static bool HUD_enabled { get; protected set; } = true;

		protected static Rect drag_handle = new Rect(0,0, 10000, 20);
		protected static string tooltip = "";

		protected virtual void onShowUI() { HUD_enabled = true; UpdateContent(); }
		protected virtual void onHideUI() { HUD_enabled = false; UpdateContent(); }

		protected virtual void UpdateContent() {}

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

		#region Tooltips
		//adapted from blizzy's Toolbar
		protected static void GetToolTip()
		{
			if(Event.current.type == EventType.repaint)
				tooltip = GUI.tooltip.Trim();
		}

		protected static void DrawToolTip(Rect window) 
		{
			if(tooltip.Length == 0) return;
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
		#endregion

		public static void TooltipAndDrag(Rect window_rect)
		{
			GetToolTip();
			DrawToolTip(window_rect);
			GUI.DragWindow(drag_handle);
		}
	}

	abstract public class AddonWindowBase<T> : AddonWindowBase where T : AddonWindowBase<T>
	{
		protected static T instance { get; private set; }
		protected static PluginConfiguration GUI_CFG = PluginConfiguration.CreateForType<T>();

		protected static int  width = 550, height = 100;
		protected static Rect MainWindow = new Rect();
		public static bool window_enabled { get; protected set; } = false;
		public static bool do_show { get { return window_enabled && HUD_enabled; } }
		public static string Title { get; protected set; }

		static void update_content() 
		{ if(instance != null) instance.UpdateContent(); }

		public static void Show(bool show) { window_enabled = show; update_content(); }
		public static void Toggle() { window_enabled = !window_enabled; update_content(); }

		public override void Awake()
		{
			base.Awake();
			instance = (T)this;
			LoadConfig();
			var assembly = Assembly.GetCallingAssembly().GetName();
			Title = string.Concat(assembly.Name, " - ", assembly.Version);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			SaveConfig();
			instance = null;
		}

		//settings
		protected static string mangleName(string name) { return typeof(T).Name+"-"+name; }

		protected static void SetConfigValue(string key, object value)
		{ GUI_CFG.SetValue(mangleName(key), value); }

		protected static V GetConfigValue<V>(string key, V _default)
		{ return GUI_CFG.GetValue<V>(mangleName(key), _default); }

		virtual public void LoadConfig()
		{
			GUI_CFG.load();
			window_enabled = GetConfigValue<bool>(Utils.PropertyName(new {window_enabled}), window_enabled);
			MainWindow = GetConfigValue<Rect>(Utils.PropertyName(new {MainWindow}), new Rect(100, 50, width, height));
		}

		virtual public void SaveConfig()
		{
			SetConfigValue(Utils.PropertyName(new {window_enabled}), window_enabled);
			SetConfigValue(Utils.PropertyName(new {MainWindow}), MainWindow);
			GUI_CFG.save();
		}

		/// <summary>
		/// Draws the main window. Should be called last in child class overrides.
		/// </summary>
		/// <param name="windowID">Window ID</param>
		protected virtual void DrawMainWindow(int windowID)
		{ TooltipAndDrag(MainWindow); }
	}
}

