//   PartSelector.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    public class PartSelector : GUIWindowBase
    {
        List<AvailablePart> parts = new List<AvailablePart>();
        List<GUIContent> buttons = new List<GUIContent>();
        Action<AvailablePart> load_part = delegate { };

        public PartSelector()
        {
            width = 400;
            height = 200;
            WindowPos = new Rect(Screen.width / 2 - width / 2, 100, width, 100);
        }

        public override void Awake()
        {
            base.Awake();
            Show(false);
        }

        public void RefreshParts()
        {
            parts.Clear();
            buttons.Clear();
            if(PartLoader.LoadedPartsList == null) return;
            foreach(var info in PartLoader.LoadedPartsList)
            {
                if(Utils.PartIsPurchased(info))
                {
                    Vector3 dims = Vector3.zero;
                    float mass = float.NaN, cost = float.NaN;
                    if(info.partPrefab)
                    {
                        mass = info.partPrefab.TotalMass();
                        cost = info.partPrefab.TotalCost();
                        dims = new Metric(info.partPrefab).size;
                    }
                    var label = string.Format("<color=yellow><b>{0}</b></color>\n" +
                                              "<color=silver>mass:</color> {1:F1}t " +
                                              "<color=silver>cost:</color> {2:F0} " +
                                              "<color=silver>size:</color> {3}",
                                              info.title, mass, cost,
                                              Utils.formatDimensions(dims));
                    var button = new GUIContent(label, info.description);
                    buttons.Add(button);
                    parts.Add(info);
                }
            }
        }

        public override void Show(bool show)
        {
            base.Show(show);
            if(show) RefreshParts();
        }

        Vector2 scroll = Vector2.zero;
        void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();
            scroll = GUILayout.BeginScrollView(scroll);
            AvailablePart toLoad = null;
            for(int i = 0, count = buttons.Count; i < count; i++)
            {
                var button = buttons[i];
                GUILayout.BeginHorizontal();
                if(GUILayout.Button(button, Styles.boxed_label, GUILayout.ExpandWidth(true)))
                    toLoad = parts[i];
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            if(toLoad != null && load_part != null)
            {
                load_part(toLoad);
                Show(false);
            }
            if(GUILayout.Button("Close", Styles.close_button, GUILayout.ExpandWidth(true)))
                Show(false);
            GUILayout.EndVertical();
            TooltipsAndDragWindow();
        }

        public void Draw(Action<AvailablePart> loadPart)
        {
            if(doShow)
            {
                LockControls();
                load_part = loadPart;
                WindowPos = GUILayout.Window(GetInstanceID(),
                                             WindowPos,
                                             DrawWindow,
                                             "Select Part",
                                             GUILayout.Width(width),
                                             GUILayout.Height(height))
                                     .clampToScreen();
            }
            else UnlockControls();
        }
    }
}
