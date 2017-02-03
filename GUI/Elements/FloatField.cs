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
	public class FloatField : ConfigNodeObject
	{
		string svalue;
		[Persistent] public string format;
		[Persistent] public float  fvalue;
		[Persistent] public float  Min;
		[Persistent] public float  Max;
		[Persistent] public bool   Circle;

		public bool IsSet { get; private set; }

		public float Range { get { return Max-Min; } }

		public float Value 
		{ 
			get { return fvalue; } 
			set 
			{
				fvalue = Circle? Utils.Circle(value, Min, Max) : Utils.Clamp(value, Min, Max);
				svalue = fvalue.ToString(format);	
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
			Value = fvalue;
		}

		public static implicit operator float(FloatField ff) { return ff.fvalue; }
		public override string ToString () { return fvalue.ToString(format); }

		public FloatField(string format = "R", float min = float.MinValue, float max = float.MaxValue, bool circle = false)
		{
			this.format = format;
			Circle = circle;
			Min = min; Max = max;
			Value = fvalue;
		}

		public bool UpdateValue()
		{
			float val;
			if(float.TryParse(svalue, out val)) 
			{ Value = val; return true; }
			return false;
		}

		public bool Draw(string suffix = "", bool show_set_button = true, float increment = 0, string iformat = "F1")
		{
			bool updated = false;
			if(!increment.Equals(0)) 
			{
				if(GUILayout.Button(string.Format("-{0}", increment.ToString(iformat)), 	
				                    Styles.normal_button, GUILayout.ExpandWidth(false)))
				{ Value = fvalue-increment; updated = true; }
				if(GUILayout.Button(string.Format("+{0}", increment.ToString(iformat)), 
				                    Styles.normal_button, GUILayout.ExpandWidth(false)))
				{ Value = fvalue+increment; updated = true; }
			}
			svalue = GUILayout.TextField(svalue, svalue.Equals(fvalue.ToString(format))? Styles.green : Styles.white,
			                             GUILayout.ExpandWidth(true), GUILayout.MinWidth(70));
			if(!string.IsNullOrEmpty(suffix)) GUILayout.Label(suffix, Styles.label, GUILayout.ExpandWidth(false));
			IsSet = show_set_button && GUILayout.Button("Set", Styles.normal_button, GUILayout.ExpandWidth(false));
			updated |= IsSet && UpdateValue();
			return updated;
		}
	}
}

