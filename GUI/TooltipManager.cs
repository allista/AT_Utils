//   TooltipManager.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using System;
using UnityEngine;

namespace AT_Utils
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, true)]
    public class TooltipManager : MonoBehaviour
    {
        private static TooltipManager instance;
        private static TooltipWindow tooltipWindow;

        private static string tooltip = "";
        private static readonly int max_width = Math.Max(Screen.width / 6, 200);

        private static Rect get_tooltip_rect(Vector2 mousePos)
        {
            var content = new GUIContent(tooltip);
            var size = Styles.tooltip.CalcSize(content);
            if(size.x > max_width)
            {
                size.x = max_width;
                size.y = Styles.tooltip.CalcHeight(content, max_width);
            }
            return new Rect(mousePos.x, mousePos.y + 20, size.x, size.y);
        }

        private static Rect clamp_to_screen(Rect rect, Rect orig, Vector2 mousePos)
        {
            //clamping moved the tooltip up -> reposition above mouse cursor
            if(rect.y < orig.y)
            {
                rect.y = mousePos.y - rect.height - 5;
                rect = rect.clampToScreen();
            }
            //clamping moved the tooltip left -> reposition lefto of the mouse cursor
            if(rect.x < orig.x)
            {
                rect.x = mousePos.x - rect.width - 5;
                rect = rect.clampToScreen();
            }
            return rect;
        }

        /// <summary>
        /// Gets the tooltip text. Should be called inside WindowFunction.
        /// </summary>
        public static void GetTooltip()
        {
            if(Event.current.type != EventType.Repaint)
                return;
            var tip = GUI.tooltip.Trim();
            if(!string.IsNullOrEmpty(tip))
                tooltip = tip;
        }

        /// <summary>
        /// Draws the tooltip inside the window Rect. Should be called inside WindowFunction.
        /// </summary>
        /// <param name="window">Window.</param>
        public static void DrawToolTip(Rect window)
        {
            GetTooltip();
            if(string.IsNullOrEmpty(tooltip))
                return;
            var mousePos = Utils.GetMousePosition(window);
            var rect = get_tooltip_rect(mousePos);
            rect = clamp_to_screen(rect.clampToWindow(window), rect, mousePos);
            GUI.Label(rect, tooltip, Styles.tooltip);
        }

        /// <summary>
        /// Draws the tooltip on screen. Should be called outside the WindowFunction.
        /// GetTooltip should be called beforehand insde the WindowFunction.
        /// </summary>
        public static void DrawToolTipOnScreen()
        {
            if(string.IsNullOrEmpty(tooltip))
                return;
            var mousePos =
                new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            var rect = get_tooltip_rect(mousePos);
            rect = clamp_to_screen(rect.clampToScreen(), rect, mousePos);
            GUI.Label(rect, tooltip, Styles.tooltip);
        }

        void Awake()
        {
            if(instance != null)
            {
                Destroy(this);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            tooltipWindow = new TooltipWindow();
            tooltipWindow.Show(this);
        }

        private void OnDestroy()
        {
            if(this != instance)
                return;
            instance = null;
            tooltipWindow.Close();
        }

        void LateUpdate()
        {
            tooltip = "";
        }

        void OnGUI()
        {
            if(!GUIWindowBase.HUD_enabled)
                return;
            GUI.depth = -1000;
            DrawToolTipOnScreen();
#if DEBUG
            if(HighLogic.LoadedScene != GameScenes.EDITOR
               && HighLogic.LoadedScene != GameScenes.FLIGHT
               && HighLogic.LoadedScene != GameScenes.SPACECENTER)
                return;
            if(!ComputationBalancer.Running)
                return;
            GUI.Label(debug_rect,
                string.Format("{0} {1:HH:mm:ss.fff} {4} FPS: {2:F0}:{3:F0}",
                    FlightGlobals.fetch != null
                        ? FlightGlobals.ActiveVessel != null
                            ? FlightGlobals.ActiveVessel.situation.ToString()
                            : ""
                        : "",
                    DateTime.Now,
                    ComputationBalancer.FPS,
                    ComputationBalancer.FPS_AVG,
                    Time.frameCount),
                Styles.boxed_label);
#endif
        }

#if DEBUG
        static readonly Rect debug_rect =
            new Rect(Screen.width * 0.75f, 0, 300, 25).clampToScreen();
#endif
    }
}
