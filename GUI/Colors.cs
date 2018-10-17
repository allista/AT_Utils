//   Colors.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using UnityEngine;
namespace AT_Utils
{
    public class Colors : ConfigNodeObject
    {
        [Persistent] public PersistentColor Enabled = PersistentColor.green;
        [Persistent] public PersistentColor Active = PersistentColor.yellow;
        [Persistent] public PersistentColor Inactive = PersistentColor.grey;
        [Persistent] public PersistentColor Confirm = PersistentColor.green;
        [Persistent] public PersistentColor Open = PersistentColor.green;
        [Persistent] public PersistentColor Close = PersistentColor.red;
        [Persistent] public PersistentColor Good = PersistentColor.green;
        [Persistent] public PersistentColor Warning = PersistentColor.yellow;
        [Persistent] public PersistentColor Danger = PersistentColor.red;
        [Persistent] public PersistentColor Selected1 = PersistentColor.cyan;
        [Persistent] public PersistentColor Selected2 = PersistentColor.magenta;

        [Persistent]
        public Gradient FractionGradient = new Gradient{
            PersistentColor.red,
            PersistentColor.yellow,
            PersistentColor.white,
            PersistentColor.green
        };
    }

    public class PersistentColor : ConfigNodeObject
    {
        [Persistent] string html = "#FFFFFF";
        Color _color = Color.white;

        public static PersistentColor white => new PersistentColor();
        public static PersistentColor red => new PersistentColor(Color.red);
        public static PersistentColor green => new PersistentColor(Color.green);
        public static PersistentColor blue => new PersistentColor(Color.blue);
        public static PersistentColor black => new PersistentColor(Color.black);
        public static PersistentColor grey => new PersistentColor(Color.grey);
        public static PersistentColor yellow => new PersistentColor(Color.yellow);
        public static PersistentColor magenta => new PersistentColor(Color.magenta);
        public static PersistentColor cyan => new PersistentColor(Color.cyan);
        public static PersistentColor clear => new PersistentColor(Color.clear);

        public PersistentColor() { }

        public PersistentColor(string html)
        {
            this.html = html;
            parse();
        }

        public PersistentColor(Color color)
        {
            c = color;
        }

        public string s => html;

        public Color c
        {
            get { return _color; }
            set
            {
                _color = value;
                html = "#"+ColorUtility.ToHtmlStringRGBA(_color);
            }
        }

        public string Tag(string msg) =>
        string.Format("<color={0}>{1}</color>", html, msg);

        void parse()
        {
            if(!ColorUtility.TryParseHtmlString(html, out _color))
            {
                html = "#FFFFFF";
                c = Color.white;
                Utils.Log("Unable to parse color: {}", html);
            }
        }

        public override void Load(ConfigNode node)
        {
            base.Load(node);
            parse();
        }

        public static implicit operator Color(PersistentColor c) => c.c;
    }

    public class Gradient : PersistentList<PersistentColor>
    {
        public Color Evaluate(float frac)
        {
            if(Count > 0)
            {
                var max = Count - 1;
                var index = Mathf.FloorToInt(max * Utils.Clamp(frac, 0, 1));
                var rem = frac * max - index;
                return index < max ?
                    Color.Lerp(this[index], this[index + 1], rem) : this[index];
            }
            return Color.black;
        }
    }
}
