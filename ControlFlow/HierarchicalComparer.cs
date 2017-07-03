//   HierarchicalContition.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using System;
using System.Collections.Generic;

namespace AT_Utils
{
    /// <summary>
    /// This is a composite boolean comparer that answers the question "is X better than Y?"
    /// using a hierarchy of conditions. It is designed to encapsulate multiple constrains of an 
    /// optimization problem, which have different "importance".
    /// 
    /// A condition itself can answer one of the two questions:
    /// 
    /// 1. Is X good or bad? If X is bad and Y is bad, which is better?
    /// If "X is good", then the next condition in the hierarchy is checked.
    /// If "X is NOT good", than X is compared with Y and the result is returned.
    /// 
    /// 2. Which is better, X or Y?
    /// The X is compared with Y and the result is returned, 
    /// so conditions below it in the hierarchy will NEVER be checked.
    /// </summary>
    public class HierarchicalComparer<T>
    {
        public delegate bool Constraint(T x);
        public delegate bool Comparison(T x, T y);

        public struct Condition
        {
            public Constraint isGood;
            public Comparison isBetter;
        }

        List<Condition> conditions = new List<Condition>();

        public static Condition MakeCondition(Constraint isGood, Comparison isBetter)
        {
            return new Condition{isGood = isGood, isBetter = isBetter};
        }

        public HierarchicalComparer() {}
        public HierarchicalComparer(params Condition[] many_conditions)
        {
            AddConditions(many_conditions);
        }

        public void AddCondition(Constraint isGood, Comparison isBetter)
        {
            AddCondition(MakeCondition(isGood, isBetter));
        }

        public void AddCondition(Condition condition)
        {
            if(condition.isBetter == null)
                throw new ArgumentException("condition.isBetter cannot be null");
            conditions.Add(condition);
        }

        public void AddConditions(params Condition[] many_conditions)
        {
            for(int i = 0, len = many_conditions.Length; i < len; i++)
                AddCondition(many_conditions[i]);
        }

        public void Clear() { conditions.Clear(); }

        public bool isBetter(T x, T y)
        {
            if(x.Equals(y)) return false;
            for(int i = 0, count = conditions.Count; i < count; i++)
            {
                var c = conditions[i];
                if(c.isGood == null)
                    return c.isBetter(x, y);
                if(c.isGood(x))
                    continue;
                if(c.isGood(y))
                    return false;
                return c.isBetter(x, y);
            }
            return true;
        }
    }
}

