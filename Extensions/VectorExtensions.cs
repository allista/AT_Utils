using System;
using UnityEngine;

namespace AT_Utils
{
    public static class VectorExtensions
    {
        public static bool IsNaN(this Vector3d v) =>
            double.IsNaN(v.x) || double.IsNaN(v.y) || double.IsNaN(v.z);

        public static bool IsNaN(this Vector3 v) =>
            float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);

        public static bool IsInf(this Vector3d v) =>
            double.IsInfinity(v.x) || double.IsInfinity(v.y) || double.IsInfinity(v.z);

        public static bool IsInf(this Vector3 v) =>
            float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z);

        public static bool Invalid(this Vector3d v) => v.IsNaN() || v.IsInf();

        public static bool Invalid(this Vector3 v) => v.IsNaN() || v.IsInf();

        public static Vector3 CubeNorm(this Vector3 v)
        {
            if(v.IsZero())
                return v;
            var max = -1f;
            for(int i = 0; i < 3; i++)
            {
                var ai = Mathf.Abs(v[i]);
                if(max < ai)
                    max = ai;
            }
            return v / max;
        }

        public static Vector3d CubeNorm(this Vector3d v)
        {
            if(v.IsZero())
                return v;
            var max = -1.0;
            for(int i = 0; i < 3; i++)
            {
                var ai = Math.Abs(v[i]);
                if(max < ai)
                    max = ai;
            }
            return v / max;
        }

        public static Vector3 Inverse(this Vector3 v, float inf = float.MaxValue) =>
            new Vector3(v.x.Equals(0) ? inf : 1 / v.x,
                v.y.Equals(0) ? inf : 1 / v.y,
                v.z.Equals(0) ? inf : 1 / v.z);

        public static Vector3d Inverse(this Vector3d v, double inf = double.MaxValue) =>
            new Vector3d(v.x.Equals(0) ? inf : 1 / v.x,
                v.y.Equals(0) ? inf : 1 / v.y,
                v.z.Equals(0) ? inf : 1 / v.z);

        public static Vector3 ScaleChain(this Vector3 vec, params Vector3[] vectors)
        {
            var result = vec;
            for(int i = 0, vectorsLength = vectors.Length; i < vectorsLength; i++)
            {
                var v = vectors[i];
                result.x *= v.x;
                result.y *= v.y;
                result.z *= v.z;
            }
            return result;
        }

        public static Vector3d ScaleChain(this Vector3d vec, params Vector3d[] vectors)
        {
            var result = vec;
            for(int i = 0, vectorsLength = vectors.Length; i < vectorsLength; i++)
            {
                var v = vectors[i];
                result.x *= v.x;
                result.y *= v.y;
                result.z *= v.z;
            }
            return result;
        }

        public static Vector3 SquaredComponents(this Vector3 v) =>
            new Vector3(v.x * v.x, v.y * v.y, v.z * v.z);

        public static Vector3d SquaredComponents(this Vector3d v) =>
            new Vector3d(v.x * v.x, v.y * v.y, v.z * v.z);

        public static Vector3 SqrtComponents(this Vector3 v) =>
            new Vector3(Mathf.Sqrt(v.x), Mathf.Sqrt(v.y), Mathf.Sqrt(v.z));

        public static Vector3d SqrtComponents(this Vector3d v) =>
            new Vector3d(Math.Sqrt(v.x), Math.Sqrt(v.y), Math.Sqrt(v.z));

        public static Vector3 PowComponents(this Vector3 v, float pow) =>
            new Vector3(Mathf.Pow(v.x, pow), Mathf.Pow(v.y, pow), Mathf.Pow(v.z, pow));

        public static Vector3d PowComponents(this Vector3d v, double pow) =>
            new Vector3d(Math.Pow(v.x, pow), Math.Pow(v.y, pow), Math.Pow(v.z, pow));

        public static Vector3 ClampComponents(this Vector3 v, float min, float max) =>
            new Vector3(Mathf.Clamp(v.x, min, max),
                Mathf.Clamp(v.y, min, max),
                Mathf.Clamp(v.z, min, max));

        public static Vector3 ClampComponents(this Vector3 v, Vector3 min, Vector3 max) =>
            new Vector3(Mathf.Clamp(v.x, min.x, max.x),
                Mathf.Clamp(v.y, min.y, max.y),
                Mathf.Clamp(v.z, min.z, max.z));

        public static Vector3 ClampComponentsL(this Vector3 v, Vector3 min) =>
            new Vector3(Utils.ClampL(v.x, min.x),
                Utils.ClampL(v.y, min.y),
                Utils.ClampL(v.z, min.z));

        public static Vector3 ClampComponentsH(this Vector3 v, Vector3 max) =>
            new Vector3(Utils.ClampH(v.x, max.x),
                Utils.ClampH(v.y, max.y),
                Utils.ClampH(v.z, max.z));

        public static Vector3d ClampComponents(this Vector3d v, double min, double max) =>
            new Vector3d(Utils.Clamp(v.x, min, max),
                Utils.Clamp(v.y, min, max),
                Utils.Clamp(v.z, min, max));

        public static Vector3d ClampComponents(this Vector3d v, Vector3d min, Vector3d max) =>
            new Vector3d(Utils.Clamp(v.x, min.x, max.x),
                Utils.Clamp(v.y, min.y, max.y),
                Utils.Clamp(v.z, min.z, max.z));

        public static Vector3d ClampComponentsH(this Vector3d v, Vector3d max) =>
            new Vector3d(Utils.ClampH(v.x, max.x),
                Utils.ClampH(v.y, max.y),
                Utils.ClampH(v.z, max.z));

        public static Vector3d ClampComponentsL(this Vector3d v, Vector3d min) =>
            new Vector3d(Utils.ClampL(v.x, min.x),
                Utils.ClampL(v.y, min.y),
                Utils.ClampL(v.z, min.z));

        public static Vector3 ClampComponentsH(this Vector3 v, float max) =>
            new Vector3(Utils.ClampH(v.x, max),
                Utils.ClampH(v.y, max),
                Utils.ClampH(v.z, max));

        public static Vector3 ClampComponentsL(this Vector3 v, float min) =>
            new Vector3(Utils.ClampL(v.x, min),
                Utils.ClampL(v.y, min),
                Utils.ClampL(v.z, min));

        public static Vector3d ClampComponentsH(this Vector3d v, double max) =>
            new Vector3d(Utils.ClampH(v.x, max),
                Utils.ClampH(v.y, max),
                Utils.ClampH(v.z, max));

        public static Vector3d ClampComponentsL(this Vector3d v, double min) =>
            new Vector3d(Utils.ClampL(v.x, min),
                Utils.ClampL(v.y, min),
                Utils.ClampL(v.z, min));


        public static Vector3 ClampMagnitudeH(this Vector3 v, float max)
        {
            var vm = v.magnitude;
            return vm > max ? v / vm * max : v;
        }

        public static Vector3d ClampMagnitudeH(this Vector3d v, double max)
        {
            var vm = v.magnitude;
            return vm > max ? v / vm * max : v;
        }

        public static Vector3d ClampMagnitudeL(this Vector3d v, double min)
        {
            var vm = v.magnitude;
            return vm < min ? v / vm * min : v;
        }

        public static Vector3 Sign(this Vector3 v) =>
            new Vector3(Mathf.Sign(v.x), Mathf.Sign(v.y), Mathf.Sign(v.z));

        public static Vector3 AbsComponents(this Vector3 v) =>
            new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        public static int MaxI(this Vector3 v)
        {
            var maxi = 0;
            var max = 0f;
            for(int i = 0; i < 3; i++)
            {
                if(Mathf.Abs(v[i]) > Mathf.Abs(max))
                {
                    max = v[i];
                    maxi = i;
                }
            }
            return maxi;
        }

        public static int MinI(this Vector3 v)
        {
            var mini = 0;
            var min = float.MaxValue;
            for(int i = 0; i < 3; i++)
            {
                if(Mathf.Abs(v[i]) < Mathf.Abs(min))
                {
                    min = v[i];
                    mini = i;
                }
            }
            return mini;
        }

        public static int MaxI(this Vector3d v)
        {
            var maxi = 0;
            var max = 0.0;
            for(int i = 0; i < 3; i++)
            {
                if(Math.Abs(v[i]) > Math.Abs(max))
                {
                    max = v[i];
                    maxi = i;
                }
            }
            return maxi;
        }

        public static int MinI(this Vector3d v)
        {
            var mini = 0;
            var min = double.MaxValue;
            for(int i = 0; i < 3; i++)
            {
                if(Math.Abs(v[i]) < Math.Abs(min))
                {
                    min = v[i];
                    mini = i;
                }
            }
            return mini;
        }

        public static Vector3 Component(this Vector3 v, int i)
        {
            var ret = Vector3.zero;
            ret[i] = v[i];
            return ret;
        }

        public static Vector3 Exclude(this Vector3 v, int i)
        {
            var ret = v;
            ret[i] = 0;
            return ret;
        }

        public static Vector3d Component(this Vector3d v, int i)
        {
            var ret = Vector3d.zero;
            ret[i] = v[i];
            return ret;
        }

        public static Vector3d Exclude(this Vector3d v, int i)
        {
            var ret = v;
            ret[i] = 0;
            return ret;
        }

        public static Vector3 MaxComponentV(this Vector3 v) => v.Component(v.MaxI());

        public static Vector3 MinComponentV(this Vector3 v) => v.Component(v.MinI());

        public static Vector3d MaxComponentV(this Vector3d v) => v.Component(v.MaxI());

        public static Vector3d MinComponentV(this Vector3d v) => v.Component(v.MinI());

        public static float MaxComponentF(this Vector3 v) => v[v.MaxI()];

        public static float MinComponentF(this Vector3 v) => v[v.MinI()];

        public static double MaxComponentD(this Vector3d v) => v[v.MaxI()];

        public static double MinComponentD(this Vector3d v) => v[v.MinI()];

        public static Vector3 xzy(this Vector3 v) => new Vector3(v.x, v.z, v.y);

        public static Vector2d Rotate(this Vector2d v, double angle)
        {
            angle *= Mathf.Deg2Rad;
            var sin = Math.Sin(angle);
            var cos = Math.Cos(angle);
            return new Vector2d(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
        }

        public static Vector2d RotateRad(this Vector2d v, double angle)
        {
            var sin = Math.Sin(angle);
            var cos = Math.Cos(angle);
            return new Vector2d(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
        }

        public static Vector2d Rotate90(this Vector2d v) => new Vector2d(-v.y, v.x);

        public static Vector3 Local2LocalDir(this Vector3 vec, Transform from, Transform to) =>
            to.InverseTransformDirection(from.TransformDirection(vec));

        public static Vector3d Local2LocalDir(this Vector3d vec, Transform from, Transform to) =>
            to.InverseTransformDirection(from.TransformDirection(vec));

        public static Vector3 Local2Local(this Vector3 vec, Transform from, Transform to) =>
            to.InverseTransformPoint(from.TransformPoint(vec));

        public static Vector3d Local2Local(this Vector3d vec, Transform from, Transform to) =>
            to.InverseTransformPoint(from.TransformPoint(vec));

        public static Vector3 TransformPointUnscaled(this Transform T, Vector3 local) =>
            T.position + T.TransformDirection(local);

        public static Vector3 InverseTransformPointUnscaled(this Transform T, Vector3 world) =>
            T.InverseTransformDirection(world - T.position);
    }
}
