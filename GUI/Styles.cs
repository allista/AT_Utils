//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri
//
// This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.

using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEngine;
using AT_Utils.UI;

namespace AT_Utils
{
    public static class Styles
    {
        public class Config : ConfigNodeObject
        {
        }

        public static Config CFG => AT_UtilsGlobals.Instance.StylesConfig;

        //This code is based on Styles class from Extraplanetary Launchpad plugin.
        public static GUISkin skin;
        public static Action onSkinInit = () => { };

        public static GUIStyle normal_button;
        public static GUIStyle inactive_button;
        public static GUIStyle active_button;
        public static GUIStyle enabled_button;

        public static GUIStyle confirm_button;
        public static GUIStyle open_button;
        public static GUIStyle close_button;

        public static GUIStyle good_button;
        public static GUIStyle danger_button;

        public static GUIStyle sel1_button;
        public static GUIStyle sel2_button;

        public static GUIStyle white;
        public static GUIStyle white_on_black;

        public static GUIStyle inactive;
        public static GUIStyle active;
        public static GUIStyle enabled;

        public static GUIStyle good;
        public static GUIStyle warning;
        public static GUIStyle danger;

        public static GUIStyle selected1;
        public static GUIStyle selected2;

        public static GUIStyle green;
        public static GUIStyle blue;

        public static GUIStyle label;
        public static GUIStyle rich_label;
        public static GUIStyle boxed_label;

        public static GUIStyle tooltip;

        public static GUIStyle slider;
        public static GUIStyle slider_text;

        public static GUIStyle list_item;
        public static GUIStyle list_box;

        public static GUIStyle no_window;

        public static FieldInfo[] StyleFields = typeof(Styles)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(fi => fi.FieldType == typeof(GUIStyle)).ToArray();

        public static void InitSkin()
        {
            if(skin != null) return;

            GUI.skin = null;
            skin = UnityEngine.Object.Instantiate(GUI.skin);

            //new styles
            var tooltip_texture = new Texture2D(1, 1);
            tooltip_texture.SetPixel(0, 0, new Color(0.82f, 0.85f, 0.88f, 1f));
            tooltip_texture.Apply();

            var black_texture = new Texture2D(1, 1);
            black_texture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 1f));
            black_texture.Apply();

            var alpha_texture = new Texture2D(1, 1);
            alpha_texture.SetPixel(0, 0, new Color(0, 0, 0, 0));
            //            alpha_texture.SetPixel(0, 0, new Color(0, 0, 0, 0.3f));//debug
            alpha_texture.Apply();

            //buttons
            normal_button = GUI.skin.button.OtherColor(Color.white, Color.yellow);
            normal_button.padding = new RectOffset(4, 4, 4, 4);

            //boxes
            white = GUI.skin.box.OtherColor(Color.white);
            white.padding = new RectOffset(4, 4, 4, 4);

            white_on_black = new GUIStyle(white);
            white_on_black.normal.background = white_on_black.onNormal.background = white_on_black.hover.background = white_on_black.onHover.background = black_texture;

            inactive = white.OtherColor(Colors.Inactive);
            active = white.OtherColor(Colors.Active);
            enabled = white.OtherColor(Colors.Enabled);
            good = white.OtherColor(Colors.Good);
            warning = white.OtherColor(Colors.Warning);
            danger = white.OtherColor(Colors.Danger);
            selected1 = white.OtherColor(Colors.Selected1);
            selected2 = white.OtherColor(Colors.Selected2);

            green = white.OtherColor(Color.green);
            blue = white.OtherColor(new Color(0.6f, 0.6f, 1f, 1f));

            //tooltip
            tooltip = white.OtherColor(Color.black);
            tooltip.wordWrap = true;
            tooltip.richText = true;
            tooltip.alignment = TextAnchor.MiddleCenter;
            tooltip.normal.background = tooltip.onNormal.background = tooltip.hover.background = tooltip.onHover.background = tooltip_texture;

