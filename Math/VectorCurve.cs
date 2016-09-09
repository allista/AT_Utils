//   VectorChain.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System;
using UnityEngine;

namespace AT_Utils
{
	/// <summary>
	/// VectorCurve is a full analog of the FloatCurve for Vector3.
	/// It implements the IConfigNode interface and supports in/out Tangents.
	/// Unlike the stock Vector3Curve, which does none of these.
	/// </summary>
	public class VectorCurve : ConfigNodeObject
	{
		static char[] separators = { ' ', ',', ';', '\t' };

		AnimationCurve x, y, z;
		public float minTime { get; private set; }
		public float maxTime { get; private set; }

		public VectorCurve() { init_curves(); }

		static AnimationCurve new_curve()
		{
			var c = new AnimationCurve();
			c.postWrapMode = WrapMode.ClampForever;
			c.preWrapMode  = WrapMode.ClampForever;
			return c;
		}

		void init_curves()
		{
			x = new_curve();
			y = new_curve();
			z = new_curve();
			minTime = float.MinValue;
			maxTime = float.MaxValue;
		}

		void add(string value)
		{
			var k = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
			if(k.Length < 4)
				throw new FormatException("VectorChain: Invalid keyframe. " +
					"Requires at least four values: 'time, x, y, z'. Got: "+value);
			if(k.Length == 6)
				Add(float.Parse(k[0]), 
				    new Vector3(float.Parse(k[1]), float.Parse(k[2]), float.Parse(k[3])), 
				    float.Parse(k[4]), float.Parse(k[5]));
			else
				Add(float.Parse(k[0]), 
				    new Vector3(float.Parse(k[1]), float.Parse(k[2]), float.Parse(k[3])));
		}

		void update_time_limits(float time)
		{
			minTime = Mathf.Min(minTime, time);
			maxTime = Mathf.Max(maxTime, time);
		}

		public void Add(float time, Vector3 value)
		{
			x.AddKey(time, value.x);
			y.AddKey(time, value.y);
			z.AddKey(time, value.z);
			update_time_limits(time);
		}

		public void Add(float time, Vector3 value, float inTangent, float outTangent)
		{
			x.AddKey(new Keyframe(time, value.x, inTangent, outTangent));
			y.AddKey(new Keyframe(time, value.y, inTangent, outTangent));
			z.AddKey(new Keyframe(time, value.z, inTangent, outTangent));
			update_time_limits(time);
		}

		public Vector3 Evaluate(float time) 
		{ return new Vector3(x.Evaluate(time), y.Evaluate(time), z.Evaluate(time)); }

		public override void Load(ConfigNode node)
		{
			init_curves();
			base.Load(node);
			var keys = node.GetValues("key");
			for(int i = 0; i < keys.Length; i++) add(keys[i]);
			if(x.keys.Length < 2)
				throw new FormatException("VectorChain node must have at least 2 'key' values.");
		}

		public override void Save(ConfigNode node)
		{
			base.Save(node);
			for(int i = 0; i < x.keys.Length; i++)
			{
				var xk = x.keys[i];
				node.AddValue("key", string.Concat(new object[]
				{
					xk.time, 
					xk.value, y.keys[i].value, z.keys[i].value,
					xk.inTangent, xk.outTangent
				}));
			}
		}
	}
}

