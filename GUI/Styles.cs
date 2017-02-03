//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri
//
// This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.

using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AT_Utils
{
	public static class Styles 
	{
		public class Config : ConfigNodeObject
		{
			[Persistent] public string EnabledButtonColor  = "green";
			[Persistent] public string ActiveButtonColor   = "yellow";
			[Persistent] public string InactiveButtonColor = "grey";
			[Persistent] public string ConfirmButtonColor  = "green";
			[Persistent] public string AddButtonColor      = "green";
			[Persistent] public string CloseButtonColor    = "red";
			[Persistent] public string DangerButtonColor   = "red";
		}

		public static Config CFG { get { return AT_UtilsGlobals.Instance.StylesConfig; } }

		//This code is based on Styles class from Extraplanetary Launchpad plugin.
		public static GUISkin skin;

		public static GUIStyle normal_button;
		public static GUIStyle active_button;
		public static GUIStyle enabled_button;
		public static GUIStyle inactive_button;
		public static GUIStyle confirm_button;
		public static GUIStyle add_button;
		public static GUIStyle close_button;
		public static GUIStyle danger_button;

		public static GUIStyle grey_button;
		public static GUIStyle red_button;
		public static GUIStyle dark_red_button;
		public static GUIStyle green_button;
		public static GUIStyle dark_green_button;
		public static GUIStyle yellow_button;
		public static GUIStyle dark_yellow_button;
		public static GUIStyle cyan_button;
		public static GUIStyle magenta_button;

		public static GUIStyle white;
		public static GUIStyle white_on_black;
		public static GUIStyle grey;
		public static GUIStyle red;
		public static GUIStyle yellow;
		public static GUIStyle green;
		public static GUIStyle blue;
		public static GUIStyle cyan;
		public static GUIStyle magenta;

		public static GUIStyle label;
		public static GUIStyle rich_label;
		public static GUIStyle boxed_label;

		public static GUIStyle tooltip;

		public static GUIStyle slider;
		public static GUIStyle slider_text;

		public static GUIStyle list_item;
		public static GUIStyle list_box;

		public static GUIStyle no_window;

		public static FieldInfo[] StyleFields = typeof(Styles)
			.GetFields(BindingFlags.Public|BindingFlags.Static)
			.Where(fi => fi.FieldType == typeof(GUIStyle)).ToArray();

		public static void InitSkin()
		{
			if(skin != null) return;

			GUI.skin = null;
			skin = Object.Instantiate(GUI.skin);

			//new styles
			var tooltip_texture = new Texture2D(1, 1);
			tooltip_texture.SetPixel(0, 0, new Color(0.82f, 0.85f, 0.88f, 1f));
			tooltip_texture.Apply();

			var black_texture = new Texture2D(1, 1);
			black_texture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 1f));
			black_texture.Apply();

			var alpha_texture = new Texture2D(1, 1);
//			alpha_texture.SetPixel(0, 0, new Color(0, 0, 0, 0.3f));
			alpha_texture.SetPixel(0, 0, new Color(0, 0, 0, 0));
			alpha_texture.Apply();

			//buttons
			normal_button = GUI.skin.button.OtherColor(Color.white, Color.yellow);
			normal_button.padding = new RectOffset (4, 4, 4, 4);

			grey_button        = normal_button.OtherColor(Color.grey, Color.white);
			red_button         = normal_button.OtherColor(Color.red, Color.white);
			dark_red_button    = red_button.OtherColor(new Color(0.6f, 0, 0, 1));
			green_button       = red_button.OtherColor(Color.green);
			dark_green_button  = red_button.OtherColor(new Color(0, 0.6f, 0, 1));
			yellow_button      = red_button.OtherColor(Color.yellow);
			dark_yellow_button = red_button.OtherColor(new Color(0.6f, 0.6f, 0, 1));
			cyan_button        = red_button.OtherColor(Color.cyan);
			magenta_button     = red_button.OtherColor(Color.magenta);

			//boxes
			white = GUI.skin.box.OtherColor(Color.white);
			white.padding = new RectOffset (4, 4, 4, 4);

			white_on_black = new GUIStyle(white);
			white_on_black.normal.background = white_on_black.onNormal.background = white_on_black.hover.background = white_on_black.onHover.background = black_texture;

			grey    = white.OtherColor(Color.grey);
			red     = white.OtherColor(Color.red);
			yellow  = white.OtherColor(Color.yellow);
			green   = white.OtherColor(Color.green);
			blue    = white.OtherColor(new Color(0.6f, 0.6f, 1f, 1f));
			cyan    = white.OtherColor(Color.cyan);
			magenta = white.OtherColor(Color.magenta);

			//tooltip
			tooltip  = white.OtherColor(Color.black);
			tooltip.wordWrap = true;
			tooltip.richText = true;
			tooltip.alignment = TextAnchor.MiddleCenter;
			tooltip.normal.background = tooltip.onNormal.background = tooltip.hover.background = tooltip.onHover.background = tooltip_texture;

			//lable
			label = GUI.skin.label.OtherColor(Color.white);
			label.alignment = TextAnchor.MiddleCenter;
			label.wordWrap = true;

			rich_label = GUI.skin.label.OtherColor(Color.white);
			rich_label.richText = true;
			rich_label.wordWrap = true;

			boxed_label = new GUIStyle(white);
			boxed_label.alignment = TextAnchor.MiddleCenter;
			boxed_label.richText = true;
			boxed_label.wordWrap = true;

			//slider
			slider = new GUIStyle(GUI.skin.horizontalSlider);
			slider.margin = new RectOffset (0, 0, 0, 0);

			slider_text = new GUIStyle(GUI.skin.label);
			slider_text.alignment = TextAnchor.MiddleCenter;
			slider_text.margin = new RectOffset (0, 0, 0, 0);

			//list box
			list_item = new GUIStyle(GUI.skin.box);
			list_item.normal.background = list_item.onNormal.background = list_item.hover.background = list_item.onHover.background = black_texture;
			list_item.normal.textColor = list_item.focused.textColor = Color.white;
			list_item.hover.textColor = list_item.active.textColor = Color.yellow;
			list_item.onNormal.textColor = list_item.onFocused.textColor = list_item.onHover.textColor = list_item.onActive.textColor = Color.yellow;
			list_item.padding = new RectOffset(4, 4, 4, 4);

			list_box = new GUIStyle(GUI.skin.button);
			list_box.normal.textColor = list_box.focused.textColor = Color.yellow;
			list_box.hover.textColor = list_box.active.textColor = Color.green;
			list_box.onNormal.textColor = list_box.onFocused.textColor = list_box.onHover.textColor = list_box.onActive.textColor = Color.green;
			list_box.padding = new RectOffset (4, 4, 4, 4);

			//borderless window
			no_window = new GUIStyle(GUI.skin.window);
			no_window.normal.background = no_window.onNormal.background = no_window.hover.background = no_window.onHover.background = alpha_texture;
			no_window.border = new RectOffset(0,0,0,0);
			no_window.contentOffset = Vector2.zero;
			no_window.padding = new RectOffset(4,4,4,4);

			//customization
			//vertical scrollbar texture
			var scrollbar_texture = new Texture2D(1, 1);
			scrollbar_texture.SetPixel(0, 0, new Color(1f, 0.8f, 0f, 1f));
			scrollbar_texture.Apply();
			//vertical scrollbar
			skin.verticalScrollbar.fixedWidth = 5;
			skin.verticalScrollbarThumb.fixedWidth = 5;
			skin.verticalScrollbarThumb.border = new RectOffset(0,0,0,0);
			skin.verticalScrollbarThumb.normal.background = skin.verticalScrollbarThumb.onNormal.background = 
				skin.verticalScrollbarThumb.hover.background = skin.verticalScrollbarThumb.onHover.background = scrollbar_texture;
			//horizontal scrollbar
			skin.horizontalScrollbar.fixedHeight = 10;
			skin.horizontalScrollbarThumb.fixedHeight = 8;

			ConfigureButtons();
		}

		static GUIStyle find_style(string name)
		{
			foreach(var fi in StyleFields)
			{
				if(fi.Name == name) 
					return fi.GetValue(null) as GUIStyle;
			}
			return null;
		}

		static GUIStyle find_button_style(string color)
		{ return find_style(color.Replace(" ", "_")+"_button");	}

		public static void ConfigureButtons()
		{
			enabled_button  = find_button_style(CFG.EnabledButtonColor)  ?? green_button;
			active_button   = find_button_style(CFG.ActiveButtonColor)   ?? yellow_button;
			inactive_button = find_button_style(CFG.InactiveButtonColor) ?? grey_button;
			confirm_button  = find_button_style(CFG.ConfirmButtonColor)  ?? green_button;
			add_button      = find_button_style(CFG.AddButtonColor)      ?? green_button;
			close_button    = find_button_style(CFG.CloseButtonColor)    ?? red_button;
			danger_button   = find_button_style(CFG.DangerButtonColor)   ?? red_button;
		}

		static GUIStyle OtherColor(this GUIStyle style, Color normal)
		{
			var s = new GUIStyle(style);
			s.normal.textColor = s.focused.textColor = normal;
			return s;
		}

		static GUIStyle OtherColor(this GUIStyle style, Color normal, Color hover)
		{
			var s = style.OtherColor(normal);
			s.hover.textColor = s.active.textColor = hover;
			s.onNormal.textColor = s.onFocused.textColor = s.onHover.textColor = s.onActive.textColor = hover;
			return s;
		}

		public static void Init()
		{
			Styles.InitSkin();
			GUI.skin = Styles.skin;
		}

		public static GUIStyle fracStyle(float frac)
		{
			if(frac < 0.1) return Styles.red;
			if(frac < 0.5) return Styles.yellow;
			if(frac < 0.8) return Styles.white;
			return Styles.green;
		}
	}
}

