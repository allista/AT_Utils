//   IntField.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using UnityEngine;

namespace AT_Utils
{
    public class IntField : BoundValueField<int>
    {
        public override int Range { get { return Max-Min; } }

        protected override string formated_value
        {
            get
            {
                return _value.ToString("D");
            }
        }

        public override int Value 
        { 
            get { return _value; } 
            set 
            {
                _value = Circle? Utils.Circle(value, Min, Max) : Utils.Clamp(value, Min, Max);
                svalue = formated_value;
            }
        }

        public override void Invert()
        {
            Min = -Max; Max = -Min;
            Value = -Value;
        }

        public override void ClampValue()
        { Value = Utils.Clamp(Value, Min, Max); }

        public IntField(int min = int.MinValue, int max = int.MaxValue, bool circle = false)
            : base("D", min, max, circle) {}

        public override void Load(ConfigNode node)
        {
            base.Load(node);
            Value = _value;
        }

        public override bool UpdateValue()
        {
            int val;
            if(int.TryParse(svalue, out val)) 
            { Value = val; return true; }
            return false;
        }

        public bool Draw(string suffix, int increment = 0, int suffix_width = -1, GUIStyle style = null)
        {
            bool updated = false;
            GUILayout.BeginHorizontal();
            if(!increment.Equals(0)) 
            {
                if(GUILayout.Button(string.Format("-{0}", increment.ToString("D")),     
                                    Styles.normal_button, GUILayout.ExpandWidth(false)))
                { Value = _value-increment; updated = true; }
                if(GUILayout.Button(string.Format("+{0}", increment.ToString("D")), 
                                    Styles.normal_button, GUILayout.ExpandWidth(false)))
                { Value = _value+increment; updated = true; }
            }
            GUI.SetNextControlName(field_name);
            svalue = GUILayout.TextField(svalue, svalue.Equals(formated_value)? (style ?? Styles.enabled) : Styles.white,
                                         GUILayout.ExpandWidth(true), GUILayout.MinWidth(70));
            if(!string.IsNullOrEmpty(suffix)) 
                GUILayout.Label(suffix, Styles.label, suffix_width < 0? 
                                GUILayout.ExpandWidth(false) : GUILayout.Width(suffix_width));
            GUILayout.EndHorizontal();
            updated |= TrySetValue();
            return updated;
        }

        public override bool Draw() { return Draw(""); }
    }
}

