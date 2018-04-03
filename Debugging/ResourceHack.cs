#if DEBUG
//   ResourceHack.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri
using UnityEngine;

namespace AT_Utils
{
    public class ResourceHack : PartModule
    {
        bool manage;

        [KSPEvent(guiName = "Manage Resources", guiActive = true, guiActiveUncommand = true)]
        void ManageResources() { manage = !manage; }

        public override void OnAwake()
        {
            base.OnAwake();
            LockName += GetInstanceID().ToString();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            StartCoroutine(CallbackUtil.DelayedCallback(5, () => { enabled = isEnabled = part.Resources.Count > 0; }));
        }

        string LockName = "ResourceHack";
        static int width = 200, height = 150;
        Vector2 scroll = Vector3.zero;
        Rect WindowPos = new Rect((Screen.width-width)/2, Screen.height/4, width, height+60);
        void ResourceManager(int windowID)
        {
            GUILayout.BeginVertical();
            scroll = GUILayout.BeginScrollView(scroll, Styles.white, GUILayout.Width(width), GUILayout.Height(height));
            foreach(var res in part.Resources)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(res.resourceName, Styles.boxed_label, GUILayout.ExpandWidth(true));
                if(GUILayout.Button("Empty", Styles.danger_button, GUILayout.ExpandWidth(false)))
                    res.amount = 0;
                if(GUILayout.Button("Fill", Styles.enabled_button, GUILayout.ExpandWidth(false)))
                    res.amount = res.maxAmount;
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            if(GUILayout.Button("Close", Styles.danger_button, GUILayout.ExpandWidth(true)))
                manage = false;
            GUILayout.EndVertical();
            GUIWindowBase.TooltipsAndDragWindow();
        }

        void OnGUI()
        {
            
            if(Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint) return;
            if(manage && GUIWindowBase.HUD_enabled)
            {
                Styles.Init();
                Utils.LockIfMouseOver(LockName, WindowPos);
                WindowPos = GUILayout.Window(GetInstanceID(), 
                                             WindowPos, ResourceManager, part.partInfo.title,
                                             GUILayout.Width(width),
                                             GUILayout.Height(height)).clampToScreen();
            }
            else Utils.LockIfMouseOver(LockName, WindowPos, false);
        }
    }
}
#endif