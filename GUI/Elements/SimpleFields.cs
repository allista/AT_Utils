//   StringField.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using UnityEngine;

namespace AT_Utils
{
    public class StringField : ITypeUI<string>
    {
        public string Value { get; set; }

        public bool Draw()
        {
            Value = GUILayout.TextField(Value);
            return true;
        }
    }

    public class BoolField : ITypeUI<bool>
    {
        public bool Value { get; set; }

        public bool Draw()
        {
            if(GUILayout.Button(Value? "True" : "False", Value? Styles.enabled_button : Styles.active_button))
            {
                Value = !Value;
                return true;
            }
            return false;
        }
    }
}