            //lable
            label = GUI.skin.label.OtherColor(Color.white);
            label.alignment = TextAnchor.MiddleCenter;
            label.wordWrap = true;

            rich_label = GUI.skin.label.OtherColor(Color.white);
            rich_label.richText = true;
            rich_label.wordWrap = true;

            boxed_label = new GUIStyle(white);
            boxed_label.alignment = TextAnchor.MiddleCenter;
            boxed_label.richText = true;
            boxed_label.wordWrap = true;

            //slider
            slider = new GUIStyle(GUI.skin.horizontalSlider);
            slider.margin = new RectOffset(0, 0, 0, 0);

            slider_text = new GUIStyle(GUI.skin.label);
            slider_text.alignment = TextAnchor.MiddleCenter;
            slider_text.margin = new RectOffset(0, 0, 0, 0);

            //list box
            list_item = new GUIStyle(GUI.skin.box);
            list_item.normal.background = list_item.onNormal.background = list_item.hover.background = list_item.onHover.background = black_texture;
            list_item.normal.textColor = list_item.focused.textColor = Color.white;
            list_item.hover.textColor = list_item.active.textColor = Color.yellow;
            list_item.onNormal.textColor = list_item.onFocused.textColor = list_item.onHover.textColor = list_item.onActive.textColor = Color.yellow;
            list_item.padding = new RectOffset(4, 4, 4, 4);

            list_box = new GUIStyle(GUI.skin.button);
            list_box.normal.textColor = list_box.focused.textColor = Color.yellow;
            list_box.hover.textColor = list_box.active.textColor = Color.green;
            list_box.onNormal.textColor = list_box.onFocused.textColor = list_box.onHover.textColor = list_box.onActive.textColor = Color.green;
            list_box.padding = new RectOffset(4, 4, 4, 4);

            //borderless window
            no_window = new GUIStyle(GUI.skin.window);
            no_window.normal.background = no_window.onNormal.background = no_window.hover.background = no_window.onHover.background = alpha_texture;
            no_window.border = new RectOffset(0, 0, 0, 0);
            no_window.contentOffset = Vector2.zero;
            no_window.padding = new RectOffset(4, 4, 4, 4);

            //customization
            //vertical scrollbar texture
            var scrollbar_texture = new Texture2D(1, 1);
            scrollbar_texture.SetPixel(0, 0, new Color(1f, 0.8f, 0f, 1f));
            scrollbar_texture.Apply();
            //vertical scrollbar
            skin.verticalScrollbar.fixedWidth = 5;
            skin.verticalScrollbarThumb.fixedWidth = 5;
            skin.verticalScrollbarThumb.border = new RectOffset(0, 0, 0, 0);
            skin.verticalScrollbarThumb.normal.background = skin.verticalScrollbarThumb.onNormal.background =
                skin.verticalScrollbarThumb.hover.background = skin.verticalScrollbarThumb.onHover.background = scrollbar_texture;
            //horizontal scrollbar
            skin.horizontalScrollbar.fixedHeight = 10;
            skin.horizontalScrollbarThumb.fixedHeight = 8;

