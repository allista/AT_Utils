//
// FloatField.cs
//
// Author:
//       Allis Tauri <allista@gmail.com>
//
// Copyright (c) 2016 Allis Tauri

using UnityEngine;

namespace AT_Utils
{
    public class FloatField : BoundValueField<float>
    {
        public override float Range { get { return Max-Min; } }

        protected override string formated_value
        { 
            get 
            { 
                return _value.ToString(format); 
            }
        }

        public override float Value 
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

        public FloatField(string format = "R", float min = float.MinValue, float max = float.MaxValue, bool circle = false)
            : base(format, min, max, circle) {}

        public override bool UpdateValue()
        {
            float val;
            if(float.TryParse(svalue, out val)) 
            { Value = val; return true; }
            return false;
        }

        public bool Draw(string suffix, float increment = 0, string iformat = "F1", int suffix_width = -1, GUIStyle style = null)
        {
            bool updated = false;
            GUILayout.BeginHorizontal();
            if(!increment.Equals(0)) 
            {
                if(GUILayout.Button(string.Format("-{0}", increment.ToString(iformat)),     
                                    Styles.normal_button, GUILayout.ExpandWidth(false)))
                { Value = _value-increment; updated = true; }
                if(GUILayout.Button(string.Format("+{0}", increment.ToString(iformat)), 
                                    Styles.normal_button, GUILayout.ExpandWidth(false)))
                { Value = _value+increment; updated = true; }
            }
            GUI.SetNextControlName(field_name);
            svalue = GUILayout.TextField(svalue, svalue.Equals(formated_value)? (style ?? Styles.green) : Styles.white,
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

