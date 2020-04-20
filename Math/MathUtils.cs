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

        public static float ClampSignedH(float x, float high) 
        { return x > 0 ? (x > high? high : x) : (x < -high? -high : x); }

        public static double ClampSignedH(double x, double high)
        { return x > 0 ? (x > high? high : x) : (x < -high? -high : x); }

        public static float ClampSignedL(float x, float low) 
        { return x > 0 ? (x < low? low : x) : (x > -low? -low : x); }

        public static double ClampSignedL(double x, double low)
        { return x > 0 ? (x < low? low : x) : (x > -low? -low : x); }

        public static int Circle(int a, int min, int max)
        { if(a > max) a = a%max+min; return a < min? max-min+a : a; }

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

        /// <summary>
        /// Acr-cosine.
        /// </summary>
        /// <param name="x">The cosine.</param>
        public static double Acot(double x) { return HalfPI - Math.Atan(x); }

        /// <summary>
        /// Haversine of an angle.
        /// See: https://en.wikipedia.org/wiki/Haversine_formula
        /// </summary>
        /// <param name="a">Angle in radians.</param>
        public static double Haversine(double a) { return (1-Math.Cos(a))/2; }

        /// <summary>
        /// Angle2 is a more numerically stable for small angles version of Vector3.Angle method.
        /// </summary>
        /// <param name="a">Vector a.</param>
        /// <param name="b">Vector b.</param>
        public static float Angle2(Vector3 a, Vector3 b)
        {
            var abm = a*b.magnitude;
            var bam = b*a.magnitude;
            return 2 * Mathf.Atan2((abm-bam).magnitude, (abm+bam).magnitude) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Angle2 is a more numerically stable for small angles version of Vector3d.Angle method.
        /// </summary>
        /// <param name="a">Vector a.</param>
        /// <param name="b">Vector b.</param>
        public static double Angle2(Vector3d a, Vector3d b)
        {
            var abm = a*b.magnitude;
            var bam = b*a.magnitude;
            return 2 * Math.Atan2((abm-bam).magnitude, (abm+bam).magnitude) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Angle2 is a more numerically stable for small angles version of Vector2d.Angle method.
        /// </summary>
        /// <param name="a">Vector a.</param>
        /// <param name="b">Vector b.</param>
        public static double Angle2(Vector2d a, Vector2d b)
        {
            var abm = a*b.magnitude;
            var bam = b*a.magnitude;
            return 2 * Math.Atan2((abm-bam).magnitude, (abm+bam).magnitude) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Angle2Rad is a more numerically stable for small angles version of Vector3.Angle method.
        /// Its return value is in radians rather than degrees.
        /// </summary>
        /// <param name="a">Vector a.</param>
        /// <param name="b">Vector b.</param>
        public static float Angle2Rad(Vector3 a, Vector3 b)
        {
            var abm = a*b.magnitude;
            var bam = b*a.magnitude;
            return 2 * Mathf.Atan2((abm-bam).magnitude, (abm+bam).magnitude);
        }

        /// <summary>
        /// Angle2Rad is a more numerically stable for small angles version of Vector3d.Angle method.
        /// Its return value is in radians rather than degrees.
        /// </summary>
        /// <param name="a">Vector a.</param>
        /// <param name="b">Vector b.</param>
        public static double Angle2Rad(Vector3d a, Vector3d b)
        {
            var abm = a*b.magnitude;
            var bam = b*a.magnitude;
            return 2 * Math.Atan2((abm-bam).magnitude, (abm+bam).magnitude);
        }

        /// <summary>
        /// Angle2Rad is a more numerically stable for small angles version of Vector2d.Angle method.
        /// Its return value is in radians rather than degrees.
        /// </summary>
        /// <param name="a">Vector a.</param>
        /// <param name="b">Vector b.</param>
        public static double Angle2Rad(Vector2d a, Vector2d b)
        {
            var abm = a*b.magnitude;
            var bam = b*a.magnitude;
            return 2 * Math.Atan2((abm-bam).magnitude, (abm+bam).magnitude);
        }

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
        { return old + (cur-old)*ratio; }

        public static double EWA(double old, double cur, double ratio = 0.7f)
        { return old + (cur-old)*ratio; }

        public static Vector3 EWA(Vector3 old, Vector3 cur, float ratio = 0.7f)
        { return old + (cur-old)*ratio; }

        /// <summary>
        /// Calculate time needed for an iterative expression
        /// a = Lerp(a, b, speed*dT)
        /// to reach the value at lerp coordinate t.
        /// </summary>
        /// <param name="speed">Lerp speed.</param>
        /// <param name="t">t parameter of the Lerp.</param>
        public static float LerpTime(float speed, float t)
        {
            if(t <= 0) return 0;
            if(t >= 1) return float.PositiveInfinity;
            return 1/speed*Mathf.Log(1/(1-t));
        }

        /// <summary>
        /// Calculate time needed for an iterative expression
        /// a = Lerp(a, b, speed*dT)
        /// to reach the value of c.
        /// </summary>
        /// <param name="a">Lerp start.</param>
        /// <param name="b">Lerp end</param>
        /// <param name="c">Desired lerp value.</param>
        /// <param name="speed">Rate of change from a to b.</param>
        public static float LerpTime(float a, float b, float c, float speed)
        {
            if(c <= a) return 0;
            if(c >= b) return float.PositiveInfinity;
            return 1/speed*Mathf.Log((b-a)/(b-c));
        }

        public static float CubeSurface(float volume)
        { return 6*Mathf.Pow(volume, 2/3f); }

        public static double CubeSurface(double volume)
        { return 6*Math.Pow(volume, 2/3.0); }

        /// <summary>
        /// Clamps vector's direction to the cone around axis.
        /// </summary>
        /// <returns>A new vector that is inside the cone, has the same magnitude as vec 
        /// and lies in the plane of vec*axis. If vec itself lies within the cone, it is returned 
        /// unchanged; otherwise, returnd vector lies just at the edge of the cone.</returns>
        /// <param name="vec">Vector to clamp</param>
        /// <param name="max_angle">Cone angle</param>
        /// <param name="axis">Axis.of the cone</param>
        public static Vector3d ClampDirection(Vector3d vec, Vector3d axis, double max_angle)
        {
            if(Angle2(vec, axis) > max_angle)
                return QuaternionD.AngleAxis(max_angle, Vector3d.Cross(axis, vec)) 
                                  * axis.normalized * vec.magnitude;
            return vec;
        }

        /// <summary>
        /// Clamps vector's direction to the cone around axis.
        /// </summary>
        /// <returns>A new vector that is inside the cone, has the same magnitude as vec 
        /// and lies in the plane of vec*axis. If vec itself lies within the cone, it is returned 
        /// unchanged; otherwise, returnd vector lies just at the edge of the cone.</returns>
        /// <param name="vec">Vector to clamp</param>
        /// <param name="max_angle">Cone angle</param>
        /// <param name="axis">Axis.of the cone</param>
        public static Vector3 ClampDirection(Vector3 vec, Vector3 axis, float max_angle)
        {
            if(Angle2(vec, axis) > max_angle)
                return Quaternion.AngleAxis(max_angle, Vector3.Cross(axis, vec)) 
                                  * axis.normalized * vec.magnitude;
            return vec;
        }

        public static QuaternionD FromToRotation(Vector3d fromV, Vector3d toV)
        {
            var cross = Vector3d.Cross(fromV, toV);
            var dot = Vector3d.Dot(fromV, toV);
            var wval = dot + Math.Sqrt(fromV.sqrMagnitude * toV.sqrMagnitude);
            var norm = 1.0 / Math.Sqrt(cross.sqrMagnitude + wval * wval);
            return new QuaternionD(cross.x * norm, cross.y * norm, cross.z * norm, wval * norm);
        }
    }
}

