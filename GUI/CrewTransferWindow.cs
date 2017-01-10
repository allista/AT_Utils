//   CrewTransferWindow.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System.Collections.Generic;
using UnityEngine;
using AT_Utils;

namespace AT_Utils
{
	public class CrewTransferWindow : GUIWindowBase
	{
		int CrewCapacity;
		List<ProtoCrewMember> crew;
		List<ProtoCrewMember> selected;

		public bool Closed { get; private set; }

		public CrewTransferWindow()
		{
			width = 250; height = 150;
		}
		
		Vector2 scroll_view = Vector2.zero;
        void TransferWindow(int windowId)
        {
			GUILayout.BeginVertical();
			scroll_view = GUILayout.BeginScrollView(scroll_view, GUILayout.Width(width), GUILayout.Height(height));
			GUILayout.BeginVertical(Styles.white);
            foreach(ProtoCrewMember kerbal in crew)
            {
				int ki = selected.FindIndex(cr => cr.name == kerbal.name);
				if(Utils.ButtonSwitch(kerbal.name, ki >= 0, "", GUILayout.ExpandWidth(true)))
				{
					if(ki >= 0) selected.RemoveAt(ki);
					else if(selected.Count < CrewCapacity)
						selected.Add(kerbal);
				}
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
			Closed = GUILayout.Button("Close", Styles.close_button, GUILayout.ExpandWidth(true));
			GUILayout.EndVertical();
			TooltipsAndDragWindow();
        }
		
		public void Draw(List<ProtoCrewMember> _crew, 
		                 List<ProtoCrewMember> _selected, 
		                 int _crew_capacity)
		{
			crew = _crew;
			selected = _selected;
			CrewCapacity = _crew_capacity;
			LockControls();
			WindowPos = GUILayout.Window(GetInstanceID(), 
			                             WindowPos, TransferWindow,
										 string.Format("Vessel Crew {0}/{1}", selected.Count, CrewCapacity),
			                             GUILayout.Width(width), GUILayout.Height(height)).clampToScreen();
			if(Closed) UnlockControls();
		}
	}
}

