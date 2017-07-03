//   Vector3Field.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System;
using UnityEngine;

namespace AT_Utils
{
    public class Vector3Field : ITypeUI<Vector3>
	{
		public FloatField Fx = new FloatField();
		public FloatField Fy = new FloatField();
		public FloatField Fz = new FloatField();

		public Vector3 Value
		{
			get { return new Vector3(Fx.Value, Fy.Value, Fz.Value); }
			set 
			{
				Fx.Value = value.x;
				Fy.Value = value.y;
				Fz.Value = value.z;
			}
		}

		public bool UpdateValue()
		{
			var updated = false;
			updated = Fx.UpdateValue() || updated;
			updated = Fy.UpdateValue() || updated;
			updated = Fz.UpdateValue() || updated;
			return updated;
		}

		public bool Draw(string suffix, bool show_set_button = true, float increment = 0, string iformat = "F1")
		{
			var ret = false;
			GUILayout.BeginHorizontal();
			ret = Fx.Draw("", increment, iformat) || ret;
			ret = Fy.Draw("", increment, iformat) || ret;
			ret = Fz.Draw(suffix, increment, iformat) || ret;
			if(Fz.IsSet) ret = UpdateValue() || ret;
			GUILayout.EndHorizontal();
			return ret;
		}

        public bool Draw() { return Draw(""); }
	}
}

