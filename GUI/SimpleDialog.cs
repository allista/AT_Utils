using UnityEngine;

namespace AT_Utils
{
	public class SimpleDialog : GUIWindowBase
	{
		public enum Answer { None, Yes, No }

		public SimpleDialog()
		{ 
			width = 400;
			WindowPos = new Rect(Screen.width/2-width/2, 100, width, 50); 
		}

		string message;
		public Answer Result { get; private set; }

		void DialogWindow(int windowId)
		{
			GUILayout.BeginVertical();
			GUILayout.Label(message, Styles.rich_label, GUILayout.Width(width));
			GUILayout.BeginHorizontal();
			Result = Answer.None;
			if(GUILayout.Button("No", Styles.red_button, GUILayout.Width(70))) Result = Answer.No;
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Yes", Styles.green_button, GUILayout.Width(70))) Result = Answer.Yes;
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUI.DragWindow(new Rect(0, 0, Screen.width, 20));
		}

		public Answer Show(string message, string title = "Warning")
		{
			this.message = message;
			LockControls();
			WindowPos = GUILayout.Window(GetInstanceID(), 
			                             WindowPos, DialogWindow,
			                             title,
			                             GUILayout.Width(width)).clampToScreen();
			if(Result != Answer.None) UnlockControls();
			return Result;
		}
	}
}
