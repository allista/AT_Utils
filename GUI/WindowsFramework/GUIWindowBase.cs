//   AddonWindowBase.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri

using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    [PersistState]
    abstract public class GUIWindowBase : MonoBehaviour
    {
        public static readonly Rect ScreenRect = new Rect(0,0,Screen.width,Screen.height);
        protected static Rect drag_handle = new Rect(0,0, 10000, 20);

        [ConfigOption]
        public Rect WindowPos = new Rect(200, 100, Screen.width/4, Screen.height/4);
        protected int width = 10, height = 10;

        public void Move(Vector2 dPos)
        {
            WindowPos.x += dPos.x;
            WindowPos.y += dPos.y;
        }

        #region Subwindows
        private List<FieldInfo> subwindow_fields;
        protected List<GUIWindowBase> subwindows;
        private GUIWindowBase parent_window;

        void init_subwindows()
        {
            subwindow_fields = GetType()
                .GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy)
                .Where(fi => !fi.FieldType.IsAbstract && typeof(GUIWindowBase).IsAssignableFrom(fi.FieldType)).ToList();
            subwindows = new List<GUIWindowBase>();
            foreach(var sw in subwindow_fields)
            {
                var obj = gameObject.AddComponent(sw.FieldType) as GUIWindowBase;
                if(obj == null)
                    continue;
                sw.SetValue(this, obj);
                subwindows.Add(obj);
                obj.parent_window = this;
            }
        }
        #endregion

        protected static bool level_loaded;
        protected static bool hud_enabled = true;
        [ConfigOption] protected bool window_enabled = true;
        public bool WindowEnabled => window_enabled;
        public static bool HUD_enabled => hud_enabled && level_loaded;
        public bool doShow => level_loaded && window_enabled && hud_enabled && can_draw() && (parent_window == null || parent_window.doShow);

        protected virtual bool can_draw() { return true; }

        public virtual void Show(bool show)
        {
            if(window_enabled == show)
                return;
            window_enabled = show;
            if(!show)
            {
                this.SaveState();
                UnlockControls();
            }
            update_content();
        }

        public void Toggle() { Show(!window_enabled); }

        protected virtual void onShowUI() { hud_enabled = true; update_content(); }
        protected virtual void onHideUI() { hud_enabled = false; update_content(); }

        protected void onLevelLoaded(GameScenes scene) { level_loaded = true; }
        protected void onGameSceneLoad(GameScenes scene) { level_loaded = false; }

        protected virtual void update_content() {}

        public virtual void Awake()
        {
            LockName = GetType().FullName+GetInstanceID();
            init_subwindows();
            GameEvents.onHideUI.Add(onHideUI);
            GameEvents.onShowUI.Add(onShowUI);
            GameEvents.onLevelWasLoadedGUIReady.Add(onLevelLoaded);
            GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoad);
            this.LoadState();
        }

        public virtual void OnDestroy()
        {
            this.SaveState();
            UnlockControls();
            subwindows.ForEach(Destroy);
            GameEvents.onHideUI.Remove(onHideUI);
            GameEvents.onShowUI.Remove(onShowUI);
            GameEvents.onLevelWasLoadedGUIReady.Remove(onLevelLoaded);
            GameEvents.onGameSceneLoadRequested.Remove(onGameSceneLoad);
        }

        #region GUI Lock
        public string LockName { get; protected set; }

        public virtual void UnlockControls()
        { 
            Utils.LockIfMouseOver(LockName, WindowPos, false);
            subwindows.ForEach(sw => sw.UnlockControls());
        }

        public virtual void LockControls()
        { Utils.LockIfMouseOver(LockName, WindowPos); }

        public static void TooltipsAndDragWindow()
        {
            TooltipManager.GetTooltip();
            GUI.DragWindow(drag_handle);
        }
        #endregion
    }
}

