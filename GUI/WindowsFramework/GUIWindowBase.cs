//   AddonWindowBase.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using KSP.IO;

namespace AT_Utils
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class ConfigOption : Attribute {}

    abstract public class GUIWindowBase : MonoBehaviour
    {
        public static bool HUD_enabled { get; protected set; } = true;
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
        public string Name = "";
        protected List<FieldInfo> subwindow_fields = new List<FieldInfo>();
        protected List<GUIWindowBase> subwindows = new List<GUIWindowBase>();

        void init_subwindows()
        {
            subwindow_fields = GetType()
                .GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy)
                .Where(fi => typeof(GUIWindowBase).IsAssignableFrom(fi.FieldType)).ToList();
            foreach(var sw in subwindow_fields)
            {
                var obj = gameObject.AddComponent(sw.FieldType) as GUIWindowBase;
                obj.Name = Name+"-"+sw.Name;
                obj.SetConfig(GUI_CFG);
                sw.SetValue(this, obj);
                subwindows.Add(obj);
            }
        }
        #endregion

        #region Config
        protected static Dictionary<string, PluginConfiguration> configs = new Dictionary<string, PluginConfiguration>();
        protected PluginConfiguration GUI_CFG;

        public void SetConfig(PluginConfiguration cfg)
        {
            if(cfg == null) return;
            GUI_CFG = cfg;
            subwindows.ForEach(sw => sw.SetConfig(cfg));
        }

        void create_config()
        {
            var config_path = AssemblyLoader.GetPathByType(GetType());
            if(configs.TryGetValue(config_path, out GUI_CFG)) return;
            var create_for_type = typeof(PluginConfiguration).GetMethod("CreateForType");
            create_for_type = create_for_type.MakeGenericMethod(new [] { GetType() });
            GUI_CFG = create_for_type.Invoke(null, new object[] { null }) as PluginConfiguration;
            configs[config_path] = GUI_CFG;
        }

        protected string mangleName(string name) { return Name+"-"+name; }

        protected void SetConfigValue(string key, object value)
        { GUI_CFG.SetValue(mangleName(key), value); }

        protected V GetConfigValue<V>(string key, V _default)
        { return GUI_CFG.GetValue<V>(mangleName(key), _default); }

        public virtual void LoadConfig()
        {
            GUI_CFG.load();
            var T = GetType();
            var get_val = T.GetMethod("GetConfigValue", BindingFlags.NonPublic|BindingFlags.Instance);
            foreach(var opt_fi in T.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy))
            {
//                Utils.Log("Load: {}[{}].{} = {}", T, GetInstanceID(), opt_fi.Name, opt_fi.GetValue(this));//debug
                if(typeof(GUIWindowBase).IsAssignableFrom(opt_fi.FieldType))
                {
                    try 
                    {
                        var opt = opt_fi.GetValue(this) as GUIWindowBase;
                        opt.LoadConfig();
                    } catch(NullReferenceException) {}
                    continue;
                }
                if(opt_fi.GetCustomAttributes(typeof(ConfigOption), true).Length == 0) continue;
                var get_val_gen = get_val.MakeGenericMethod(new []{opt_fi.FieldType});
                var val = get_val_gen.Invoke(this, new []{opt_fi.Name, opt_fi.GetValue(this)});
//                Utils.Log("Load: {}[{}].{} = {}, was {}", T, GetInstanceID(), opt_fi.Name, val, opt_fi.GetValue(this));//debug
                opt_fi.SetValue(this, val);
            }
        }

        public virtual void SaveConfig()
        {
            foreach(var opt_fi in GetType().GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy))
            {
//                Utils.Log("Save: {}[{}].{} = {}", GetType(), GetInstanceID(), opt_fi.Name, opt_fi.GetValue(this));//debug
                if(typeof(GUIWindowBase).IsAssignableFrom(opt_fi.FieldType))
                {
                    try
                    {
                        var opt = opt_fi.GetValue(this) as GUIWindowBase;
                        opt.SaveConfig();
                    } catch(NullReferenceException) {}
                    continue;
                }
                if(opt_fi.GetCustomAttributes(typeof(ConfigOption), true).Length == 0) continue;
                var val = opt_fi.GetValue(this);
//                Utils.Log("Save: {}[{}].{} = {}, was {}", GetType(), GetInstanceID(), opt_fi.Name, val, opt_fi.GetValue(this));//debug
                if(val != null) SetConfigValue(opt_fi.Name, val);
            }
            GUI_CFG.save();
        }
        #endregion

        [ConfigOption] protected bool window_enabled = true;
        public bool WindowEnabled { get { return window_enabled; } }
        public bool doShow { get { return level_loaded && window_enabled && HUD_enabled && can_draw(); } }
        protected static bool level_loaded;

        protected virtual bool can_draw() { return true; }

        public virtual void Show(bool show)
        {
            window_enabled = show;
            if(!show) UnlockControls();
            update_content();
        }

        public void Toggle() { Show(!window_enabled); }

        protected virtual void onShowUI() { HUD_enabled = true; update_content(); }
        protected virtual void onHideUI() { HUD_enabled = false; update_content(); }

        protected void onLevelLoaded(GameScenes scene) { level_loaded = true; }
        protected void onGameSceneLoad(GameScenes scene) { level_loaded = false; }

        protected virtual void update_content() {}

        public virtual void Awake()
        {
            LockName = GetType().FullName+GetInstanceID();
            Name = GetType().Name;
            create_config();
            init_subwindows();
            GameEvents.onHideUI.Add(onHideUI);
            GameEvents.onShowUI.Add(onShowUI);
            GameEvents.onLevelWasLoadedGUIReady.Add(onLevelLoaded);
            GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoad);
        }

        public virtual void OnDestroy()
        {
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

