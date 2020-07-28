//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri
//
// This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace AT_Utils
{
    //convergent with Anatid's Vector6, but not taken from it
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Vector6
    {
        public static Vector6 zero => new Vector6();

        public Vector3 positive;
        public Vector3 negative;

        public Vector6() { }

        public Vector6(Vector3 pos, Vector3 neg)
        {
            positive = pos;
            negative = neg;
        }

        public Vector6(Vector6 other)
            : this(other.positive, other.negative) { }

        public Vector6(
            float xp,
            float yp,
            float zp,
            float xn,
            float yn,
            float zn
        )
            : this(new Vector3(xp, yp, zp), new Vector3(xn, yn, zn)) { }

        public static Vector6 operator +(Vector6 first, Vector6 second)
        {
            var sum = new Vector6
            {
                positive = first.positive + second.positive, negative = first.negative + second.negative
            };
            return sum;
        }

        public bool IsZero()
        {
            return positive.IsZero() && negative.IsZero();
        }

        public float this[int i]
        {
            get => i < 3 ? positive[i] : negative[i - 3];
            set
            {
                if(i < 3)
                    positive[i] = value;
                else
                    negative[i - 3] = value;
            }
        }

        public void Add(Vector6 vec)
        {
            positive += vec.positive;
            negative += vec.negative;
        }

        public void Add(Vector3 vec)
        {
            for(var i = 0; i < 3; i++)
            {
                if(vec[i] >= 0)
                    positive[i] = positive[i] + vec[i];
                else
                    negative[i] = negative[i] + vec[i];
            }
        }

        public void Add(List<Vector3> vectors)
        {
            vectors.ForEach(Add);
        }

        public Vector3 ClampComponents(Vector3 vec)
        {
            var clampedVector = Vector3.zero;
            for(var i = 0; i < 3; i++)
            {
                var componentI = vec[i];
                clampedVector[i] = componentI >= 0
                    ? Mathf.Min(positive[i], componentI)
                    : Mathf.Max(negative[i], componentI);
            }
            return clampedVector;
        }

        [Obsolete("Use ClampComponents instead")]
        public Vector3 Clamp(Vector3 vec) => ClampComponents(vec);

        public Vector3 ClampMagnitude(Vector3 vec)
        {
            var scale = -1f;
            for(var i = 0; i < 3; i++)
            {
                var componentI = vec[i];
                var boundI = componentI >= 0 ? positive[i] : negative[i];
                var scaleI = componentI / boundI;
                if(scaleI > 1 && scaleI > scale)
                    scale = scaleI;
            }
            return scale > 0 ? vec / scale : vec;
        }

        public Vector6 Inverse(float inf = 0) => new Vector6(positive.Inverse(inf), negative.Inverse(inf));

        public Vector3 Scale(Vector3 vec)
        {
            var scaledVector = Vector3.zero;
            for(var i = 0; i < 3; i++)
            {
                var vi = vec[i];
                scaledVector[i] = vi >= 0
                    ? positive[i] * Mathf.Abs(vi)
                    : negative[i] * Mathf.Abs(vi);
            }
            return scaledVector;
        }

        public void Scale(Vector6 other)
        {
            positive.Scale(other.positive);
            negative.Scale(-other.negative);
        }

        public Vector6 Scaled(Vector6 other)
        {
            var s = new Vector6(this);
            s.Scale(other);
            return s;
        }

        public Vector3 Max
        {
            get
            {
                var maxVector = Vector3.zero;
                for(var i = 0; i < 3; i++)
                    maxVector[i] = Mathf.Max(-negative[i], positive[i]);
                return maxVector;
            }
        }

        public Vector3 Min
        {
            get
            {
                var minVector = Vector3.zero;
                for(var i = 0; i < 3; i++)
                    minVector[i] = Mathf.Min(-negative[i], positive[i]);
                return minVector;
            }
        }

        public Vector3 MaxInPlane(Vector3 normal)
        {
            var maxMagnitude = 0f;
            var maxVector = Vector3.zero;
            var tempVector = Vector3.zero;
            for(var i = 0; i < 3; i++)
            {
                tempVector[i] = positive[i];
                tempVector = Vector3.ProjectOnPlane(tempVector, normal);
                var tempVectorMagnitude = tempVector.sqrMagnitude;
                if(tempVectorMagnitude > maxMagnitude)
                {
                    maxVector = tempVector;
                    maxMagnitude = tempVectorMagnitude;
                }
                tempVector[i] = negative[i];
                tempVector = Vector3.ProjectOnPlane(tempVector, normal);
                tempVectorMagnitude = tempVector.sqrMagnitude;
                if(tempVectorMagnitude > maxMagnitude)
                {
                    maxVector = tempVector;
                    maxMagnitude = tempVectorMagnitude;
                }
                tempVector[i] = 0;
            }
            return maxVector;
        }

        public Vector3 SumInPlane(Vector3 normal)
        {
            var sum = Vector3.zero;
            var tempVector = Vector3.zero;
            for(var i = 0; i < 3; i++)
            {
                tempVector[i] = positive[i];
                sum += Vector3.ProjectOnPlane(tempVector, normal);
                tempVector[i] = negative[i];
                sum += Vector3.ProjectOnPlane(tempVector, normal);
                tempVector[i] = 0;
            }
            return sum;
        }

        public Vector3 Project(Vector3 normal)
        {
            var proj = 0f;
            var tempVector = Vector3.zero;
            for(var i = 0; i < 3; i++)
            {
                tempVector[i] = positive[i];
                var projection = Vector3.Dot(tempVector, normal);
                if(projection > 0)
                    proj += projection;
                tempVector[i] = negative[i];
                projection = Vector3.Dot(tempVector, normal);
                if(projection > 0)
                    proj += projection;
                tempVector[i] = 0;
            }
            return proj * normal;
        }

        public Vector3 Slice(Vector3 normal)
        {
            var sum = Vector3.zero;
            var tempVector = Vector3.zero;
            for(var i = 0; i < 3; i++)
            {
                tempVector[i] = positive[i];
                var projection = Vector3.Dot(tempVector, normal);
                if(projection > 0)
                    sum += tempVector;
                tempVector[i] = negative[i];
                projection = Vector3.Dot(tempVector, normal);
                if(projection > 0)
                    sum += tempVector;
                tempVector[i] = 0;
            }
            return sum;
        }

        public Vector6 Transform(Transform T)
        {
            var tV = new Vector6();
            for(var i = 0; i < 3; i++)
            {
                tV.Add(T.TransformDirection(negative.Component(i)));
                tV.Add(T.TransformDirection(positive.Component(i)));
            }
            return tV;
        }

        public Vector6 InverseTransform(Transform T)
        {
            var tV = new Vector6();
            for(var i = 0; i < 3; i++)
            {
                tV.Add(T.InverseTransformDirection(negative.Component(i)));
                tV.Add(T.InverseTransformDirection(positive.Component(i)));
            }
            return tV;
        }

        public Vector6 Local2Local(Transform fromT, Transform toT)
        {
            var tV = new Vector6();
            for(var i = 0; i < 3; i++)
            {
                tV.Add(toT.InverseTransformDirection(fromT.TransformDirection(negative.Component(i))));
                tV.Add(toT.InverseTransformDirection(fromT.TransformDirection(positive.Component(i))));
            }
            return tV;
        }

        public override string ToString()
        {
            return $"Vector6:\nMax {Max}\n+ {Utils.formatVector(positive)}\n- {Utils.formatVector(negative)}";
        }
    }
}
