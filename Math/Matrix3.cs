//
// Matrix3.cs
//
// Author:
//       Allis Tauri <allista@gmail.com>
//
// Copyright (c) 2016 Allis Tauri

using UnityEngine;

namespace AT_Utils
{
    //from MechJeb2
    public class Matrix3x3f
    {
        //row index, then column index
        float[,] e = new float[3, 3];
        public float this[int i, int j]
        {
            get { return e[i, j]; }
            set { e[i, j] = value; }
        }

        public void Add(int i, int j, float v) { e[i, j] += v; }

        public Matrix3x3f transpose()
        {
            var ret = new Matrix3x3f();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                    ret.e[i, j] = e[j, i];
            }
            return ret;
        }

        public static Vector3 operator *(Matrix3x3f M, Vector3 v)
        {
            Vector3 ret = Vector3.zero;
            for(int i = 0; i < 3; i++) {
                for(int j = 0; j < 3; j++) {
                    ret[i] += M.e[i, j] * v[j];
                }
            }
            return ret;
        }
    }
}

