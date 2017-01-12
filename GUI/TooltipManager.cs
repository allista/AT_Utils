//   TooltipManager.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using UnityEngine;

namespace AT_Utils
{
	[KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
	public class TooltipManager : MonoBehaviour
	{
		static string tooltip = "";

		/// <summary>
		/// Gets the tooltip text. Should be called inside WindowFunction.
		/// </summary>
		public static void GetTooltip()
		{
			if(Event.current.type == EventType.Repaint)
			{ 
				var tip = GUI.tooltip.Trim();
				if(!string.IsNullOrEmpty(tip))
					tooltip = tip;
			}
		}

		/// <summary>
		/// Draws the tooltip inside the window Rect. Should be called inside WindowFunction.
		/// </summary>
		/// <param name="window">Window.</param>
		public static void DrawToolTip(Rect window) 
		{
			GetTooltip();
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

		/// <summary>
		/// Draws the tooltip on screen. Should be called outside the WindowFunction.
		/// GetTooltip should be called beforehand insde the WindowFunction.
		/// </summary>
		public static void DrawToolTipOnScreen()
		{
			if(string.IsNullOrEmpty(tooltip)) return;
			var mousePos = new Vector2(Input.mousePosition.x, Screen.height-Input.mousePosition.y);
			var size = Styles.tooltip.CalcSize(new GUIContent(tooltip));
			var rect = new Rect(mousePos.x, mousePos.y + 20, size.x, size.y);
			Rect orig = rect;
			rect = rect.clampToScreen();
			//clamping moved the tooltip up -> reposition above mouse cursor
			if(rect.y < orig.y) 
			{
				rect.y = mousePos.y - size.y - 5;
				rect = rect.clampToScreen();
			}
			//clamping moved the tooltip left -> reposition left of the mouse cursor
			if(rect.x < orig.x)
			{
				rect.x = mousePos.x - size.x - 5;
				rect = rect.clampToScreen();
			}
			GUI.Label(rect, tooltip, Styles.tooltip);
		}

		void Update() { tooltip = ""; }

		void OnGUI()
		{
			GUI.depth = -1;
			if(GUIWindowBase.HUD_enabled)
				DrawToolTipOnScreen();
		}
	}
}

