//   Colors.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace AT_Utils.UI
{
    [Serializable]
    public class ColorChangedEvent : UnityEvent<Color> { }

    [Serializable]
    public class Colors : IColorProvider
    {
        static Colors instance;
        public static Colors Instance
        {
            get
            {
                if(instance == null)
                    instance = new Colors();
                return Instance;
            }
        }

        public static ColorSetting Neutral = ColorSetting.white;
        public static ColorSetting Enabled = ColorSetting.green;
        public static ColorSetting Active = ColorSetting.yellow;
        public static ColorSetting Inactive = ColorSetting.grey;
        public static ColorSetting Confirm = ColorSetting.green;
        public static ColorSetting Open = ColorSetting.green;
        public static ColorSetting Close = ColorSetting.red;
        public static ColorSetting Good = ColorSetting.green;
        public static ColorSetting Warning = ColorSetting.yellow;
        public static ColorSetting Danger = ColorSetting.red;
        public static ColorSetting Selected1 = ColorSetting.cyan;
        public static ColorSetting Selected2 = ColorSetting.magenta;

        public static SimpleGradient FractionGradient = new SimpleGradient();

        public static SortedList<string, ColorSetting> All { get; } 

        static Colors()
        {
            All = new SortedList<string, ColorSetting>();
            foreach(var fi in typeof(Colors).GetFields(BindingFlags.Static|BindingFlags.Public)
                    .Where(fi => fi.FieldType == typeof(ColorSetting)))
                All.Add(fi.Name, fi.GetValue(null) as ColorSetting);
            FractionGradient = new SimpleGradient(new [] { Danger, Warning, Good });
        }

        public static void SetDefaults()
        {
            Enabled.color = ColorSetting.green;
            Active.color = ColorSetting.yellow;
            Inactive.color = ColorSetting.grey;
            Confirm.color = ColorSetting.green;
            Open.color = ColorSetting.green;
            Close.color = ColorSetting.red;
            Good.color = ColorSetting.green;
            Warning.color = ColorSetting.yellow;
            Danger.color = ColorSetting.red;
            Selected1.color = ColorSetting.cyan;
            Selected2.color = ColorSetting.magenta;
        }

        public static ColorSetting GetColor(string key)
        {
            ColorSetting color;
            return All.TryGetValue(key, out color)? color : null;
        }

        public IColored GetColored(string key) => GetColor(key);
    }

    [Serializable]
    public class ColorSetting : IColored
    {
        Color _color = Color.white;
        string _html = "#FFFFFFFF";
        string _tag = "<color=#FFFFFFFF>{0}</color>";

        public ColorChangedEvent onColorChanged = new ColorChangedEvent();

        public static ColorSetting white => new ColorSetting();
        public static ColorSetting red => new ColorSetting(Color.red);
        public static ColorSetting green => new ColorSetting(Color.green);
        public static ColorSetting blue => new ColorSetting(Color.blue);
        public static ColorSetting black => new ColorSetting(Color.black);
        public static ColorSetting grey => new ColorSetting(Color.grey);
        public static ColorSetting yellow => new ColorSetting(Color.yellow);
        public static ColorSetting magenta => new ColorSetting(Color.magenta);
        public static ColorSetting cyan => new ColorSetting(Color.cyan);
        public static ColorSetting clear => new ColorSetting(Color.clear);

        public ColorSetting() { }

        public ColorSetting(string html)
        {
            this.html = html;
        }

        public ColorSetting(Color color)
        {
            this.color = color;
        }

        public Color color
        {
            get { return _color; }
            set
            {
                if(_color == value)
                    return;
                _color = value;
                _html = "#" + ColorUtility.ToHtmlStringRGBA(_color);
                update_tag();
                onColorChanged.Invoke(_color);
            }
        }

        public string html
        {
            get { return _html; }
            set 
            {
                if(_html == value)
                    return;
                _html = value;
                parse();
            }
        }

        public string Tag(string msg) =>
        string.Format(_tag, msg);

        public string Tag(string msg, params object[] args) =>
        Tag(string.Format(msg, args));

        void parse()
        {
            if(!ColorUtility.TryParseHtmlString(_html, out _color))
            {
                Debug.LogFormat("Unable to parse color: {0}", _html);
                _html = "#FFFFFFFF";
                _color = Color.white;
            }
            onColorChanged.Invoke(_color);
            update_tag();
        }

        void update_tag()=> _tag = "<color=" + _html + ">{0}</color>";

        public void addOnColorChangeListner(UnityAction<Color> action) =>
        onColorChanged.AddListener(action);

        public void removeOnColorChangeListner(UnityAction<Color> action) =>
        onColorChanged.RemoveListener(action);

        public static implicit operator Color(ColorSetting c) => c.color;
    }

    public class SimpleGradient
    {
        Gradient gradient;
        ColorSetting[] colors;
        int len, last;

        static readonly GradientAlphaKey[] alpha_keys = {
            new GradientAlphaKey{alpha=1, time=0},
            new GradientAlphaKey{alpha=1, time=1}
        };

        public static implicit operator Gradient(SimpleGradient g) => g.gradient;

        public SimpleGradient() { }

        public SimpleGradient(IEnumerable<ColorSetting> content)
        {
            colors = content.ToArray();
            len = colors.Length;
            last = len - 1;
            update();
            for(int i = 0; i < len; i++)
                colors[i].onColorChanged.AddListener(update);
        }

        ~SimpleGradient()
        {
            for(int i = 0; i < len; i++)
                colors[i].onColorChanged.RemoveListener(update);
        }

        void update(Color _ = default(Color))
        {
            if(len > 1)
            {
                gradient = new Gradient{ mode = GradientMode.Blend };
                GradientColorKey[] keys = new GradientColorKey[len];
                for(int i = 0; i < last; i++)
                    keys[i] = new GradientColorKey { color = colors[i], time = (float)i / len };
                keys[last] = new GradientColorKey { color = colors[last], time = 1 };
                gradient.colorKeys = keys;
                gradient.alphaKeys = alpha_keys;
            }
            else
                gradient = null;
        }

        public Color Evaluate(float frac)
        {
            if(gradient != null)
                return gradient.Evaluate(frac);
            if(len > 0)
                return colors[0];
            return Color.black;
        }
    }
}
