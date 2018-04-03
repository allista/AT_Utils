//   OscillationDetector.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    /// <summary>
    /// Oscillation detector based on the article:
    /// "A Frequency Domain Method for Real-Time Detection of Oscillations"
    /// by Girish Chowdhary, Sriram Srinivasan, and Eric N. Johnson 
    /// Georgia Institute of Technology, Atlanta, GA 30332
    /// http://web.mit.edu/girishc/www/publications/files/Chowdhary_Srinivasan_Johnson_JACIC_2010.pdf
    /// 
    /// It uses DCT instead of DFT to simplify calculations.
    /// 
    /// This is a generic base class.
    /// </summary>
    public abstract class OscillationDetector<T> where T: new()
    {
        /// <summary>
        /// The bounds of frequency window of interest.
        /// </summary>
        protected readonly double low, high;

        /// <summary>
        /// Number of bins in the frequency domain; the resolution.
        /// </summary>
        protected readonly int bins;

        /// <summary>
        /// Sliding time-window that is scanned for the oscillations.
        /// </summary>
        protected readonly int window;

        protected readonly LinkedList<double> time;
        protected readonly LinkedList<T> samples;
        public readonly double[] freqs;
        public readonly T[] spectrum;

        public abstract T Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AT_Utils.OscillationDetector"/> class.
        /// </summary>
        /// <param name="low_freq">Low bound of frequency window in Hz.</param>
        /// <param name="high_freq">High bound of frequency window in Hz.</param>
        /// <param name="freq_bins">Number of frequency bins.</param>
        /// <param name="time_window">Number of time frames.</param>
        protected OscillationDetector(double low_freq, double high_freq, int freq_bins, int time_window)
        {
            //fill frequencies
            bins = freq_bins;
            low  = low_freq*Utils.TwoPI;
            high = high_freq*Utils.TwoPI;
            freqs = new double[bins];
            var df = (high-low)/bins;
            for(int k = 0; k < bins; k++)
                freqs[k] = low + df*k;
            //prepare time windo
            window = time_window;
            time = new LinkedList<double>();
            samples = new LinkedList<T>();
            //output
            spectrum = new T[bins];
        }

        /// <summary>
        /// Normalized input value.
        /// </summary>
        protected abstract T norm(T val);

        /// <summary>
        /// Next value of a Furier k-th bin.
        /// </summary>
        /// <param name="k">Furier bin index.</param>
        /// <param name="s">Last sample (normalized).</param>
        /// <param name="t">Current time.</param>
        protected abstract T next(int k, T s, double t);

        /// <summary>
        /// Next value of a Furier k-th bin.
        /// </summary>
        /// <param name="k">Furier bin index.</param>
        /// <param name="s">Last sample (normalized).</param>
        /// <param name="t">Current time.</param>
        /// <param name="s0">First sample in the time window (normalized).</param>
        /// <param name="t0">Time of the first sample in the time window.</param>
        protected abstract T next(int k, T s, double t, T s0, double t0);

        /// <summary>
        /// Compares absolute value of a Furier bin with the max value and sets the latter.
        /// </summary>
        /// <param name="s">The value of a Furier bin.</param>
        /// <param name="max">Current maximum value.</param>
        protected abstract void max_magnitude(T s, ref T max);

        /// <summary>
        /// Filters the maximum value of the specter.
        /// </summary>
        /// <returns>The filtered value.</returns>
        /// <param name="max">The value to filter.</param>
        protected abstract T filter_max_value(T max);

        /// <summary>
        /// Update detector's state.
        /// </summary>
        /// <param name="input">Input: a new value of the signal.</param>
        /// <param name="dt">The time interval that has passed from the previous update.</param>
        /// <returns>A number in [0:1] interval that indicates the presence of oscillations in
        /// the signal in given time window within frequency limits.</returns>
        public T Update(T input, double dt)
        {
            if(time.Count == 0) time.AddFirst(0);
            else time.AddFirst(time.First.Value+dt);
            samples.AddFirst(norm(input));
            var s = samples.First.Value;
            var t = time.First.Value;
            var max = default(T);
            if(samples.Count <= window)
            {
                for(int k = 0; k < bins; k++)
                {
                    spectrum[k] = next(k, s, t);
                    max_magnitude(spectrum[k], ref max);
                }
            }
            else
            {
                var t0 = time.Last.Value; time.RemoveLast();
                var s0 = samples.Last.Value; samples.RemoveLast();
                for(int k = 0; k < bins; k++)
                {
                    spectrum[k] = next(k, s, t, s0, t0);
                    max_magnitude(spectrum[k], ref max);
                }
            }
            return filter_max_value(max);
        }

        public virtual void Reset()
        {
            for(int k = 0; k < bins; k++)
                spectrum[k] = default(T);
            samples.Clear();
            time.Clear();
        }

        public override string ToString()
        {
            var spec = new string[bins];
            for(int k = 0; k < bins; k++)
                spec[k] = string.Format("{0:F}Hz\t{1}\n", freqs[k]/Utils.TwoPI, spectrum[k]);
            return Utils.Format("OscillationDetector: Value = {}\nSpectrum:\n{}", 
                                Value, string.Concat(spec));
        }
    }

    public class OscillationDetectorD : OscillationDetector<double>
    {
        protected LowPassFilterD max_filter = new LowPassFilterD();

        public override double Value { get { return max_filter.Value; } }

        public OscillationDetectorD(double low_freq, double high_freq, int freq_bins, int time_window, float smoothing)
            : base(low_freq, high_freq, freq_bins, time_window) 
        { max_filter.Tau = smoothing; }

        protected override double norm(double val) { return val/bins; }

        protected override double next(int k, double s, double t)
        { 
            return spectrum[k] + s * Math.Cos(freqs[k] * t); 
        }

        protected override double next(int k, double s, double t, double s0, double t0)
        {
            var f = freqs[k];
            return spectrum[k] + s * Math.Cos(f * t) - s0 * Math.Cos(f * t0);
        }

        protected override void max_magnitude(double s, ref double max)
        {
            var abs = Math.Abs(s);
            if(abs > max) max = abs;
        }

        protected override double filter_max_value(double max)
        {
            return max_filter.Update(max);
        }

        public override void Reset()
        {
            base.Reset();
            max_filter.Reset();
        }
    }

    public class OscillationDetectorF : OscillationDetector<float>
    {
        protected LowPassFilterF max_filter = new LowPassFilterF();

        public override float Value { get { return max_filter.Value; } }

        public OscillationDetectorF(double low_freq, double high_freq, int freq_bins, int time_window, float smoothing)
            : base(low_freq, high_freq, freq_bins, time_window) 
        { max_filter.Tau = smoothing; }

        protected override float norm(float val) { return val/bins; }

        protected override float next(int k, float s, double t)
        { 
            return spectrum[k] + s * (float)Math.Cos(freqs[k] * t); 
        }

        protected override float next(int k, float s, double t, float s0, double t0)
        {
            var f = freqs[k];
            return spectrum[k] + s * (float)Math.Cos(f * t) - s0 * (float)Math.Cos(f * t0);
        }

        protected override void max_magnitude(float s, ref float max)
        {
            var abs = Math.Abs(s);
            if(abs > max) max = abs;
        }

        protected override float filter_max_value(float max)
        {
            return max_filter.Update(max);
        }

        public override void Reset()
        {
            base.Reset();
            max_filter.Reset();
        }
    }

    public class OscillationDetectorV : OscillationDetector<Vector3>
    {
        protected LowPassFilterV max_filter = new LowPassFilterV();

        public override Vector3 Value { get { return max_filter.Value; } }

        public OscillationDetectorV(double low_freq, double high_freq, int freq_bins, int time_window, float smoothing)
            : base(low_freq, high_freq, freq_bins, time_window) 
        { max_filter.Tau = smoothing; }

        protected override Vector3 norm(Vector3 val) { return val/bins; }

        protected override Vector3 next(int k, Vector3 s, double t)
        { 
            return spectrum[k] + s * (float)Math.Cos(freqs[k] * t); 
        }

        protected override Vector3 next(int k, Vector3 s, double t, Vector3 s0, double t0)
        {
            var f = freqs[k];
            return spectrum[k] + s * (float)Math.Cos(f * t) - s0 * (float)Math.Cos(f * t0);
        }

        protected override void max_magnitude(Vector3 s, ref Vector3 max)
        {
            if(s.sqrMagnitude > max.sqrMagnitude) max = s;
        }

        protected override Vector3 filter_max_value(Vector3 max)
        {
            return max_filter.Update(max);
        }

        public override void Reset()
        {
            base.Reset();
            max_filter.Reset();
        }
    }

    public class OscillationDetector3D
    {
        readonly OscillationDetectorD x_OD, y_OD, z_OD;

        public Vector3d Value { get; private set; }

        public OscillationDetector3D(double low_freq, double high_freq, int freq_bins, int time_window, float smoothing)
        { 
            x_OD = new OscillationDetectorD(low_freq, high_freq, freq_bins, time_window, smoothing);
            y_OD = new OscillationDetectorD(low_freq, high_freq, freq_bins, time_window, smoothing);
            z_OD = new OscillationDetectorD(low_freq, high_freq, freq_bins, time_window, smoothing);
        }

        public Vector3d Update(Vector3d input, double dt)
        {
            Value = new Vector3d(x_OD.Update(input.x, dt),
                                  y_OD.Update(input.y, dt),
                                 z_OD.Update(input.z, dt));
            return Value;
        }

        public Vector3 Update(Vector3 input, double dt)
        { return Update((Vector3d)input, dt); }

        public void Reset()
        {
            x_OD.Reset();
            y_OD.Reset();
            z_OD.Reset();
        }

        public override string ToString()
        {
            return Utils.Format("OscillationDetector3D: Value={}\n" +
                                "\nx_OD:\n{}" +
                                "\ny_OD:\n{}" +
                                "\nz_OD:\n{}", 
                                Value, x_OD, y_OD, z_OD);
        }
    }
}

