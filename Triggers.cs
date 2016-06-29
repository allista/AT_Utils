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
		public Action action;

		protected TimerBase(double period) { Period = period; }
		public abstract void Reset();
	}

	public abstract class Timer<T> : TimerBase<T> where T : IComparable
	{
		protected abstract T now { get; }
		protected abstract T default_time { get; }

		protected Timer(double period) : base(period) { next_time = default_time; }
		public override void Reset() { next_time = default_time; }
		public abstract void Start();
		public abstract double Remaining { get; }

		public bool Check
		{
			get 
			{
				var time = now;
				if(next_time.Equals(default_time))
				{
					Start();
					return false;
				}
				return next_time.CompareTo(time) < 0;
			}
		}

		public bool RunIf(Action action, bool predicate)
		{
			if(predicate) { if(Check) { action(); Reset(); return true; } }
			else Reset(); 
			return false;
		}
		public bool RunIf(bool predicate) { return RunIf(action, predicate); }

		public override string ToString()
		{
			var time = now;
			return string.Format("time: {0} < next time {1}: {2}", 
			                     time, next_time, time.CompareTo(next_time) < 0);
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
		public override void Start() { next_time = now.AddSeconds(Period); }
		public override double Remaining { get { return next_time.Subtract(now).TotalSeconds; } }
	}

	public class Timer : Timer<double>
	{
		protected override double now { get { return Planetarium.GetUniversalTime(); } }
		protected override double default_time { get { return -1; } }
		public Timer(double period = 1) : base(period) { next_time = default_time; }
		public override void Start() { next_time = now+Period; }
		public override double Remaining { get { return next_time-now; } }
	}

	class MemoryTimer : IEnumerator<YieldInstruction>
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

