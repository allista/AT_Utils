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

        public static SimpleGradient FractionGradient = new SimpleGradient{};

        public static SortedList<string, ColorSetting> All { get; } 

        static Colors()
        {
            All = new SortedList<string, ColorSetting>();
            foreach(var fi in typeof(Colors).GetFields(BindingFlags.Static|BindingFlags.Public)
                    .Where(fi => fi.FieldType == typeof(ColorSetting)))
                All.Add(fi.Name, fi.GetValue(null) as ColorSetting);
            FractionGradient = new SimpleGradient { Danger, Warning, Good };
        }

        public static ColorSetting GetColor(string key)
        {
            ColorSetting color;
            if(All.TryGetValue(key, out color))
                return color;
            return null;
        }

        public IColored GetColored(string key) => GetColor(key);
    }

    [Serializable]
    public class ColorSetting : IColored
    {
        Color _color = Color.white;
        public string _html = "#FFFFFFFF";
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
                _color = value;
                _html = "#" + ColorUtility.ToHtmlStringRGBA(_color);
                onColorChanged.Invoke(_color);
            }
        }

        public string html
        {
            get { return _html; }
            set 
            {
                _html = value;
                parse();
            }
        }

        public string Tag(string msg) =>
        string.Format("<color={0}>{1}</color>", _html, msg);

        void parse()
        {
            if(!ColorUtility.TryParseHtmlString(_html, out _color))
            {
                Debug.LogFormat("Unable to parse color: {0}", _html);
                _html = "#FFFFFFFF";
                _color = Color.white;
            }
            onColorChanged.Invoke(_color);
        }

        public static implicit operator Color(ColorSetting c) => c.color;
    }

    public class SimpleGradient : List<ColorSetting>
    {
        Gradient gradient;

        public static implicit operator Gradient(SimpleGradient g) => g.gradient;

        public SimpleGradient() { }

        public SimpleGradient(IEnumerable<ColorSetting> content)
            : base(content)
        { 
            update(); 
            ForEach(cs => cs.onColorChanged.AddListener(c => update()));
        }

        void update()
        {
            if(Count > 1)
            {
                gradient = new Gradient();
                gradient.mode = GradientMode.Blend;
                GradientColorKey[] colors = new GradientColorKey[Count];
                for(int i = 0, count = Count; i < count; i++)
                    colors[i] = new GradientColorKey { color = this[i], time = (float)i / count };
                gradient.colorKeys = colors;
                gradient.alphaKeys = new[]{
                    new GradientAlphaKey{alpha=1, time=0},
                    new GradientAlphaKey{alpha=1, time=1}
                };
            }
            else
                gradient = null;
        }

        public Color Evaluate(float frac)
        {
            if(gradient != null)
                return gradient.Evaluate(frac);
            if(Count > 0)
                return this[0];
            return Color.black;
        }
    }
}
