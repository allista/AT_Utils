//   CrewTransferWindow.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    public class CrewTransferWindow : GUIWindowBase
    {
        int CrewCapacity;
        List<ProtoCrewMember> crew;
        List<ProtoCrewMember> selected;

        public CrewTransferWindow()
        {
            width = 350;
            height = 200;
        }

        public override void Awake()
        {
            base.Awake();
            Show(false);
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
                var label =
                    $"<b>{kerbal.name}</b> ({kerbal.trait} {kerbal.experienceLevel})\n<i>{kerbal.KerbalRef.InPart.Title()}</i>";
                if(GUILayout.Button(label, ki >= 0 ? Styles.enabled : Styles.active, GUILayout.ExpandWidth(true)))
                {
                    if(ki >= 0) selected.RemoveAt(ki);
                    else if(selected.Count < CrewCapacity)
                        selected.Add(kerbal);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            if(GUILayout.Button("Close", Styles.close_button, GUILayout.ExpandWidth(true))) Show(false);
            GUILayout.EndVertical();
            TooltipsAndDragWindow();
        }
        
        public void Draw(List<ProtoCrewMember> _crew, 
                         List<ProtoCrewMember> _selected, 
                         int _crew_capacity)
        {
            if(doShow)
            {
                crew = _crew;
                selected = _selected;
                CrewCapacity = _crew_capacity;
                LockControls();
                WindowPos = GUILayout.Window(GetInstanceID(), 
                                             WindowPos, TransferWindow,
                                             string.Format("Vessel Crew {0}/{1}", selected.Count, CrewCapacity),
                                             GUILayout.Width(width), GUILayout.Height(height)).clampToScreen();
            }
            else UnlockControls();
        }
    }
}

