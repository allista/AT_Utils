//  Author:
//       allis <>
//
//  Copyright (c) 2016 allis
//
// This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.
//
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
	public static partial class Utils
	{
		/// <summary>
		/// Gets the mouse position relative to the window Rect.
		/// </summary>
		/// <returns>The mouse position.</returns>
		/// <param name="window">Rect of the window.</param>
		public static Vector2 GetMousePosition(Rect window) 
		{
			var mouse_pos = Input.mousePosition;
			return new Vector2(mouse_pos.x-window.x, Screen.height-mouse_pos.y-window.y).clampToScreen();
		}

		/// <summary>
		/// Checks if the mouse pointer hovers over the last drawn GUILayout element.
		/// Should be called only if(Event.current.type == EventType.Repaint)
		/// </summary>
		/// <returns><c>true</c>, if mouse is over the last element, <c>false</c> otherwise.</returns>
		public static bool MouseInLastElement()
		{
			var rect = GUILayoutUtility.GetLastRect();
			var mousePos = Input.mousePosition;
			mousePos.y = Screen.height - mousePos.y;
			Vector2 clippedMousePos = Event.current.mousePosition;
			rect.x += mousePos.x - clippedMousePos.x;
			rect.y += mousePos.y - clippedMousePos.y;
			return rect.Contains(mousePos);
		}

		public static float FloatSlider(string name, float value, float min, float max, string format="F1", int label_width = -1, string tooltip = "")
		{
			var label = name.Length > 0? string.Format("{0}: {1}", name, value.ToString(format)) : value.ToString(format);
			GUILayout.Label(new GUIContent(label, tooltip), label_width > 0? GUILayout.Width(label_width) : GUILayout.ExpandWidth(false));
			return GUILayout.HorizontalSlider(value, min, max, GUILayout.ExpandWidth(true));
		}

		public static int IntSelector(int value, int min, int max=int.MaxValue, string format="D", string tooltip = "")
		{
			if(GUILayout.Button("<", Styles.normal_button, GUILayout.Width(15)))
			{ if(value >= min) value--; }
			GUILayout.Label(new GUIContent(value < min? "Off" : value.ToString(format), tooltip), 
				GUILayout.Width(20));
			if(GUILayout.Button(">", Styles.normal_button, GUILayout.Width(15)))
			{ if(value <= max) value++; }
			return value;
		}

		public static bool ButtonSwitch(string name, bool current_value, string tooltip = "", params GUILayoutOption[] options)
		{
			return string.IsNullOrEmpty(tooltip)? 
				GUILayout.Button(name, current_value ? Styles.enabled_button : Styles.active_button, options) : 
				GUILayout.Button(new GUIContent(name, tooltip), current_value ? Styles.enabled_button : Styles.active_button, options);
		}

		public static bool ButtonSwitch(string name, ref bool current_value, string tooltip = "", params GUILayoutOption[] options)
		{
			var ret = ButtonSwitch(name, current_value, tooltip, options);
			if(ret) current_value = !current_value;
			return ret;
		}

		public static int LeftRightChooser(string text, string tooltip = "", int width = 0)
		{
			width -= 40;
			var left  = GUILayout.Button("<", Styles.yellow_button, GUILayout.Width(20));
			if(width > 0) GUILayout.Label(new GUIContent(text, tooltip), Styles.white, GUILayout.Width(width));
			else GUILayout.Label(new GUIContent(text, tooltip), Styles.white);
			var right = GUILayout.Button(">", Styles.yellow_button, GUILayout.Width(20));
			return left? -1 : (right? 1 : 0);
		}

		public static T LeftRightChooser<T>(T current, IList<T> options, string tooltip = "", int width = 0)
		{
			var choice = Utils.LeftRightChooser(current.ToString(), tooltip, width);
			if(choice < 0) return options.Prev(current);
			if(choice > 0) return options.Next(current);
			return current;
		}

		public static V LeftRightChooser<K,V>(K current, SortedList<K,V> options, string tooltip = "", int width = 0)
		{
			var choice = Utils.LeftRightChooser(current.ToString(), tooltip, width);
			if(choice < 0) return options.Prev(current);
			else if(choice > 0) return options.Next(current);
			return default(V);
		}

		#region KSP_UI
		public static void EnableEvent(BaseEvent evt, bool enable, bool gui, bool editor)
		{
			evt.active = enable;
			evt.guiActive = gui;
			evt.guiActiveEditor = editor;
		}

		public static void EnableEvent(BaseEvent evt, bool enable = true)
		{ EnableEvent(evt, enable, enable, enable); }

		public static void EnableEventGUI(BaseEvent evt, bool enable = true)
		{ EnableEvent(evt, enable, enable, false); }

		public static void EnableEventEditor(BaseEvent evt, bool enable = true)
		{ EnableEvent(evt, enable, false, enable); }

		public static void EnableField(BaseField field, bool enable = true, UI_Scene scene = UI_Scene.All)
		{
			if((scene & UI_Scene.Flight) == UI_Scene.Flight) field.guiActive       = enable;
			if((scene & UI_Scene.Editor) == UI_Scene.Editor) field.guiActiveEditor = enable;
			if(field.uiControlEditor != null) field.uiControlEditor.controlEnabled = enable;
			if(field.uiControlFlight != null) field.uiControlFlight.controlEnabled = enable;
		}

		static void setup_chooser_control(string[] names, string[] values, UI_Control control)
		{
			var current_editor = control as UI_ChooseOption;
			if(current_editor == null) return;
			current_editor.display = names;
			current_editor.options = values;
		}

		public static void SetupChooser(string[] names, string[] values, BaseField field)
		{
			setup_chooser_control(names, values, field.uiControlEditor);
			setup_chooser_control(names, values, field.uiControlFlight);
		}
		#endregion

		#region ControlLock
		//modified from Kerbal Alarm Clock mod
		public static void LockControls(string LockName, bool Lock=true)
		{
			if(Lock && InputLockManager.GetControlLock(LockName) != ControlTypes.ALLBUTCAMERAS)
				InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, LockName);
			else if(!Lock && InputLockManager.GetControlLock(LockName) == ControlTypes.ALLBUTCAMERAS) 
				InputLockManager.RemoveControlLock(LockName);
		}

		public static void LockIfMouseOver(string LockName, Rect WindowRect, bool Lock=true)
		{
			Lock &= WindowRect.Contains(Event.current.mousePosition);
			LockControls(LockName, Lock);
		}

		public static void UpdatePartMenu(this Part part)
		{
			MonoUtilities.RefreshContextWindows(part);
			Utils.UpdateEditorGUI();
		}

		public static void UpdateEditorGUI()
		{ if(EditorLogic.fetch != null)	GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship); }
		#endregion

		public static void Message(float duration, string msg, params object[] args)
		{ 
			msg = string.Format(msg, args);
			#if DEBUG
			Log(msg);
			msg = string.Format("[{0:HH:mm:ss.fff}]\n{1}", DateTime.Now, msg);
			#endif
			ScreenMessages.PostScreenMessage(msg, duration, ScreenMessageStyle.UPPER_CENTER);
		}

		public static void Message(string msg, params object[] args) { Message(5, msg, args); }
	}
}

