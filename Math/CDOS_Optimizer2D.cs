//   CDOS_Optimizer2D.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    /// <summary>
    /// 2D optimizer for a real-value function based on 
    /// Conjugate Directions with Orthogonal Shift algorithm:
    /// https://arxiv.org/abs/1102.1347
    /// </summary>
    public class CDOS_Optimizer2D_Generic : IEnumerable
    {
        struct Point : IEquatable<Point>
        {
            CDOS_Optimizer2D_Generic opt;
            public double x, y, v;

            int feasible;
            public bool IsFeasible
            {
                get
                {
                    if(feasible < 0)
                    {
                        feasible = !double.IsNaN(v) && !double.IsInfinity(v) &&
                            (opt.constraints == null || opt.constraints(x, y, v)) ? 1 : 0;
                    }
                    return feasible.Equals(1);
                }
            }

            public Point(double x, double y, CDOS_Optimizer2D_Generic optimizer)
            {
                this.x = x;
                this.y = y;
                this.v = double.NaN;
                feasible = -1;
                opt = optimizer;
            }

            public void Update()
            {
                v = opt.calculate_value(x, y);
                feasible = -1;
            }

            public void Shift(double dx, double dy, bool update = false)
            { 
                x += dx; 
                y += dy; 
                feasible = -1;
                if(update) Update();
            }

            public static Point operator+(Point p, Vector2d step)
            { 
                var newP = p;
                newP.Shift(step.x, step.y);
                return newP;
            }

            public Point Shifted(double s, double t, bool with_distance = false)
            { 
                var newP = this;
                newP.Shift(s, t, with_distance);
                return newP;
            }

            public static bool operator<(Point a, Point b)
            { return a.v < b.v; }

            public static bool operator>(Point a, Point b)
            { return a.v < b.v; }

            public static Vector2d Delta(Point a, Point b)
            { return new Vector2d(b.x-a.x, b.y-a.y); }

            public static double DistK(Point a, Point b)
            { 
                if(a.IsFeasible && b.IsFeasible)
                    return 1-Math.Abs(a.v-b.v)/Math.Max(a.v, b.v); 
                return 1;
            }

            public static implicit operator Vector3d(Point p)
            { return new Vector3d(p.x, p.y, p.v); }

            #region IEquatable implementation
            public bool Equals(Point other)
            { return x.Equals(other.x) && y.Equals(other.y); }
            #endregion

            public override string ToString()
            {
                return Utils.Format("Point ({}, {}) = {}, IsFeasible: {}", 
                                    x, y, v, IsFeasible);
            }
        }

        Vector2d dir;
        Point P0, P, BestP;
        double delta, xtol, tol;
        double dv = double.MaxValue;
        Func<double,double,double> calculate_value;
        Func<double,double,double,bool> constraints;

        /// <summary>
        /// The best point so far: z = f(x, y)
        /// </summary>
        public Vector3d Best { get { return BestP; } }

        /// <summary>
        /// The value of the function of the best point so far.
        /// </summary>
        public double BestValue { get { return BestP.v; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AT_Utils.CDOS_Optimizer2D_Generic"/> class.
        /// </summary>
        /// <param name="x">The start x coordinate.</param>
        /// <param name="y">The start y coordinate.</param>
        /// <param name="delta">Initial step.</param>
        /// <param name="xtol">Argument tolerance. The search stops when the step of function argument 
        /// becomes smaller than this.</param>
        /// <param name="tol">Function tolerance. The search stops when the difference of function values 
        /// of successive best values becomes smaller than this.</param>
        /// <param name="calculate_value">A real function of two real variables to minimize.</param>
        /// <param name="constraints">A constrint function that takes 3D points of x, y, value; 
        /// returns true if a point is feasible, false otherwise.</param>
        public CDOS_Optimizer2D_Generic(double x, double y, double delta, double xtol, double tol,
                                        Func<double,double,double> calculate_value, 
                                        Func<double,double,double,bool> constraints = null)
        {
            this.delta = delta;
            this.xtol = xtol;
            this.tol = tol;
            this.calculate_value = calculate_value;
            this.constraints = constraints;
            P0 = P = new Point(x, y, this);
            dir = new Vector2d(1,0);
        }

        void set_dir(Vector2d new_dir)
        {
            if(new_dir.x.Equals(0) && new_dir.y.Equals(0)) 
                new_dir[Math.Abs(dir.x) > Math.Abs(dir.y)? 1 : 0] = 1;
            dir = new_dir;
        }

        IEnumerable<Vector2d> find_anti_grad()
        {
            var d0 = P.v;
            var d = delta;
            var Px = default(Point);
            var Py = default(Point);
            while(d > xtol)
            {
                if(!Px.IsFeasible)
                    Px = P.Shifted(d,0, true);
                if(!Py.IsFeasible)
                    Py = P.Shifted(0,d, true);
                if(Px.IsFeasible && Py.IsFeasible)
                {
                    yield return new Vector2d((d0-Px.v)/d, (d0-Py.v)/d).normalized;
                    break;
                }
                d /= -2.1;
                yield return default(Vector2d);
            }
        }

        IEnumerable find_first_point()
        {
            var d = dir * delta;
            //need to limit the search in case of start in non-feasible region
            for(int i = 0; i < 100; i++)
            {
                P.Update();
                yield return null;
                if(P.IsFeasible) break;
                P += d;
            }
        }

        IEnumerable find_minimum(double d, bool strict = false)
        {
            var bestP = P;
            var prev = P;
            var cur = P;
            var path = 0.0;
            const int endL = 50;
            while(Math.Abs(d) > xtol)
            {
                var step_k = Point.DistK(prev,cur);
                prev = cur;
                path += step_k;
                cur += dir*d*step_k;
                cur.Update();
//                Utils.Log("d {} > xtol {}, dir {}, step_k {}, prev {}, cur {} < best {}", 
//                          d, xtol, dir, step_k, prev, cur, bestP);//debug
                if(cur.IsFeasible && cur < bestP)
                {
                    bestP = cur;
                    path = 0.0;
                    if(strict) d *= 1.4;
                }
                else if(strict || path > endL)
                {
                    d /= -2.1;
                    cur = bestP;
                    strict = true;
                }
                yield return null;
            }
            P = bestP;
        }


        IEnumerable orto_shift(double d)
        {
            Point P1;
            var shift_dir = new Vector2d(-dir.y, dir.x);
            var _xtol = d/100;
            while(Math.Abs(d) > _xtol)
            {
                P1 = P+shift_dir*d;
                P1.Update();
                yield return null;
                if(P1.IsFeasible)
                {
                    P = P1;
                    break;
                }
                d /= -2;
            }
        }

        IEnumerable shift_and_find(double d, double stride = 1)
        {
            P0 = P;
            foreach(var t in orto_shift(0.62*d)) yield return t;
            foreach(var t in find_minimum(d, true)) yield return t;
            if(P0 < P) 
            {
                set_dir(Point.Delta(P, P0).normalized);
                P = P0;
            }
            else set_dir(Point.Delta(P0, P).normalized);
            foreach(var t in find_minimum(stride*d)) yield return t;
            dv = P.v.Equals(double.MaxValue) || P0.v.Equals(double.MaxValue)? 
                double.MaxValue : Math.Abs(P.v-P0.v);
            if(P0 < P) 
            {
                P = P0;
                yield return null;
            }
        }

        IEnumerable build_conjugate_set()
        {
            Vector2d new_dir = dir;
            foreach(var t in find_anti_grad()) 
            {
                new_dir = t;
                yield return t;
            }
            set_dir(new_dir);
            foreach(var t in find_minimum(delta)) yield return t;
            foreach(var t in shift_and_find(delta)) yield return t;
        }

        public IEnumerator GetEnumerator()
        {
            Utils.Log("CDOS Start");//debug
            foreach(var t in find_first_point())
                yield return t;
            BestP = P;
            Utils.Log("CDOS First: Best {}", P);//debug
            if(!P.IsFeasible) yield break;
            foreach(var t in build_conjugate_set())
                yield return t;
            Utils.Log("CDOS Set: Best {}", P);//debug
            BestP = P;
            var d = delta;
            while(d > xtol && dv > tol)
            {
                foreach(var t in shift_and_find(d, 3)) yield return t;
                d = 0.3 * Point.Delta(P0, P).magnitude + 0.1 * d;
                if(d.Equals(0)) d = delta*0.1;
                BestP = P;
                Utils.Log("CDOS Iter: Best {}, d {}, dv {}", P, d, dv);//debug
            }
        }
    }
}

