//   FloatingWindow.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using System;
using UnityEngine;

namespace AT_Utils
{
	public enum AnchorPosition { TopLeft, TopRight, BottomRight, BottomLeft }

	public class FloatingWindow : GUIWindowBase
	{
		[ConfigOption] public AnchorPosition Anchor = AnchorPosition.TopLeft;
		protected bool moving;

		static GUIContent anchor_content = new GUIContent("●", "Press to move the window");
		void draw_anchor()
		{
			GUILayout.Label(anchor_content, Styles.green, GUILayout.ExpandWidth(false));
			if(Event.current.type == EventType.Repaint)
				moving = GUILayoutUtility.GetLastRect().Contains(Utils.GetMousePosition(WindowPos));
		}

		public FloatingWindow() 
		{ 
			width = height = 10;
		}

		public Action DrawContent = delegate {};

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
				DrawContent();
				GUILayout.EndHorizontal();
				break;
			case AnchorPosition.TopRight:
				GUILayout.BeginHorizontal();
				DrawContent();
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
				DrawContent();
				GUILayout.EndHorizontal();
				break;
			case AnchorPosition.BottomRight:
				GUILayout.BeginHorizontal();
				DrawContent();
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

		void update_window_rect(Rect r)
		{
			switch(Anchor)
			{
			case AnchorPosition.TopRight:
				if(!r.width.Equals(WindowPos.width))
					r.x += WindowPos.width-r.width;
				break;
			case AnchorPosition.BottomLeft:
				if(!r.height.Equals(WindowPos.width))
					r.y += WindowPos.height-r.height;
				break;
			case AnchorPosition.BottomRight:
				if(!r.width.Equals(WindowPos.width))
					r.x += WindowPos.width-r.width;
				if(!r.height.Equals(WindowPos.width))
					r.y += WindowPos.height-r.height;
				break;
			}
			WindowPos = r.clampToScreen();
		}

		public void Draw(Action content = null)
		{
			if(content != null) 
				DrawContent = content;
			LockControls();
			var r = GUILayout.Window(GetInstanceID(), 
			                         WindowPos, 
		                             main_window, 
		                             "", Styles.no_window,
		                             GUILayout.Width(width),
		                             GUILayout.Height(height));
			update_window_rect(r);
		}
	}
}

