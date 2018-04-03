//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    #region Timers
    public abstract class TimerBase<T> where T : IComparable
    {
        protected T next_time;
        public double Period;
        public Action action = delegate {};

        protected TimerBase(double period) { Period = period; }
        public abstract void Reset();
    }

    public abstract class Timer<T> : TimerBase<T> where T : IComparable
    {
        protected abstract T now { get; }
        protected abstract T default_time { get; }

        protected Timer(double period) : base(period) { next_time = default_time; }

        protected abstract void start();

        /// <summary>
        /// True if this <see cref="AT_Utils.Timer"/> is started.
        /// </summary>
        /// <value><c>true</c> if started; otherwise, <c>false</c>.</value>
        public bool Started { get { return !next_time.Equals(default_time); } }

        /// <summary>
        /// Start this timer if it is not running.
        /// <returns><c>true</c> if the timer was not running; <c><false/c> otherwise.</returns>
        /// </summary>
        public bool Start() 
        { 
            if(Started) return false;
            start(); return true;
        }

        /// <summary>
        /// Reset the timer.
        /// </summary>
        public override void Reset() { next_time = default_time; }

        /// <summary>
        /// Reset, then Start.
        /// </summary>
        public void Restart() { Reset(); Start(); }

        /// <summary>
        /// Gets the remaining time.
        /// </summary>
        /// <value>The remaining time.</value>
        public abstract double Remaining { get; }

        /// <summary>
        /// True if the <see cref="AT_Utils.Timer.Period"/> has passed since Start.
        /// </summary>
        /// <value><c>true</c> if time passed; otherwise, <c>false</c>.</value>
        public bool TimePassed
        {
            get 
            {
                if(Start()) return false;
                return next_time.CompareTo(now) < 0;
            }
        }

        /// <summary>
        /// Starts the timer if the predicate is <c>true</c>; Resets the timer otherwise.
        /// </summary>
        /// <returns><c>true</c>, if the timer was started, <c>false</c> otherwise.</returns>
        /// <param name="predicate">A preevaluated boolean condition.</param>
        public bool StartIf(bool predicate)
        {
            if(predicate) return Start();
            Reset(); return false;
        }

        /// <summary>
        /// Runs the action if the predicate is true and the TimePassed; Resets the timer otherwise.
        /// </summary>
        /// <returns><c>true</c>, if the time has passed and the action was run, <c>false</c> otherwise.</returns>
        /// <param name="action">An <c>>Action</c> to be run.</param>
        /// <param name="predicate">A preevaluated boolean condition.</param>
        public bool RunIf(Action action, bool predicate)
        {
            if(predicate) { if(TimePassed) { action(); Reset(); return true; } }
            else Reset(); 
            return false;
        }
        public bool RunIf(bool predicate) { return RunIf(action, predicate); }

        public override string ToString()
        {
            var time = now;
            return string.Format("Started: {0}. TimePassed: [{1} > next {2}]: {3}", 
                                 Started, time, next_time, Started && next_time.CompareTo(time) < 0);
        }
    }

    public class ActionDamper : TimerBase<DateTime>
    {
        public ActionDamper(double period = 0.1) : base(period) { next_time = DateTime.MinValue; }

        public override void Reset() { next_time = DateTime.MinValue; }

        public void Run(Action action)
        {
            var time = DateTime.Now;
            if(next_time > time) return;
            next_time = time.AddSeconds(Period);
            action();
        }

        public void Run() { Run(action); }
    }

    public class RealTimer : Timer<DateTime>
    {
        protected override DateTime now { get { return DateTime.Now; } }
        protected override DateTime default_time { get { return DateTime.MinValue; } }
        public RealTimer(double period = 1) : base(period) { next_time = default_time; }
        protected override void start() { next_time = now.AddSeconds(Period); }
        public override double Remaining { get { return next_time.Subtract(now).TotalSeconds; } }
    }

    public class Timer : Timer<double>
    {
        protected override double now { get { return Planetarium.GetUniversalTime(); } }
        protected override double default_time { get { return -1; } }
        public Timer(double period = 1) : base(period) { next_time = default_time; }
        protected override void start() { next_time = now+Period; }
        public override double Remaining { get { return next_time-now; } }
    }

    public class Blinker : RealTimer
    {
        public Blinker(double period = 1) : base(period) { next_time = default_time; }

        bool state = false;
        public bool On 
        { 
            get
            {
                if(TimePassed) 
                {
                    state = !state;
                    Reset();
                }
                return state;
            }
        }
    }

    public class MemoryTimer : IEnumerator<YieldInstruction>
    {
        public delegate void Callback();

        public bool  Active = true;
        public float WaitPeriod = 1f;
        public Callback EndAction = null;

        public YieldInstruction Current
        {
            get
            {
                Active = false;
                return new WaitForSeconds(WaitPeriod);
            }
        }
        object IEnumerator.Current { get { return Current; } }

        public bool MoveNext() 
        { 
            if(!Active && EndAction != null) 
                EndAction();
            return Active; 
        }

        public void Reset() { Active = true; }

        public void Dispose() {}
    }
    #endregion

    public class Switch
    {
        bool state, prev_state;

        public void Set(bool s)
        { prev_state = state; state = s; }

        public bool WasSet { get { return state != prev_state; } }
        public bool On { get { return state; } }
        public void Checked() { prev_state = state; }

        public static implicit operator bool(Switch s) { return s.state; }
    }

    public class SingleAction
    {
        bool done;
        public Action action;

        public void Run(Action action) 
        { if(!done) { action(); done = true; } }

        public void Run() { Run(action); }

        public void Reset() { done = false; }

        public static implicit operator bool(SingleAction a) { return a.done; }
    }
}

