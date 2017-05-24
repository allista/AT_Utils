//   IntField.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using UnityEngine;

namespace AT_Utils
{
    public class IntField : ConfigNodeObject, ITypeUI<int>
    {
        string svalue;
        [Persistent] public int  ivalue;
        [Persistent] public int  Min;
        [Persistent] public int  Max;
        [Persistent] public bool Circle;

        public bool IsSet { get; private set; }

        public int Range { get { return Max-Min; } }

        public int Value 
        { 
            get { return ivalue; } 
            set 
            {
                ivalue = Circle? Utils.Circle(value, Min, Max) : Utils.Clamp(value, Min, Max);
                svalue = ivalue.ToString("D");   
            }
        }

        public void Invert()
        {
            Min = -Max; Max = -Min;
            Value = -Value;
        }

        public void ClampValue()
        { Value = Utils.Clamp(Value, Min, Max); }

        public override void Load(ConfigNode node)
        {
            base.Load(node);
            Value = ivalue;
        }

        public static implicit operator int(IntField fi) { return fi.ivalue; }
        public override string ToString () { return ivalue.ToString("D"); }

        public IntField(int min = int.MinValue, int max = int.MaxValue, bool circle = false)
        {
            Circle = circle;
            Min = min; Max = max;
            Value = ivalue;
        }

        public bool UpdateValue()
        {
            int val;
            if(int.TryParse(svalue, out val)) 
            { Value = val; return true; }
            return false;
        }

        public bool Draw(string suffix, bool show_set_button = true, int increment = 0)
        {
            bool updated = false;
            if(!increment.Equals(0)) 
            {
                if(GUILayout.Button(string.Format("-{0}", increment.ToString("D")),     
                                    Styles.normal_button, GUILayout.ExpandWidth(false)))
                { Value = ivalue-increment; updated = true; }
                if(GUILayout.Button(string.Format("+{0}", increment.ToString("D")), 
                                    Styles.normal_button, GUILayout.ExpandWidth(false)))
                { Value = ivalue+increment; updated = true; }
            }
            svalue = GUILayout.TextField(svalue, svalue.Equals(ivalue.ToString("D"))? Styles.green : Styles.white,
                                         GUILayout.ExpandWidth(true), GUILayout.MinWidth(70));
            if(!string.IsNullOrEmpty(suffix)) GUILayout.Label(suffix, Styles.label, GUILayout.ExpandWidth(false));
            IsSet = show_set_button && GUILayout.Button("Set", Styles.normal_button, GUILayout.ExpandWidth(false));
            updated |= IsSet && UpdateValue();
            return updated;
        }

        public bool Draw() { return Draw(""); }
    }
}

