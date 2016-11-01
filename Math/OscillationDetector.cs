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
		protected readonly double low, high; //boundaries of frequency window of interest
		protected readonly int bins;  //number of bins in the frequency domain; the resolution
		protected readonly int window; //a sliding time-window that is scanned for the oscillations

		protected readonly LowPassFilterD detection_filter;
		protected readonly LinkedList<double> time;
		protected readonly LinkedList<T> samples;
		public readonly double[] freqs;
		public readonly T[] spectrum;

		public double Value { get { return detection_filter.Value; } }

		protected OscillationDetector(double low_freq, double high_freq, int freq_bins, int time_window, float smoothing)
		{
			//fill frequencies
			bins = freq_bins;
			low  = low_freq*Utils.TwoPI;
			high = high_freq*Utils.TwoPI;
			freqs = new double[bins];
			for(int k = 0; k < bins; k++)
				freqs[k] = low + (high-low)*k/bins;
			//prepare time windo
			window = time_window;
			time = new LinkedList<double>();
			samples = new LinkedList<T>();
			//output
			spectrum = new T[bins];
			detection_filter = new LowPassFilterD();
			detection_filter.Tau = smoothing;
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
		/// True if the Furier bin indicates the presence of oscillation.
		/// </summary>
		/// <param name="s">The value of a Furier bin.</param>
		protected abstract bool has_oscillation(T s);

		/// <summary>
		/// Update detector's state.
		/// </summary>
		/// <param name="input">Input: a new value of the signal.</param>
		/// <param name="dt">The time interval that has passed from the previous update.</param>
		/// <returns>A number in [0:1] interval that indicates the presence of oscillations in
		/// the signal in given time window within frequency limits.</returns>
		public double Update(T input, double dt)
		{
			if(time.Count == 0) time.AddFirst(0);
			else time.AddFirst(time.First.Value+dt);
			samples.AddFirst(norm(input));
			var s = samples.First.Value;
			var t = time.First.Value;
			var flag = false;
			if(samples.Count <= window)
			{
				for(int k = 0; k < bins; k++)
				{
					spectrum[k] = next(k, s, t);
					flag |= has_oscillation(spectrum[k]);
				}
			}
			else
			{
				var t0 = time.Last.Value; time.RemoveLast();
				var s0 = samples.Last.Value; samples.RemoveLast();
				for(int k = 0; k < bins; k++)
				{
					spectrum[k] = next(k, s, t, s0, t0);
					flag |= has_oscillation(spectrum[k]);
				}
			}
			return detection_filter.Update(flag? 1 : 0);
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
		public double Threshold;

		public OscillationDetectorD(double low_freq, double high_freq, int freq_bins, int time_window, float smoothing, double threshold)
			: base(low_freq, high_freq, freq_bins, time_window, smoothing) 
		{ Threshold = threshold; }

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

		protected override bool has_oscillation(double s)
		{
			return Math.Abs(s) > Threshold;
		}
	}

	public class OscillationDetectorV : OscillationDetector<Vector3>
	{
		public double Threshold;

		public OscillationDetectorV(double low_freq, double high_freq, int freq_bins, int time_window, float smoothing, double threshold)
			: base(low_freq, high_freq, freq_bins, time_window, smoothing) 
		{ Threshold = threshold; }

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

		protected override bool has_oscillation(Vector3 s)
		{
			return Math.Abs(s.x) > Threshold ||
				Math.Abs(s.y) > Threshold ||
				Math.Abs(s.z) > Threshold;
		}
	}

	public class OscillationDetector3D
	{
		double threshold;
		readonly OscillationDetectorD x_OD, y_OD, z_OD;

		public double Threshold
		{
			get { return threshold; }
			set 
			{
				threshold = value;
				x_OD.Threshold = threshold;
				y_OD.Threshold = threshold;
				z_OD.Threshold = threshold;
			}
		}

		public Vector3d Value { get; private set; }

		public OscillationDetector3D(double low_freq, double high_freq, int freq_bins, int time_window, float smoothing, double threshold)
		{ 
			this.threshold = threshold; 
			x_OD = new OscillationDetectorD(low_freq, high_freq, freq_bins, time_window, smoothing, threshold);
			y_OD = new OscillationDetectorD(low_freq, high_freq, freq_bins, time_window, smoothing, threshold);
			z_OD = new OscillationDetectorD(low_freq, high_freq, freq_bins, time_window, smoothing, threshold);
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

		public override string ToString()
		{
			return Utils.Format("OscillationDetector3D: Threshold={}, Value={}", Threshold, Value);
//			return Utils.Format("OscillationDetector3D: Threshold={}, Value={}\n" +
//			                    "\nx_OD:\n{}" +
//			                    "\ny_OD:\n{}" +
//			                    "\nz_OD:\n{}", 
//			                    Threshold, Value, x_OD, y_OD, z_OD);
		}
	}
}

