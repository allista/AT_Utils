//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri
//
// This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.

using System;
using UnityEngine;

namespace AT_Utils
{
	#region PI Controllers
	public class PI_Controller : ConfigNodeObject
	{
		new public const string NODE_NAME = "PICONTROLLER";

		//buggy: need to be public to be persistent
		[Persistent] public float p = 0.5f, i = 0.5f; //some default values
		protected PI_Controller master;

		public float P { get { return master == null? p : master.P; } set { p = value; } }
		public float I { get { return master == null? i : master.I; } set { i = value; } }

		public PI_Controller() {}
		public PI_Controller(float P, float I) { p = P; i = I; }

		public void setPI(PI_Controller other) { p = other.P; i = other.I; }
		public void setMaster(PI_Controller master) { this.master = master; }

		public virtual void DrawControls(string name, float maxP, float maxI)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(name, GUILayout.ExpandWidth(false));
			p = Utils.FloatSlider(" P", P, 0, maxP, "F2");
			i = Utils.FloatSlider(" I", I, 0, maxI, "F2");
			GUILayout.EndHorizontal();
		}

		public override string ToString() { return Utils.Format("[P={}, I={}]", P, I); }
	}

	public abstract class PI_Controller<T> : PI_Controller
	{
        public T Action = default(T);
        public T IntegralError = default(T);

		public abstract void Update(T error);

		public void Reset() 
		{ Action = default(T); IntegralError = default(T); }

		public static implicit operator T(PI_Controller<T> c) { return c.Action; }
	}

	public class PIf_Controller : PI_Controller<float>
	{
		public override void Update(float error)
		{
			IntegralError += error * TimeWarp.fixedDeltaTime;
			Action = error * P + IntegralError * I;
		}
	}
	#endregion

	#region PID Controllers
	public class PID_Controller<T> : ConfigNodeObject
	{
		new public const string NODE_NAME = "PIDCONTROLLER";

		[Persistent] public T Min, Max;
		[Persistent] public T P, I, D;

		public PID_Controller() {}
		public PID_Controller(T p, T i, T d, T min, T max)
		{ P = p; I = i; D = d; Min = min; Max = max; }

		public virtual void setPID(PID_Controller<T> c)
		{ P = c.P; I = c.I; D = c.D; Min = c.Min; Max = c.Max; }

        public void setClamp(T min, T max)
        {
            Min = min;
            Max = max;
        }

		public virtual void Reset() {}

		public override string ToString()
		{ 
            return Utils.Format("P={}\n" +
                                "I={}\n" +
                                "D={}\n" +
                                "Min={}\n" +
                                "Max={}", 
                                P, I, D, Min, Max); 
        }
	}

	public abstract class PID_Controller<T, C> : PID_Controller<C>
	{
        public T Action;
		public T LastError;
		public T IntegralError;

		public override void Reset() 
		{ Action = default(T); IntegralError = default(T); }

		public static implicit operator T(PID_Controller<T, C> c) { return c.Action; }

        public abstract void setClamp(C clamp);
        public abstract void Update(T error);
        public abstract void Update(T error, T speed);

		public override string ToString()
		{
			return base.ToString()+
				Utils.Format("\nLast Error: {}" +
				              "\nIntegral Error: {}" +
				              "\nAction: {}\n",
				              LastError, 
				              IntegralError, 
				              Action);
		}
	}

    public class PIDvf_Controller : PID_Controller<Vector3, float>
    {
        public PIDvf_Controller() {}
        public PIDvf_Controller(float p, float i, float d, float min, float max)
        { P = p; I = i; D = d; Min = min; Max = max; }

        public override void setClamp(float clamp) { setClamp(-clamp, clamp); }

        public override void Update(Vector3 error)
        {
            if(error.IsNaN()) return;
            if(LastError.IsZero()) LastError = error;
            Update(error, (error-LastError)/TimeWarp.fixedDeltaTime);
        }

        public override void Update(Vector3 error, Vector3 speed)
        {
            if(error.IsNaN()) return;
            if(IntegralError.x*error.x < 0) IntegralError.x = 0;
            if(IntegralError.y*error.y < 0) IntegralError.y = 0;
            if(IntegralError.z*error.z < 0) IntegralError.z = 0;
            var old_ierror = IntegralError;
            IntegralError += error*TimeWarp.fixedDeltaTime;
            var act = P*error + I*IntegralError + D*speed;
            if(act.IsZero()) Action = act;
            else
            {
                Action = new Vector3
                    (
                        float.IsNaN(act.x)? 0f : Utils.Clamp(act.x, Min, Max),
                        float.IsNaN(act.y)? 0f : Utils.Clamp(act.y, Min, Max),
                        float.IsNaN(act.z)? 0f : Utils.Clamp(act.z, Min, Max)
                    );
                if(act != Action) IntegralError = old_ierror;
            }
            LastError = error;
        }
    }

	public class PIDvf_Controller2 : PID_Controller<Vector3, float>
	{
		public PIDvf_Controller2() {}
		public PIDvf_Controller2(float p, float i, float d, float min, float max)
		{ P = p; I = i; D = d; Min = min; Max = max; }

        public override void setClamp(float clamp) { setClamp(-clamp, clamp); }

        public override void Update(Vector3 error)
        {
            if(error.IsNaN()) return;
            if(LastError.IsZero()) LastError = error;
            Update(error, (error-LastError)/TimeWarp.fixedDeltaTime);
        }

        public override void Update(Vector3 error, Vector3 speed)
		{
            if(error.IsNaN()) return;
            var derivative = D * speed;
            if(IntegralError.x*error.x < 0) IntegralError.x = 0;
            if(IntegralError.y*error.y < 0) IntegralError.y = 0;
            if(IntegralError.z*error.z < 0) IntegralError.z = 0;
			IntegralError.x = (Mathf.Abs(derivative.x) < 0.6f * Max) ? IntegralError.x + (error.x * I * TimeWarp.fixedDeltaTime) : 0.9f * IntegralError.x;
			IntegralError.y = (Mathf.Abs(derivative.y) < 0.6f * Max) ? IntegralError.y + (error.y * I * TimeWarp.fixedDeltaTime) : 0.9f * IntegralError.y;
			IntegralError.z = (Mathf.Abs(derivative.z) < 0.6f * Max) ? IntegralError.z + (error.z * I * TimeWarp.fixedDeltaTime) : 0.9f * IntegralError.z;
			Vector3.ClampMagnitude(IntegralError, Max);
			var act = error * P + IntegralError + derivative;
			Action = new Vector3
				(
					float.IsNaN(act.x)? 0f : Mathf.Clamp(act.x, Min, Max),
					float.IsNaN(act.y)? 0f : Mathf.Clamp(act.y, Min, Max),
					float.IsNaN(act.z)? 0f : Mathf.Clamp(act.z, Min, Max)
				);
            LastError = error;
		}
	}

	public class PIDvd_Controller : PID_Controller<Vector3d, double>
	{
		public PIDvd_Controller() {}
		public PIDvd_Controller(double p, double i, double d, double min, double max)
		{ P = p; I = i; D = d; Min = min; Max = max; }

        public override void setClamp(double clamp) { setClamp(-clamp, clamp); }

        public override void Update(Vector3d error)
        {
            if(error.IsNaN()) return;
            if(LastError.IsZero()) LastError = error;
            Update(error, (error-LastError)/TimeWarp.fixedDeltaTime);
        }

        public override void Update(Vector3d error, Vector3d speed)
        {
			if(error.IsNaN()) return;
            if(IntegralError.x*error.x < 0) IntegralError.x = 0;
            if(IntegralError.y*error.y < 0) IntegralError.y = 0;
            if(IntegralError.z*error.z < 0) IntegralError.z = 0;
			var old_ierror = IntegralError;
			IntegralError += error*TimeWarp.fixedDeltaTime;
            var act = P*error + I*IntegralError + D*speed;
			if(act.IsZero()) Action = act;
			else
			{
				Action = new Vector3d
					(
						double.IsNaN(act.x)? 0f : Utils.Clamp(act.x, Min, Max),
						double.IsNaN(act.y)? 0f : Utils.Clamp(act.y, Min, Max),
						double.IsNaN(act.z)? 0f : Utils.Clamp(act.z, Min, Max)
					);
				if(act != Action) IntegralError = old_ierror;
			}
			LastError = error;
		}
	}

    public class PIDv_Controller : PID_Controller<Vector3, Vector3>
    {
        public PIDv_Controller() {}
        public PIDv_Controller(Vector3 p, Vector3 i, Vector3 d, Vector3 min, Vector3 max)
        { P = p; I = i; D = d; Min = min; Max = max; }

        public override void setClamp(Vector3 clamp) { setClamp(-clamp, clamp); }

        public override void Update(Vector3 error)
        {
            if(error.IsNaN()) return;
            if(LastError.IsZero()) LastError = error;
            Update(error, (error-LastError)/TimeWarp.fixedDeltaTime);
        }

        public override void Update(Vector3 error, Vector3 speed)
        {
            if(error.IsNaN()) return;
            if(IntegralError.x*error.x < 0) IntegralError.x = 0;
            if(IntegralError.y*error.y < 0) IntegralError.y = 0;
            if(IntegralError.z*error.z < 0) IntegralError.z = 0;
            var old_ierror = IntegralError;
            IntegralError += error*TimeWarp.fixedDeltaTime;
            var act = Vector3.Scale(P, error) + Vector3.Scale(I, IntegralError) + Vector3.Scale(D, speed);
            if(act.IsZero()) Action = act;
            else
            {
                Action = new Vector3
                    (
                        float.IsNaN(act.x)? 0f : Utils.Clamp(act.x, Min.x, Max.x),
                        float.IsNaN(act.y)? 0f : Utils.Clamp(act.y, Min.y, Max.y),
                        float.IsNaN(act.z)? 0f : Utils.Clamp(act.z, Min.z, Max.z)
                    );
                if(act != Action) IntegralError = old_ierror;
            }
            LastError = error;
        }
    }

	public class PIDf_Controller : PID_Controller<float, float>
	{
		public PIDf_Controller() {}
		public PIDf_Controller(float p, float i, float d, float min, float max)
		{ P = p; I = i; D = d; Min = min; Max = max; }

        public override void setClamp(float clamp) { setClamp(-clamp, clamp); }

        public override void Update(float error)
        {
            if(float.IsNaN(error)) return;
            if(LastError.Equals(0)) LastError = error;
            Update(error, (error-LastError)/TimeWarp.fixedDeltaTime);
        }

        public override void Update(float error, float speed)
        {
			if(float.IsNaN(error)) return;
			if(IntegralError*error < 0) IntegralError = 0;
			var old_ierror = IntegralError;
			IntegralError += error*TimeWarp.fixedDeltaTime;
			var act = P*error + I*IntegralError + D*speed;
			Action = Mathf.Clamp(act, Min, Max);
			if(Mathf.Abs(act-Action) > 1e-5) IntegralError = old_ierror;
			LastError = error;
		}
	}

	public class PIDf_Controller2 : PID_Controller<float, float>
	{
		public PIDf_Controller2() {}
		public PIDf_Controller2(float p, float i, float d, float min, float max)
		{ P = p; I = i; D = d; Min = min; Max = max; }

        public override void setClamp(float clamp) { setClamp(-clamp, clamp); }

        public override void Update(float error)
        {
            if(float.IsNaN(error)) return;
            if(LastError.Equals(0)) LastError = error;
            Update(error, (error-LastError)/TimeWarp.fixedDeltaTime);
        }

        public override void Update(float error, float speed)
        {
			if(float.IsNaN(error)) return;
            if(IntegralError*error < 0) IntegralError = 0;
			var derivative = D * speed;
			IntegralError = Mathf.Clamp((Math.Abs(derivative) < 0.6f * Max) ? IntegralError + (error * I * TimeWarp.fixedDeltaTime) : 0.9f * IntegralError, Min, Max);
			var act = error * P + IntegralError + derivative;
			if(!float.IsNaN(act)) Action = Mathf.Clamp(act, Min, Max);
			LastError = error;
		}
	}

	public class PIDv_Controller2 : PID_Controller<Vector3, Vector3>
	{
		public PIDv_Controller2() {}
		public PIDv_Controller2(Vector3 p, Vector3 i, Vector3 d, Vector3 min, Vector3 max)
		{ P = p; I = i; D = d; Min = min; Max = max; }

        public override void setClamp(Vector3 clamp) { setClamp(-clamp, clamp); }

        public override void Update(Vector3 error)
        {
            if(error.IsNaN()) return;
            if(LastError.IsZero()) LastError = error;
            Update(error, (error-LastError)/TimeWarp.fixedDeltaTime);
        }

        public override void Update(Vector3 error, Vector3 speed)
        {
            if(error.IsNaN()) return;
			var derivative = Vector3.Scale(speed, D);
            if(IntegralError.x*error.x < 0) IntegralError.x = 0;
            if(IntegralError.y*error.y < 0) IntegralError.y = 0;
            if(IntegralError.z*error.z < 0) IntegralError.z = 0;
			IntegralError.x = (Mathf.Abs(derivative.x) < 0.6f * Max.x) ? IntegralError.x + (error.x * I.x * TimeWarp.fixedDeltaTime) : 0.9f * IntegralError.x;
			IntegralError.y = (Mathf.Abs(derivative.y) < 0.6f * Max.y) ? IntegralError.y + (error.y * I.y * TimeWarp.fixedDeltaTime) : 0.9f * IntegralError.y;
			IntegralError.z = (Mathf.Abs(derivative.z) < 0.6f * Max.z) ? IntegralError.z + (error.z * I.z * TimeWarp.fixedDeltaTime) : 0.9f * IntegralError.z;
			var act = Vector3.Scale(error, P) + IntegralError.ClampComponents(Min, Max) + derivative;
			Action = new Vector3
				(
					float.IsNaN(act.x)? 0f : Mathf.Clamp(act.x, Min.x, Max.x),
					float.IsNaN(act.y)? 0f : Mathf.Clamp(act.y, Min.y, Max.y),
					float.IsNaN(act.z)? 0f : Mathf.Clamp(act.z, Min.z, Max.z)
				);
            LastError = error;
		}
	}

    public class PIDf_Controller3 : PID_Controller<float, float>
    {
        [Persistent] public float Tau = 0;
        readonly LowPassFilterF dFilter = new LowPassFilterF();

        public PIDf_Controller3() {}
        public PIDf_Controller3(float p, float i, float d, float min, float max, float filter)
        { P = p; I = i; D = d; Min = min; Max = max; setTau(filter); }

        public override void Load(ConfigNode node)
        {
            base.Load(node);
            dFilter.Tau = Tau;
        }

        public override void Reset()
        {
            base.Reset();
            dFilter.Reset();
        }

        public void setPID(PIDf_Controller3 c)
        {
            base.setPID(c);
            setTau(c.Tau);
        }

        public void setTau(float tau) { Tau = tau; dFilter.Tau = tau; }

        public override void setClamp(float clamp) { setClamp(-clamp, clamp); }

        public override void Update(float error)
        {
            if(float.IsNaN(error)) return;
            if(LastError.Equals(0)) LastError = error;
            Update(error, (error-LastError)/TimeWarp.fixedDeltaTime);
        }

        public override void Update(float error, float speed)
        {
            if(float.IsNaN(error)) return;
            if(IntegralError*error < 0) IntegralError = 0;
            var old_ierror = IntegralError;
            IntegralError += error*TimeWarp.fixedDeltaTime;
            //compute filterred d component of the output
            var d = dFilter.Update(D*speed);
            //compute new Action
            var act = P*error + I*IntegralError + d;
            Action = Mathf.Clamp(act, Min, Max);
            //if the Action was clamped
            //do not save IntegralError
            //and clamp dFilter value
            if(Mathf.Abs(act-Action) > 1e-5) 
            {
                IntegralError = old_ierror;
                dFilter.Set(Utils.Clamp(d, Min, Max));
            }
            LastError = error;
        }

        public override string ToString()
        {
            return base.ToString()+"D.filter: "+dFilter+"\n";
        }
    }
	#endregion
}

