//   AddonWindowBase.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using System.Reflection;
using KSP.UI.Screens;
using UnityEngine;

namespace AT_Utils
{
    abstract public class AddonWindowBase<T> : GUIWindowBase where T : AddonWindowBase<T>
    {
        public string Title = "";

        public static T Instance { get; private set; }

        public static bool InstanceEnabled 
        { get { return Instance != null && Instance.window_enabled; } }

        public static void ShowInstance(bool show)
        { if(Instance != null) Instance.Show(show); }

        public static void ToggleInstance()
        { if(Instance != null) Instance.Show(!Instance.window_enabled); }

        public static void ShowWithButton(bool show, ApplicationLauncherButton button)
        {
            if(Instance == null) return;
            if(button == null) ShowInstance(show);
            else
            {
                if(show)
                {
                    if(!Instance.window_enabled) 
                        Instance.Show(true);
                    button.SetTrue(false);
                }
                else
                {
                    if(Instance.window_enabled) 
                        Instance.Show(false); 
                    button.SetFalse(false);
                }
            }
        }

        public static void ToggleWithButton(ApplicationLauncherButton button)
        {
            if(Instance != null)
                ShowWithButton(!Instance.window_enabled, button);
        }

        readonly ActionDamper save_timer = new ActionDamper(10);

        public override void Awake()
        {
            if(Instance != null)
            { 
                Destroy(this);
                return; 
            }
            base.Awake();
            Instance = (T)this;
            Instance.LoadState();
            var assembly = Assembly.GetAssembly(typeof(T)).GetName();
            Title = string.Concat(assembly.Name, " - ", assembly.Version);
            GameEvents.onGameStateSave.Add(onGameStateSave);
            save_timer.action = () => Instance.SaveState();
        }

        public override void OnDestroy()
        {
            if(this != Instance)
                return;
            GameEvents.onGameStateSave.Remove(onGameStateSave);
            Instance.SaveState();
            Instance = null;
            base.OnDestroy();
        }

        void onGameStateSave(ConfigNode node) { if(Instance != null) Instance.SaveState(); }

        protected abstract void draw_gui();

        protected virtual void LateUpdate()
        {
            save_timer.Run();
        }

        public virtual void OnGUI()
        {
            if(Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint) return;
            if(doShow) 
            {
                Styles.Init();
                draw_gui();
            }
            else UnlockControls();
        }
    }
}

