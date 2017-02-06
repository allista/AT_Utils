//   FloatingWindow.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using UnityEngine;

namespace AT_Utils
{
	public enum AnchorPosition { TopLeft, TopRight, BottomRight, BottomLeft }

	/// <summary>
	/// The base class for anchored windows.
	/// Such windows maintain the position of their anchored corner.
	/// </summary>
	public abstract class AnchoredWindow : GUIWindowBase
	{
		public AnchorPosition Anchor = AnchorPosition.TopLeft;

		/// <summary>
		/// Should draw the window using either GUI.Window or GUILayout.Window function.
		/// </summary>
		protected abstract Rect DrawWindow();

		public virtual Rect Draw()
		{
			if(doShow)
			{
				LockControls();
				var r = DrawWindow();
				update_window_rect(r);
				return WindowPos;
			}
			UnlockControls();
			return default(Rect);
		}

		/// <summary>
		/// Position of the anchor of the window in Screen coordinates.
		/// </summary>
		public Vector2 AnchorPos
		{
			get
			{
				switch(Anchor)
				{
				case AnchorPosition.TopLeft:
					return WindowPos.position;
				case AnchorPosition.TopRight:
					return new Vector2(WindowPos.xMax, WindowPos.y);
				case AnchorPosition.BottomLeft:
					return new Vector2(WindowPos.x, WindowPos.yMax);
				case AnchorPosition.BottomRight:
					return new Vector2(WindowPos.xMax, WindowPos.yMax);
				default:
					return WindowPos.position;
				}	
			}
		}

		/// <summary>
		/// Updates the window rect so that the anchor position remains the same.
		/// </summary>
		/// <param name="r">New window rect.</param>
		protected void update_window_rect(Rect r)
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
			WindowPos = r;
		}
	}
}

