//   AppToolbar.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri

using UnityEngine;
using KSP.UI.Screens;

namespace AT_Utils
{
    /// <summary>
    /// Toolbar manager is needed becaus in KSP-1.0+ the ApplicationLauncher
    /// works differently: it only fires OnReady event at MainMenu and the first
    /// time the Spacecenter is loaded. Thus we need to register the AppButton only
    /// once and then just hide and show it using VisibleScenes, not removing it.
    /// IMHO, this is a bug in the RemoveModApplication method, cause if you use
    /// Add/RemoveModApp repeatedly, the buttons are duplicated each time.
    /// </summary>
    public abstract class AppToolbar<T> : MonoBehaviour where T : AppToolbar<T>
    {
        public static T Instance { get; protected set; }
        //AppLauncher
        protected ApplicationLauncherButton ALButton;
        protected abstract string AL_ICON { get; }
        protected abstract ApplicationLauncher.AppScenes AL_SCENES { get; }
        protected abstract bool ForceAppLauncher { get; }
        //Toolbar
        protected IButton TBButton;
        protected abstract string TB_ICON { get; }
        protected abstract GameScenes[] TB_SCENES { get; }
        protected abstract string button_tooltip { get; }

        void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(this);
            Instance = (T)this;
            init();
        }

        void init()
        {
            //setup toolbar/applauncher button
            if(!ForceAppLauncher && ToolbarManager.ToolbarAvailable)
            {
                Utils.Log("Using Blizzy's toolbar");
                if(TBButton == null)
                    AddToolbarButton();
                if(ALButton != null)
                    ALButton.VisibleInScenes = ApplicationLauncher.AppScenes.NEVER;
            }
            else
            {
                Utils.Log("Using stock AppLauncher");
                if(ALButton == null)
                {
                    if(HighLogic.CurrentGame != null && ApplicationLauncher.Ready)
                        AddAppLauncherButton();
                    else
                        GameEvents.onGUIApplicationLauncherReady.Add(AddAppLauncherButton);
                }
                else
                    ALButton.VisibleInScenes = AL_SCENES;
                if(TBButton != null)
                {
                    TBButton.Destroy();
                    TBButton = null;
                }
            }
            on_init();
        }
        protected virtual void on_init() { }
        public static void Init() { if(Instance != null) Instance.init(); }

        //need to be instance method for Event.Add to work
        void AddAppLauncherButton()
        {
            if(ALButton == null && ApplicationLauncher.Ready)
            {
                Utils.Log("Adding AppLauncher button");
                ALButton = ApplicationLauncher.Instance.AddModApplication(
                    onALTrue, onALFalse,
                    onALHover, onALHoverOut,
                    onALEnable, onALDisable,
                    AL_SCENES,
                    TextureCache.GetTexture(AL_ICON));
                ALButton.onRightClick = onRightClick;
                on_app_launcher_init();
            }
        }
        protected virtual void on_app_launcher_init() { }

        void AddToolbarButton()
        {
            var ns = GetType().Namespace;
            TBButton = ToolbarManager.Instance.add(ns, ns + "Button");
            TBButton.TexturePath = TB_ICON;
            TBButton.ToolTip = button_tooltip;
            TBButton.Visibility = new GameScenesVisibility(TB_SCENES);
            TBButton.Visible = true;
            TBButton.OnClick += onToolbarToggle;
            on_toolbar_init();
        }
        protected virtual void on_toolbar_init() { }

        protected virtual void onToolbarToggle(ClickEvent e)
        {
            if(e.MouseButton == 1)
                onRightClick();
            else
                onLeftClick();
        }

        protected abstract void onLeftClick();
        protected virtual void onRightClick() { this.ToggleStylesUI(); }

        protected virtual void onALTrue() { onLeftClick(); }
        protected virtual void onALFalse() { onLeftClick(); }

        protected virtual void onALHover() { }
        protected virtual void onALHoverOut() { }

        protected virtual void onALEnable() { }
        protected virtual void onALDisable() { }
    }
}
