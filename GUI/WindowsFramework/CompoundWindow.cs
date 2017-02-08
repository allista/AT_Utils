//   FloatingWindow.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AT_Utils
{
	/// <summary>
	/// Subwindow specification attribute. Using it a subwindow of a compound window may be placed
	/// relative to the anchor of the main window.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class SubwindowSpec : Attribute
	{
		/// <summary>
		/// The anchor position of the subwindow.
		/// </summary>
		public AnchorPosition Anchor;

		/// <summary>
		/// The position of the anchor of the subwindow
		/// relative to the anchor of the main window.
		/// </summary>
		public Vector2 Position;

		/// <summary>
		/// If set to <c>true</c>, the x coordinate of the Position is
		/// considered to be fractions of width of the main window.
		/// </summary>
		public bool xRelative;

		/// <summary>
		/// If set to <c>true</c>, the y coordinate of the Position is
		/// considered to be fractions of height of the main window.
		/// </summary>
		public bool yRelative;

		/// <summary>
		/// If set to <c>true</c>, the subwindow is always shown.
		/// </summary>
		public bool AlwaysShow;

		public SubwindowSpec(AnchorPosition anchor, float x, float y)
		{
			Anchor = anchor;
			Position = new Vector2(x, y);
			xRelative = false;
			yRelative = false;
		}
	}

	/// <summary>
	/// Base class for compound windows.
	/// A compound window has a main anchored window and a list of anchored subwidnows 
	/// whose position is fixed relative to the anchor position of the main window.
	/// </summary>
	public abstract class CompoundWindow : AnchoredWindow
	{
		protected List<AnchoredWindow> components = new List<AnchoredWindow>();
		protected Dictionary<AnchoredWindow, SubwindowSpec> specs = new Dictionary<AnchoredWindow, SubwindowSpec>();

		public override void Awake()
		{
			base.Awake();
			var component_fileds = GetType().GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy)
				.Where(fi => typeof(AnchoredWindow).IsAssignableFrom(fi.FieldType)).ToList();
			components = component_fileds.Select(fi => fi.GetValue(this) as AnchoredWindow).ToList();
			specs = new Dictionary<AnchoredWindow, SubwindowSpec>();
			foreach(var fi in component_fileds)
			{
				var spec = fi.GetCustomAttributes(typeof(SubwindowSpec), true).FirstOrDefault() as SubwindowSpec;
				if(spec == null) continue;
				var sw = fi.GetValue(this) as AnchoredWindow;
				if(sw == null) continue;
				specs.Add(sw, spec);
			}
		}

		/// <summary>
		/// Places subwindows according to their SubwindowSpecs.
		/// </summary>
		protected void place_components()
		{
			var anchor = AnchorPos;
			foreach(var sw in specs)
			{
				sw.Key.Anchor = sw.Value.Anchor;
				var pos = sw.Value.Position;
				if(sw.Value.xRelative) pos.x *= WindowPos.width;
				if(sw.Value.yRelative) pos.y *= WindowPos.height;
				sw.Key.Move(anchor+pos-sw.Key.AnchorPos);
				if(sw.Value.AlwaysShow) sw.Key.Show(true);
			}
		}

		static Rect combine(Rect a, Rect b)
		{
			if(a.width.Equals(0) && a.height.Equals(0)) return b;
			if(b.width.Equals(0) && b.height.Equals(0)) return a;
			return Rect.MinMaxRect(Mathf.Min(a.x, b.x),
			                       Mathf.Min(a.y, b.y),
			                       Mathf.Max(a.xMax, b.xMax),
			                       Mathf.Max(a.yMax, b.yMax));
		}

		public override void LoadConfig()
		{
			base.LoadConfig();
			place_components();
		}

		protected override void update_content()
		{
			base.update_content();
			place_components();
		}

		public override Rect Draw()
		{
			var ret = default(Rect);
			if(doShow) 
			{
				//save current anchor position and window rect
				var old_anchor = AnchorPos;
				var old_rect = WindowPos;
				//draw the main window
				base.Draw();
				//if it was moved, move subwindows
				var dPos = AnchorPos-old_anchor;
				if(!dPos.IsZero())
					components.ForEach(c => c.Move(dPos));
				//draw subwindows and combine their rects to get the total rect
				var total = WindowPos;
				components.ForEach(c => total = combine(total, c.Draw()));
                //if the window was resized, reposition subwindows
                if(old_rect.size != WindowPos.size)
                {
                    place_components();
                    total = WindowPos;
                    components.ForEach(c => total = combine(total, c.WindowPos));
                }
				//check if total rect is out of the screen, and move everything if needed
				var to_screen = total.clampToScreen();
				dPos = to_screen.position-total.position;
				if(!dPos.IsZero())
				{
					Move(dPos);
					components.ForEach(c => c.Move(dPos));
				}
				//return combined rect
				ret = to_screen;
			}
			else UnlockControls();
			return ret;
		}
	}

	/// <summary>
	/// An abstract base class for subwindows of compound window.
	/// It is drawn with no_window style.
	/// </summary>
	public abstract class SubWindow : AnchoredWindow
	{
		protected abstract void MainWindow(int windowID);

		protected override Rect DrawWindow()
		{
			return GUILayout.Window(GetInstanceID(), 
			                        WindowPos, 
			                        MainWindow, 
			                        "", Styles.no_window,
			                        GUILayout.Width(width),
			                        GUILayout.Height(height));
		}
	}
}

