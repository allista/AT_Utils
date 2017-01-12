//   FloatingWindow.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using UnityEngine;

namespace AT_Utils
{
	/// <summary>
	/// Base class for floating windows, which are compound windows drawn with no_window style.
	/// The anchor position of the main window is marked with the anchor control with which the 
	/// whole compound window can be dragged.
	/// </summary>
	public abstract class FloatingWindow : CompoundWindow
	{
		protected bool moving;

		static GUIContent anchor_content = new GUIContent("●", "Press to move the window");
		void draw_anchor()
		{
			GUILayout.Label(anchor_content, Styles.green, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
			if(Event.current.type == EventType.Repaint)
				moving = GUILayoutUtility.GetLastRect().Contains(Utils.GetMousePosition(WindowPos));
		}

		protected abstract void DrawContent();

		void main_window(int windowID)
		{
			switch(Anchor)
			{
			case AnchorPosition.TopLeft:
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();
				draw_anchor();
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				DrawContent();
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				break;
			case AnchorPosition.TopRight:
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				DrawContent();
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
				draw_anchor();
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				break;
			case AnchorPosition.BottomLeft:
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				draw_anchor();
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				DrawContent();
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				break;
			case AnchorPosition.BottomRight:
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				DrawContent();
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				draw_anchor();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				break;
			}
			//get tooltip for future display
			TooltipManager.GetTooltip();
			//move the window if requested
			if(moving) GUI.DragWindow(ScreenRect);
		}

		protected override Rect DrawWindow()
		{
			return GUILayout.Window(GetInstanceID(), 
			                        WindowPos, 
			                        main_window, 
			                        "", Styles.no_window,
			                        GUILayout.Width(width),
			                        GUILayout.Height(height));
		}
	}
}

