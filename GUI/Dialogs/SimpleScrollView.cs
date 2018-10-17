//   SimpleScrollView.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using System;
using UnityEngine;

namespace AT_Utils
{
    public class SimpleScrollView : GUIWindowBase
    {
        public Action DrawContent = () => { };
        Vector2 scroll = Vector2.zero;

        public SimpleScrollView()
        {
            width = 400;
            height = 300;
            WindowPos = new Rect(Screen.width / 2 - width / 2, 100, width, height);
        }

        void DialogWindow(int windowId)
        {
            GUILayout.BeginVertical();
            scroll = GUILayout.BeginScrollView(scroll);
            DrawContent();
            GUILayout.EndScrollView();
            if(GUILayout.Button("Close", Styles.close_button, GUILayout.ExpandWidth(true)))
                Show(false);
            GUILayout.EndVertical();
            TooltipsAndDragWindow();
        }

        public void Draw(string title)
        {
            if(doShow)
            {
                LockControls();
                WindowPos = GUILayout.Window(GetInstanceID(),
                                             WindowPos, DialogWindow,
                                             title,
                                             GUILayout.Width(width),
                                             GUILayout.Height(height))
                                     .clampToScreen();
            }
            UnlockControls();
        }
    }
}
