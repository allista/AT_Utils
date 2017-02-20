//
// MathUtils.cs
//
// Author:
//       Allis Tauri <allista@gmail.com>
//
// Copyright (c) 2016 Allis Tauri
//
using System;
using UnityEngine;

namespace AT_Utils
{
	public static partial class Utils
	{
		public const double TwoPI  = Math.PI*2;
		public const double HalfPI = Math.PI/2;
		public const float  Sin45  = 0.70710678f;
		public const float  G0 = 9.80665f; //m/s2

		public static float Clamp(float x, float low, float high)  
		{ return x < low ? low : (x > high? high : x); }

		public static double Clamp(double x, double low, double high)  
		{ return x < low ? low : (x > high? high : x); }

		public static int Clamp(int x, int low, int high)  
		{ return x < low ? low : (x > high? high : x); }

		public static float ClampL(float x, float low)  { return x < low  ? low  : x;  }
		public static double ClampL(double x, double low)  { return x < low  ? low  : x;  }

		public static float ClampH(float x, float high) { return x > high ? high : x;  }
		public static double ClampH(double x, double high) { return x > high ? high : x;  }

		public static int ClampL(int x, int low)  { return x < low  ? low  : x; }
		public static int ClampH(int x, int high) { return x > high ? high : x; }

		public static float Circle(float a, float min, float max)
		{ if(a > max) a = a%max+min; return a < min? max-min+a : a; }
		public static double Circle(double a, double min, double max)
		{ if(a > max) a = a%max+min; return a < min? max-min+a : a; }

		public static float ClampAngle(float a) { a = a%360; return a < 0? 360+a : a; }
		public static double ClampAngle(double a) { a = a%360; return a < 0? 360+a : a; }

		public static float CenterAngle(float a) { return a > 180? a-360 : a; }
		public static double CenterAngle(double a) { return a > 180? a-360 : a; }

		public static float AngleDelta(float a, float b)
		{
			var d = Utils.CenterAngle(b)-Utils.CenterAngle(a);
            return Mathf.Abs(d) > 180? Mathf.Sign(d)*(Mathf.Abs(d)-360) : d;
		}

		public static double AngleDelta(double a, double b)
		{
			var d = Utils.CenterAngle(b)-Utils.CenterAngle(a);
            return Math.Abs(d) > 180? Math.Sign(d)*(Math.Abs(d)-360) : d;
		}

		public static double ClampRad(double a) { a = a%TwoPI; return a < 0? TwoPI+a : a; }
		public static double CenterRad(double a) { return a > Math.PI? a-TwoPI : a; }
		public static double RadDelta(double a, double b)
		{
			var d = Utils.CenterRad(b)-Utils.CenterRad(a);
			return Math.Abs(d) > Math.PI? -Math.Sign(d)*(TwoPI-Math.Abs(d)) : d;
		}

		public static double Acot(double x) { return HalfPI - Math.Atan(x); }

		public static double Haversine(double a) { return (1-Math.Cos(a))/2; }

		/// <summary>
		/// Returns the angle (in degrees) between a radial vector A and the projection 
		/// of a radial vector B on a plane defined by A and tangetA.
		/// The tangentA vector also defines the positive direction from A to B, so 
		/// the returned angle lies in the [-180, 180] interval.
		/// </summary>
		/// <param name="A">Radial vector A.</param>
		/// <param name="B">Radial vector B.</param>
		/// <param name="tangentA">Tangent vector to A.</param>
		public static double ProjectionAngle(Vector3d A, Vector3d B, Vector3d tangentA)
		{
			var Am = A.magnitude;
			var Ba = Vector3d.Dot(B, A)/Am;
			var Bt = Vector3d.Dot(B, Vector3d.Exclude(A, tangentA).normalized);
			return Math.Atan2(Bt, Ba)*Mathf.Rad2Deg;
		}

		public static double ClampedProjectionAngle(Vector3d A, Vector3d B, Vector3d tangentA)
		{ return ClampAngle(ProjectionAngle(A, B, tangentA)); }

		public static float EWA(float old, float cur, float ratio = 0.7f)
		{ return (1-ratio)*old + ratio*cur; }

        public static double EWA(double old, double cur, double ratio = 0.7f)
        { return (1-ratio)*old + ratio*cur; }

		public static Vector3 EWA(Vector3 old, Vector3 cur, float ratio = 0.7f)
		{ return (1-ratio)*old + ratio*cur; }

		public static float CubeSurface(float volume)
		{ return 6*Mathf.Pow(volume, 2/3f); }

		public static double CubeSurface(double volume)
		{ return 6*Math.Pow(volume, 2/3.0); }
	}
}