            ConfigureButtons();
            frac_styles.Clear();
            onSkinInit();
        }

        static Color g = Color.gray;
        static GUIStyle MakeButton(Color c) =>
        normal_button.OtherColor(c, c + g);

        public static void ConfigureButtons()
        {
            enabled_button = MakeButton(Colors.Enabled);
            active_button = MakeButton(Colors.Active);
            inactive_button = normal_button.OtherColor(Colors.Inactive, Colors.Inactive);
            confirm_button = MakeButton(Colors.Confirm);
            open_button = MakeButton(Colors.Open);
            close_button = MakeButton(Colors.Close);
            good_button = MakeButton(Colors.Good);
            danger_button = MakeButton(Colors.Danger);
            sel1_button = MakeButton(Colors.Selected1);
            sel2_button = MakeButton(Colors.Selected2);
        }

        static GUIStyle OtherColor(this GUIStyle style, Color normal)
        {
            var s = new GUIStyle(style);
            s.normal.textColor = s.focused.textColor = normal;
            return s;
        }

        static GUIStyle OtherColor(this GUIStyle style, Color normal, Color hover)
        {
            var s = style.OtherColor(normal);
            s.hover.textColor = s.active.textColor = hover;
            s.onNormal.textColor = s.onFocused.textColor = s.onHover.textColor = s.onActive.textColor = hover;
            return s;
        }

        public static void Init()
        {
            InitSkin();
            GUI.skin = skin;
        }

        static Dictionary<int, GUIStyle> frac_styles = new Dictionary<int, GUIStyle>();
        public static GUIStyle fracStyle(float frac)
        {
            GUIStyle s;
            var bin = Mathf.FloorToInt(frac * 10);
            if(!frac_styles.TryGetValue(bin, out s))
            {
                s = white.OtherColor(Colors.FractionGradient.Evaluate(Mathf.Min((bin + 0.5f) / 10f, 1)));
                frac_styles.Add(bin, s);
            }
            return s;
        }

        static GameObject colorListPrefab;
        static ColorList colorList;
        static Vector3 listPos = Vector3.zero;

        static bool in_progress;
        static public IEnumerator ShowUI()
        {
            if(in_progress || colorList != null)
                yield break;
            in_progress = true;
            bool first_start = false;
            if(colorListPrefab == null)
            {
                foreach(var _ in UIBundle.LoadAsset("ColorList"))
                    yield return null;
                colorListPrefab = UIBundle.GetAsset("ColorList");
                if(colorListPrefab == null)
                    goto end;
                first_start = true;
            }
            var listObj = UnityEngine.Object.Instantiate(colorListPrefab);
            colorList = listObj.GetComponent<ColorList>();
            listObj.SetActive(false);
            if(colorList == null)
            {
                Utils.Log("{} does not have ColorList component: {}",
                          listObj, listObj.GetComponents<MonoBehaviour>());
                UnityEngine.Object.Destroy(listObj);
                goto end;
            }
            colorList.SetTitle("Color Scheme of AT Mods");
            colorList.closeButton.onClick.AddListener(Close);
            colorList.saveButton.onClick.AddListener(Save);
            colorList.resetButton.onClick.AddListener(Reset);
            colorList.restoreButton.onClick.AddListener(Restore);
            listObj.transform.SetParent(DialogCanvasUtil.DialogCanvasRect);
            listObj.SetActive(true);
            if(first_start)
            {
                listObj.transform.localPosition = new Vector3(-Screen.width, 0);
                Rect rect = new Rect();
                while(rect.width.Equals(0))
                {
                    rect = (listObj.transform as RectTransform).rect;
                    yield return null;
                }
                listPos = new Vector3(-rect.width / 2, rect.height / 2);
            }
            listObj.transform.localPosition = listPos;
        end:
            in_progress = false;
        }

        static void Close()
        {
            HideUI();
            AT_UtilsGlobals.Load();
            skin = null;
        }

        static void Reset()
        {
            Colors.SetDefaults();
            skin = null;
        }

        static void Restore()
        {
            AT_UtilsGlobals.Restore();
            skin = null;
        }

        static void Save()
        {
            AT_UtilsGlobals.Save("Colors");
            skin = null;
        }

        static public void HideUI()
        {
            if(colorList != null)
            {
                listPos = colorList.transform.localPosition;
                colorList.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(colorList.gameObject);
                colorList = null;
            }
        }

        static public bool IsUiShown() =>
        !in_progress && colorList != null;

        public static void ToggleStylesUI(this MonoBehaviour monoBehaviour)
        {
            if(IsUiShown())
                HideUI();
            else
                monoBehaviour.StartCoroutine(ShowUI());
        }
    }
}
