//   StallDetector.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri

using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    public interface IStallDetector<T>
    {
        bool Stalled { get; }
        void Reset();
    }

    public abstract class StallDetector<T> : IStallDetector<T>
    {
        public T Threshold;
        protected T old_value;
        protected LowPassFilter<T> filter;
        protected Timer timer = new Timer();

        public bool Stalled => timer.TimePassed;
        public static implicit operator bool(StallDetector<T> d) => d.Stalled;

        protected StallDetector(T threshold, double stall_time) 
        {
            Threshold = threshold;
            timer.Period = stall_time;
        }

        protected void init_filter()
        {
            filter.Tau = (float)timer.Period/10;
        }

        protected abstract bool same_value(T a, T b);

        public void Update(T value)
        {
            filter.Update(value);
            if(!same_value(filter, old_value))
            {
                old_value = filter;
                timer.Restart();
            }
        }

        public void Reset()
        {
            filter.Reset();
            timer.Reset();
            old_value = default(T);
        }

		public override string ToString()
		{
            return Utils.Format("threshold: {}, old value {}, value {}, timer {}",
                                Threshold, old_value, filter, timer.Remaining);
		}
	}

    public class StallDetectorF : StallDetector<float>
    {
        public StallDetectorF(float threshold, double stall_time) 
            : base(threshold, stall_time)
        {
            filter = new LowPassFilterF();
            init_filter();
        }

		protected override bool same_value(float a, float b) =>
        Mathf.Abs(b-a) < Threshold;
	}

    public class StallDetectorD : StallDetector<double>
    {
        public StallDetectorD(double threshold, double stall_time) 
            : base(threshold, stall_time)
        {
            filter = new LowPassFilterD();
            init_filter();
        }

        protected override bool same_value(double a, double b) =>
        System.Math.Abs(b-a) < Threshold;
    }

    public abstract class StallDetectorMulti<T> : IStallDetector<T>
    {
        protected readonly List<StallDetector<T>> detectors = new List<StallDetector<T>>();

        public void Update(params T[] values)
        {
            for(int i = 0, detectorsCount = detectors.Count; i < detectorsCount; i++)
                detectors[i].Update(values[i]);
        }

        public bool Stalled => detectors.TrueForAll(d => d.Stalled);
        public static implicit operator bool(StallDetectorMulti<T> d) => d.Stalled;

        public void Reset() => detectors.ForEach(d => d.Reset());
    }

    public class StallDetectorMultiF : StallDetectorMulti<float>
    {
        public StallDetectorMultiF(double stall_time, params float[] thresholds)
        {
            detectors.Capacity = thresholds.Length;
            for(int i = 0, thresholdsLength = thresholds.Length; i < thresholdsLength; i++)
                detectors.Add(new StallDetectorF(thresholds[i], stall_time));
        }
    }

    public class StallDetectorMultiD : StallDetectorMulti<double>
    {
        public StallDetectorMultiD(double stall_time, params double[] thresholds)
        {
            detectors.Capacity = thresholds.Length;
            for(int i = 0, thresholdsLength = thresholds.Length; i < thresholdsLength; i++)
                detectors.Add(new StallDetectorD(thresholds[i], stall_time));
        }
    }
}
