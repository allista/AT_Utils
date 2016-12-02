//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri
//
// This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.

﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
	public static class MiscExtensions
	{
		public static Color Normalized(this Color c)
		{
			var max = c.r > c.g? (c.r > c.b? c.r : c.b) : (c.g > c.b? c.g : c.b);
			return max.Equals(0)? c : new Color(c.r / max, c.g / max, c.b / max);
		}

		#region From blizzy's Toolbar
		public static Vector2 clampToScreen(this Vector2 pos) 
		{
			pos.x = Mathf.Clamp(pos.x, 0, Screen.width - 1);
			pos.y = Mathf.Clamp(pos.y, 0, Screen.height - 1);
			return pos;
		}

		public static Rect clampToScreen(this Rect rect) 
		{
			rect.width = Mathf.Clamp(rect.width, 0, Screen.width);
			rect.height = Mathf.Clamp(rect.height, 0, Screen.height);
			rect.x = Mathf.Clamp(rect.x, 0, Screen.width - rect.width);
			rect.y = Mathf.Clamp(rect.y, 0, Screen.height - rect.height);
			return rect;
		}

		public static Rect clampToWindow(this Rect rect, Rect window) 
		{
			rect.width = Mathf.Clamp(rect.width, 0, window.width);
			rect.height = Mathf.Clamp(rect.height, 0, window.height);
			rect.x = Mathf.Clamp(rect.x, 0, window.width - rect.width);
			rect.y = Mathf.Clamp(rect.y, 0, window.height - rect.height);
			return rect;
		}
		#endregion

		#region ConfigNode
		public static void AddRect(this ConfigNode n, string name, Rect r)
		{ n.AddValue(name, ConfigNode.WriteQuaternion(new Quaternion(r.x, r.y, r.width, r.height))); }

		public static Rect GetRect(this ConfigNode n, string name)
		{ 
			try 
			{ 
				var q = ConfigNode.ParseQuaternion(n.GetValue(name)); 
				return new Rect(q.x, q.y, q.z, q.w);
			}
			catch { return default(Rect); }
		}
		#endregion
	}


	public static class VectorExtensions
	{
		public static bool IsNaN(this Vector3d v)
		{ return double.IsNaN(v.x) || double.IsNaN(v.y) || double.IsNaN(v.z); }

		public static bool IsNaN(this Vector3 v)
		{ return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z); }

		public static bool IsInf(this Vector3d v)
		{ return double.IsInfinity(v.x) || double.IsInfinity(v.y) || double.IsInfinity(v.z); }

		public static bool IsInf(this Vector3 v)
		{ return float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z); }

		public static bool Invalid(this Vector3d v) { return v.IsNaN() || v.IsInf(); }

		public static bool Invalid(this Vector3 v) { return v.IsNaN() || v.IsInf(); }

		public static Vector3 CubeNorm(this Vector3 v)
		{
			if(v.IsZero()) return v;
			var max = -1f;
			for(int i = 0; i < 3; i++)
			{
				var ai = Mathf.Abs(v[i]);
				if(max < ai) max = ai;
			}
			return v/max;
		}

		public static Vector3d CubeNorm(this Vector3d v)
		{
			if(v.IsZero()) return v;
			var max = -1.0;
			for(int i = 0; i < 3; i++)
			{
				var ai = Math.Abs(v[i]);
				if(max < ai) max = ai;
			}
			return v/max;
		}

		public static Vector3 Inverse(this Vector3 v, float nan=float.MaxValue) 
		{ 
			return new Vector3(
				v.x.Equals(0)? nan : 1/v.x, 
				v.y.Equals(0)? nan : 1/v.y, 
				v.z.Equals(0)? nan : 1/v.z); 
		}

		public static Vector3d Inverse(this Vector3d v, double nan=double.MaxValue) 
		{ 
			return new Vector3d(
				v.x.Equals(0)? nan : 1/v.x, 
				v.y.Equals(0)? nan : 1/v.y, 
				v.z.Equals(0)? nan : 1/v.z); 
		}

		public static Vector3 ScaleChain(this Vector3 vec, params Vector3[] vectors)
		{
			var result = vec;
			for(int i = 0, vectorsLength = vectors.Length; i < vectorsLength; i++)
			{
				var v = vectors[i];
				result.x *= v.x;
				result.y *= v.y;
				result.z *= v.z;
			}
			return result;
		}

		public static Vector3d ScaleChain(this Vector3d vec, params Vector3d[] vectors)
		{
			var result = vec;
			for(int i = 0, vectorsLength = vectors.Length; i < vectorsLength; i++)
			{
				var v = vectors[i];
				result.x *= v.x;
				result.y *= v.y;
				result.z *= v.z;
			}
			return result;
		}

		public static Vector3 SquaredComponents(this Vector3 v) 
		{ return new Vector3(v.x * v.x, v.y * v.y, v.z * v.z); }

		public static Vector3d SquaredComponents(this Vector3d v) 
		{ return new Vector3d(v.x * v.x, v.y * v.y, v.z * v.z); }

		public static Vector3 SqrtComponents(this Vector3 v) 
		{ return new Vector3(Mathf.Sqrt(v.x), Mathf.Sqrt(v.y), Mathf.Sqrt(v.z)); }

		public static Vector3d SqrtComponents(this Vector3d v) 
		{ return new Vector3d(Math.Sqrt(v.x), Math.Sqrt(v.y), Math.Sqrt(v.z)); }

		public static Vector3 PowComponents(this Vector3 v, float pow) 
		{ return new Vector3(Mathf.Pow(v.x, pow), Mathf.Pow(v.y, pow), Mathf.Pow(v.z, pow)); }

		public static Vector3d PowComponents(this Vector3d v, double pow) 
		{ return new Vector3d(Math.Pow(v.x, pow), Math.Pow(v.y, pow), Math.Pow(v.z, pow)); }

		public static Vector3 ClampComponents(this Vector3 v, float min, float max) 
		{ 
			return new Vector3(Mathf.Clamp(v.x, min, max), 
			                   Mathf.Clamp(v.y, min, max), 
			                   Mathf.Clamp(v.z, min, max)); 
		}

		public static Vector3 ClampComponents(this Vector3 v, Vector3 min, Vector3 max) 
		{ 
			return new Vector3(Mathf.Clamp(v.x, min.x, max.x), 
			                   Mathf.Clamp(v.y, min.y, max.y), 
			                   Mathf.Clamp(v.z, min.z, max.z)); 
		}

		public static Vector3 ClampComponentsL(this Vector3 v, Vector3 min) 
		{ 
			return new Vector3(Utils.ClampL(v.x, min.x), 
			                   Utils.ClampL(v.y, min.y), 
			                   Utils.ClampL(v.z, min.z)); 
		}

		public static Vector3 ClampComponentsH(this Vector3 v, Vector3 max) 
		{ 
			return new Vector3(Utils.ClampH(v.x, max.x), 
			                   Utils.ClampH(v.y, max.y), 
			                   Utils.ClampH(v.z, max.z));
		}

		public static Vector3d ClampComponents(this Vector3d v, double min, double max) 
		{ 
			return new Vector3d(Utils.Clamp(v.x, min, max), 
			                    Utils.Clamp(v.y, min, max), 
			                    Utils.Clamp(v.z, min, max)); 
		}

		public static Vector3d ClampComponents(this Vector3d v, Vector3d min, Vector3d max) 
		{ 
			return new Vector3d(Utils.Clamp(v.x, min.x, max.x), 
			                    Utils.Clamp(v.y, min.y, max.y), 
			                    Utils.Clamp(v.z, min.z, max.z)); 
		}

		public static Vector3d ClampComponentsH(this Vector3d v, Vector3d max) 
		{ 
			return new Vector3d(Utils.ClampH(v.x, max.x), 
			                    Utils.ClampH(v.y, max.y), 
			                    Utils.ClampH(v.z, max.z)); 
		}

		public static Vector3d ClampComponentsL(this Vector3d v, Vector3d min) 
		{ 
			return new Vector3d(Utils.ClampL(v.x, min.x), 
			                    Utils.ClampL(v.y, min.y), 
			                    Utils.ClampL(v.z, min.z)); 
		}

		public static Vector3 ClampComponentsH(this Vector3 v, float max) 
		{ 
			return new Vector3(Utils.ClampH(v.x, max), 
			                   Utils.ClampH(v.y, max), 
			                   Utils.ClampH(v.z, max)); 
		}

		public static Vector3 ClampComponentsL(this Vector3 v, float min) 
		{ 
			return new Vector3(Utils.ClampL(v.x, min), 
			                   Utils.ClampL(v.y, min), 
			                   Utils.ClampL(v.z, min)); 
		}

		public static Vector3d ClampComponentsH(this Vector3d v, double max) 
		{ 
			return new Vector3d(Utils.ClampH(v.x, max), 
			                    Utils.ClampH(v.y, max), 
			                    Utils.ClampH(v.z, max)); 
		}

		public static Vector3d ClampComponentsL(this Vector3d v, double min) 
		{ 
			return new Vector3d(Utils.ClampL(v.x, min), 
			                    Utils.ClampL(v.y, min), 
			                    Utils.ClampL(v.z, min)); 
		}


		public static Vector3 ClampMagnitudeH(this Vector3 v, float max)
		{ 
			var vm = v.magnitude;
			return vm > max? v/vm*max : v;

		}

		public static Vector3d ClampMagnitudeH(this Vector3d v, float max)
		{ 
			var vm = v.magnitude;
			return vm > max? v/vm*max : v;
		}

		public static Vector3d ClampMagnitudeL(this Vector3d v, float min)
		{ 
			var vm = v.magnitude;
			return vm < min? v/vm*min : v;
		}

		public static Vector3 Sign(this Vector3 v)
		{ return new Vector3(Mathf.Sign(v.x), Mathf.Sign(v.y), Mathf.Sign(v.z)); }

		public static Vector3 AbsComponents(this Vector3 v)
		{ return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z)); }

		public static int MaxI(this Vector3 v)
		{
			var maxi = 0;
			var max  = 0f;
			for(int i = 0; i < 3; i++)
			{
				if(Mathf.Abs(v[i]) > Mathf.Abs(max))
				{ max = v[i]; maxi = i; }
			}
			return maxi;
		}

		public static int MinI(this Vector3 v)
		{
			var mini = 0;
			var min   = float.MaxValue;
			for(int i = 0; i < 3; i++)
			{
				if(Mathf.Abs(v[i]) < Mathf.Abs(min))
				{ min = v[i]; mini = i; }
			}
			return mini;
		}

		public static int MedI(this Vector3 v)
		{
			return v.x < v.y? 
				(v.x > v.z? 0 : (v.z < v.y? 2 : 1)) : 
				(v.y > v.z? 1 : (v.z < v.x? 2 : 0));
		}

		public static int MaxI(this Vector3d v)
		{
			var maxi = 0;
			var max  = 0.0;
			for(int i = 0; i < 3; i++)
			{
				if(Math.Abs(v[i]) > Math.Abs(max))
				{ max = v[i]; maxi = i; }
			}
			return maxi;
		}

		public static int MinI(this Vector3d v)
		{
			var mini = 0;
			var min   = double.MaxValue;
			for(int i = 0; i < 3; i++)
			{
				if(Math.Abs(v[i]) < Math.Abs(min))
				{ min = v[i]; mini = i; }
			}
			return mini;
		}

		public static int MedI(this Vector3d v)
		{
			return v.x < v.y? 
				(v.x > v.z? 0 : (v.z < v.y? 2 : 1)) : 
				(v.y > v.z? 1 : (v.z < v.x? 2 : 0));
		}

		public static Vector3 Component(this Vector3 v, int i)
		{
			var ret = Vector3.zero;
			ret[i] = v[i];
			return ret;
		}

		public static Vector3 Exclude(this Vector3 v, int i)
		{
			var ret = v;
			ret[i] = 0;
			return ret;
		}

		public static Vector3d Component(this Vector3d v, int i)
		{
			var ret = Vector3d.zero;
			ret[i] = v[i];
			return ret;
		}

		public static Vector3d Exclude(this Vector3d v, int i)
		{
			var ret = v;
			ret[i] = 0;
			return ret;
		}

		public static Vector3 MaxComponentV(this Vector3 v)
		{ return v.Component(v.MaxI()); }

		public static Vector3 MinComponentV(this Vector3 v)
		{ return v.Component(v.MinI()); }

		public static Vector3d MaxComponentV(this Vector3d v)
		{ return v.Component(v.MaxI()); }

		public static Vector3d MinComponentV(this Vector3d v)
		{ return v.Component(v.MinI()); }

		public static float MaxComponentF(this Vector3 v)
		{ return v[v.MaxI()]; }

		public static float MinComponentF(this Vector3 v)
		{ return v[v.MinI()]; }

		public static double MaxComponentD(this Vector3d v)
		{ return v[v.MaxI()]; }

		public static double MinComponentD(this Vector3d v)
		{ return v[v.MinI()]; }
	}


	public static class CollectionsExtensions
	{
		public static TSource SelectMax<TSource>(this IEnumerable<TSource> s, Func<TSource, float> metric)
		{
			float max_v = -1;
			TSource max_e = default(TSource);
			foreach(TSource e in s)
			{
				float m = metric(e);
				if(m > max_v) { max_v = m; max_e = e; }
			}
			return max_e;
		}

		public static void ForEach<TSource>(this IEnumerable<TSource> E, Action<TSource> action)
		{
			var en = E.GetEnumerator();
			while(en.MoveNext()) action(en.Current);
		}

		public static void ForEach<TSource>(this IList<TSource> a, Action<TSource> action)
		{ for(int i = 0, len = a.Count; i < len; i++) action(a[i]); }

		public static void ForEach<TSource>(this TSource[] a, Action<TSource> action)
		{ for(int i = 0, len = a.Length; i < len; i++) action(a[i]); }

		public static TSource Pop<TSource>(this LinkedList<TSource> l)
		{
			TSource e = l.Last.Value;
			l.RemoveLast();
			return e;
		}

		public static TSource Min<TSource>(params TSource[] args) where TSource : IComparable
		{
			if(args.Length == 0) throw new InvalidOperationException("Min: arguments list should not be empty");
			TSource min = args[0];
			foreach(var arg in args)
			{ if(min.CompareTo(arg) < 0) min = arg; }
			return min;
		}

		public static TSource Max<TSource>(params TSource[] args) where TSource : IComparable
		{
			if(args.Length == 0) throw new InvalidOperationException("Max: arguments list should not be empty");
			TSource max = args[0];
			foreach(var arg in args)
			{ if(max.CompareTo(arg) > 0) max = arg; }
			return max;
		}

		public static V Next<K, V>(this SortedList<K,V> list, K key)
		{
			try
			{
				var i = list.IndexOfKey(key);
				var ni = (i+1) % list.Count;
				return list.Values[ni];
			} catch { return default(V); }
		}

		public static V Prev<K, V>(this SortedList<K,V> list, K key)
		{
			try
			{
				var i = list.IndexOfKey(key);
				var ni = i > 0? i-1 : list.Count-1;
				return list.Values[ni];
			} catch { return default(V); }
		}

		public static T Next<T>(this IList<T> list, T key)
		{
			try
			{
				var i = list.IndexOf(key);
				var ni = (i+1) % list.Count;
				return list[ni];
			} catch { return default(T); }
		}

		public static T Prev<T>(this IList<T> list, T key)
		{
			try
			{
				var i = list.IndexOf(key);
				var ni = i > 0? i-1 : list.Count-1;
				return list[ni];
			} catch { return default(T); }
		}

		#region Queue extensions
		public static void FillFrom<T>(this Queue<T> q, IEnumerable<T> content)
		{ q.Clear(); content.ForEach(q.Enqueue); }

		public static bool Remove<T>(this Queue<T> q, T item)
		{ 
			var count = q.Count;
			var new_content = q.Where(it => !object.Equals(it, item)).ToList();
			q.Clear(); new_content.ForEach(q.Enqueue);
			return q.Count != count;
		}

		public static bool MoveUp<T>(this Queue<T> q, T up)
		{
			if(object.Equals(up, q.Peek())) return false;
			var new_content = q.ToList();
			var upi = new_content.IndexOf(up);
			if(upi < 0) return false;
			new_content[upi] = new_content[upi-1];
			new_content[upi-1] = up;
			q.Clear(); new_content.ForEach(q.Enqueue);
			return true;
		}
		#endregion
	}


	public static class PartExtensions
	{
		#region from MechJeb2 PartExtensions
		public static bool HasModule<T>(this Part p) where T : PartModule
		{ return p.Modules.GetModule<T>() != null; }

		public static bool IsPhysicallySignificant(this Part p)
		{
			bool physicallySignificant = (p.physicalSignificance != Part.PhysicalSignificance.NONE);
			// part.PhysicsSignificance is not initialized in the Editor for all part. but physicallySignificant is useful there.
			if (HighLogic.LoadedSceneIsEditor)
				physicallySignificant = physicallySignificant && p.PhysicsSignificance != 1;
			//Landing gear set physicalSignificance = NONE when they enter the flight scene
			//Launch clamp mass should be ignored.
			physicallySignificant &= !p.HasModule<ModuleWheelBase>() && !p.HasModule<LaunchClamp>();
			return physicallySignificant;
		}

		public static float TotalMass(this Part p) { return p.mass+p.GetResourceMass(); }
		#endregion

		#region Find Modules or Parts
		public static Part RootPart(this Part p) 
		{ return p.parent == null ? p : p.parent.RootPart(); }

		public static List<Part> AllChildren(this Part p)
		{
			var all_children = new List<Part>{};
			foreach(Part ch in p.children) 
			{
				all_children.Add(ch);
				all_children.AddRange(ch.AllChildren());
			}
			return all_children;
		}

		public static List<Part> AllConnectedParts(this Part p)
		{
			if(p.parent != null) return p.parent.AllConnectedParts();
			var all_parts = new List<Part>{p};
			all_parts.AddRange(p.AllChildren());
			return all_parts;
		}

		public static Part AttachedPartWithModule<T>(this Part p) where T : PartModule
		{
			if(p.parent != null && p.parent.HasModule<T>()) return p.parent;
			foreach(var c in p.children) { if(c.HasModule<T>()) return c; }
			return null;
		}

		public static T GetModuleInAttachedPart<T>(this Part p) where T : PartModule
		{
			if(p.parent != null) { var m = p.parent.Modules.GetModule<T>(); if(m != null) return m; }
			foreach(var c in p.children) { var m = c.Modules.GetModule<T>(); if(m != null) return m; }
			return null;
		}

		public static List<ModuleT> AllModulesOfType<ModuleT>(this Part part, ModuleT exception = null)
			where ModuleT : PartModule
		{
			var passages = new List<ModuleT>();
			foreach(Part p in part.AllConnectedParts())
				passages.AddRange(from m in p.Modules.OfType<ModuleT>()
				                  where exception == null || m != exception
				                  select m);
			return passages;
		}

		public static ResourcePump CreateSocket(this Part p)
		{ return new ResourcePump(p, Utils.ElectricChargeID); }
		#endregion

		#region Resources and Phys-Props
		public static float TotalCost(this Part p) { return p.partInfo != null? p.partInfo.cost+p.GetModuleCosts(p.partInfo.cost) : 0; }

		public static float ResourcesCost(this Part p) 
		{ 
			var cost = 0.0;
			p.Resources.ForEach(r => cost += r.amount * r.info.unitCost);
			return (float)cost;
		}

		public static float MaxResourcesCost(this Part p) 
		{ 
			var cost = 0.0;
			p.Resources.ForEach(r => cost += r.maxAmount * r.info.unitCost);
			return (float)cost;
		}

		public static float DryCost(this Part p) { return p.TotalCost() - p.MaxResourcesCost(); }

		public static float MassWithChildren(this Part p)
		{
			float mass = p.TotalMass();
			p.children.ForEach(ch => mass += ch.MassWithChildren());
			return mass;
		}
		#endregion

		#region Actions
		public static void BreakConnectedCompoundParts(this Part p)
		{
			//break connected compound parts
			foreach(Part part in p.AllConnectedParts())
			{
				var cp = part as CompoundPart;
				if(cp == null) continue;
				var cpm = cp.Modules.GetModule<CompoundParts.CompoundPartModule>();
				if(cpm == null) continue;
				cpm.OnTargetLost();
			}
		}

		public static void UpdateAttachedPartPos(this Part p, AttachNode node)
		{
			if(node == null) return;
			var ap = node.attachedPart; 
			if(ap == null) return;
			var an = ap.FindAttachNodeByPart(p);	
			if(an == null) return;
			var dp =
				p.transform.TransformPoint(node.position) -
				ap.transform.TransformPoint(an.position);
			if(ap == p.parent) 
			{
				while (ap.parent) ap = ap.parent;
				ap.transform.position += dp;
				p.transform.position -= dp;
			} 
			else ap.transform.position += dp;
		}
		#endregion

		#region Logging
		public static string Title(this Part p) { return p.partInfo != null? p.partInfo.title : p.name; }

		public static void Log(this MonoBehaviour mb, string msg, params object[] args)
		{ Utils.Log(string.Format("{0}: {1}", mb.name, msg), args); }

		public static void Log(this Part p, string msg, params object[] args)
		{
			var vn = p.vessel == null? "_vessel" : (string.IsNullOrEmpty(p.vessel.vesselName)? p.vessel.id.ToString() : p.vessel.vesselName);
			Utils.Log(string.Format("{0}.{1} [{2}]: {3}", vn, p.Title(), p.flightID, msg), args);
		}
		#endregion

		#region Misc
		//directly from Part disassembly
		public static PartModule.StartState StartState(this Part part)
		{
			var _state = PartModule.StartState.None;
			if(HighLogic.LoadedSceneIsEditor)
				_state |= PartModule.StartState.Editor;
			else if(HighLogic.LoadedSceneIsFlight)
			{
				if(part.vessel.situation == Vessel.Situations.PRELAUNCH)
				{
					_state |= PartModule.StartState.PreLaunch;
					_state |= PartModule.StartState.Landed;
				}
				if(part.vessel.situation == Vessel.Situations.DOCKED)
					_state |= PartModule.StartState.Docked;
				if(part.vessel.situation == Vessel.Situations.ORBITING ||
				   part.vessel.situation == Vessel.Situations.ESCAPING)
					_state |= PartModule.StartState.Orbital;
				if(part.vessel.situation == Vessel.Situations.SUB_ORBITAL)
					_state |= PartModule.StartState.SubOrbital;
				if(part.vessel.situation == Vessel.Situations.SPLASHED)
					_state |= PartModule.StartState.Splashed;
				if(part.vessel.situation == Vessel.Situations.FLYING)
					_state |= PartModule.StartState.Flying;
				if(part.vessel.situation == Vessel.Situations.LANDED)
					_state |= PartModule.StartState.Landed;
			}
			return _state;
		}
		#endregion
	}


	public static class PartModuleExtensions
	{
		public static string Title(this PartModule pm) 
		{ return pm.part.partInfo != null? pm.part.partInfo.title : pm.part.name; }

		public static void EnableModule(this PartModule pm, bool enable)
		{ pm.enabled = pm.isEnabled = enable; }

		public static void ConfigurationInvalid(this PartModule pm, string msg, params object[] args)
		{
			Utils.Message(6, "WARNING: {0}.\n" +
			              "Configuration of \"{1}\" is INVALID.", 
			              string.Format(msg, args), 
			              pm.Title());
			pm.enabled = pm.isEnabled = false;
			return;
		}

		public static void Log(this PartModule pm, string msg, params object[] args)
		{ 
			var vn = pm.vessel == null? "_vessel" : (string.IsNullOrEmpty(pm.vessel.vesselName)? pm.vessel.id.ToString() : pm.vessel.vesselName);
			Utils.Log(string.Format("{0}:{1}:{2} [{3}]: {4}", vn, pm.part == null? "_part" : pm.part.Title(), pm.moduleName, pm.GetInstanceID(), msg), args); 
		}
	}


	public static class VesselExtensions
	{
		public static void Log(this Vessel v, string msg, params object[] args)
		{ Utils.Log(string.Format("{0}: {1}", v.vesselName, msg), args); }

		public static Part GetPart<T>(this Vessel v) where T : PartModule
		{ return v.parts.FirstOrDefault(p => p.HasModule<T>()); }

		public static bool PartsStarted(this Vessel v)
		{ return v.parts.TrueForAll(p => p.started); }

		public static bool HasLaunchClamp(this IShipconstruct ship)
		{
			foreach(Part p in ship.Parts)
			{ if(p.HasModule<LaunchClamp>()) return true; }
			return false;
		}

		public static void Unload(this ShipConstruct construct)
		{
			if(construct == null) return;
			for(int i = 0, count = construct.Parts.Count; i < count; i++)
			{
				Part p = construct.Parts[i];
				if(p != null)
				{
					p.OnDelete();
					if(p.gameObject != null)
						UnityEngine.Object.Destroy(p.gameObject);
				}
			}
			construct.Clear();
		}

		public static Bounds Bounds(this Vessel vessel, Transform refT)
		{
			//update physical bounds
			var b = new Bounds();
			bool inited = false;
			var parts = vessel.parts;
			for(int i = 0, partsCount = parts.Count; i < partsCount; i++)
			{
				Part p = parts[i];
				if(p == null) continue;
				var meshes = p.FindModelComponents<MeshFilter>();
				for(int mi = 0, meshesLength = meshes.Count; mi < meshesLength; mi++)
				{
					var m = meshes[mi];
					var bounds = Utils.BoundCorners(m.sharedMesh.bounds);
					for(int j = 0; j < 8; j++)
					{
						var c = refT.InverseTransformPoint(m.transform.TransformPoint(bounds[j]));
						if(inited) b.Encapsulate(c);
						else { b = new Bounds(c, Vector3.zero); inited = true; }
					}
				}
			}
			return b;
		}

		public static Bounds EnginesExhaust(this Vessel vessel)
		{
			var CoM = vessel.CurrentCoM;
			var refT = vessel.ReferenceTransform;
			var b = new Bounds();
			var inited = false;
			for(int i = 0, vesselPartsCount = vessel.Parts.Count; i < vesselPartsCount; i++)
			{
				var p = vessel.Parts[i];
				var engines = p.Modules.GetModules<ModuleEngines>();
				for(int j = 0, enginesCount = engines.Count; j < enginesCount; j++)
				{
					var e = engines[j];
					if(!e.exhaustDamage) continue;
					for(int k = 0, tCount = e.thrustTransforms.Count; k < tCount; k++)
					{
						var t = e.thrustTransforms[k];
						var term = refT.InverseTransformDirection(t.position + t.forward * e.exhaustDamageMaxRange - CoM);
						if(inited) b.Encapsulate(term);
						else { b = new Bounds(term, Vector3.zero); inited = true; }
					}
				}
			}
			return b;
		}

		public static float Radius(this Vessel vessel)
		{ 
			if(!vessel.loaded) return (float)Math.Pow(vessel.totalMass, 1/3.0);
			return vessel.Bounds(vessel.ReferenceTransform).size.magnitude;
		}
	}

	public class AtmosphereParams
	{
		public readonly CelestialBody Body;
		public readonly double Alt;
		public readonly double P = 0;
		public readonly double T = -273;
		public readonly double Rho = 0;
		public readonly double Mach1 = 0;

		public AtmosphereParams(CelestialBody body, double altitude)
		{
			Alt = altitude;
			Body = body;
			if(!Body.atmosphere) return;
			P = Body.GetPressure(Alt);
			T = Body.GetTemperature(Alt);
			Rho = Body.GetDensity(P, T);
			Mach1 = Body.GetSpeedOfSound(P, Rho);
		}

		public override string ToString()
		{
			return Utils.Format("{} Atmosphere Params at Alt: {} m\nP {}, T {}, Rho {}, Mach1 {} m/s",
			                    Body.name, Alt, P, T, Rho, Mach1);
		}
	}

	public static class OrbitalExtensions
	{
		public static bool ApAhead(this Orbit obt) { return obt.timeToAp < obt.timeToPe; }

		public static Vector3d hV(this Orbit obt, double UT) 
		{ 
			return Vector3d.Exclude(obt.getRelativePositionAtUT(UT), 
		                            obt.getOrbitalVelocityAtUT(UT));
		}

		public static double TerrainAltitude(this CelestialBody body, double Lat, double Lon)
		{
			if(body.pqsController == null) return 0;
			var alt = body.pqsController.GetSurfaceHeight(body.GetRelSurfaceNVector(Lat, Lon)) - body.pqsController.radius;
			return body.ocean && alt < 0? 0 : alt;
		}

		public static double TerrainAltitude(this CelestialBody body, Vector3d wpos)
		{ return TerrainAltitude(body, body.GetLatitude(wpos), body.GetLongitude(wpos)); }

		public static AtmosphereParams AtmoParamsAtAltitude(this CelestialBody body, double alt) 
		{ return new AtmosphereParams(body, alt); }
	}
}

