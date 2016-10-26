//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
using UnityEngine;

namespace AT_Utils
{
	public static partial class Utils
	{
		const string ElectricChargeName = "ElectricCharge";
		static PartResourceDefinition _electric_charge;
		public static PartResourceDefinition ElectricCharge
		{ 
			get 
			{ 
				if(_electric_charge == null)
					_electric_charge = PartResourceLibrary.Instance.GetDefinition(ElectricChargeName);	
				return _electric_charge; 
			} 
		}
		public static int ElectricChargeID { get { return ElectricCharge.id; } }


		/// <summary>
		/// The camel case components matching regexp.
		/// From: http://stackoverflow.com/questions/155303/net-how-can-you-split-a-caps-delimited-string-into-an-array
		/// </summary>
		const string CamelCaseRegexp = "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))";
		static Regex CCR = new Regex(CamelCaseRegexp);
		public static string ParseCamelCase(string s) { return CCR.Replace(s, "$1 "); }

		public static readonly char[] Delimiters = {' ', '\t', ',', ';'};
		public static readonly char[] Whitespace = {' ', '\t'};
		public static readonly char[] Semicolon = {';'};
		public static readonly char[] Comma = {','};

		public static string[] ParseLine(string line, char[] delims, bool trim = true)
		{
			if(string.IsNullOrEmpty(line)) return null;
			var array = line.Split(delims, StringSplitOptions.RemoveEmptyEntries);
			if(trim) { for(int i = 0, len = array.Length; i < len; i++) array[i] = array[i].Trim(); }
			return array;
		}

		public static string formatVeryBigValue(float value, string unit, string format = "F1")
		{
			string mod = "";
			if(value > 1e24) { value /= 1e24f; mod = "Y"; }
			else if(value > 1e21) { value /= 1e21f; mod = "Z"; }
			else if(value > 1e18) { value /= 1e18f; mod = "E"; }
			else if(value > 1e15) { value /= 1e15f; mod = "P"; }
			else if(value > 1e12) { value /= 1e12f; mod = "T"; }
			else return formatBigValue(value, unit, format);
			return value.ToString(format)+mod+unit;
		}

		public static string formatBigValue(float value, string unit, string format = "F1")
		{
			string mod = "";
			if     (value > 1e9) { value /= 1e9f; mod = "G"; }
			else if(value > 1e6) { value /= 1e6f; mod = "M"; }
			else if(value > 1e3) { value /= 1e3f; mod = "k"; }
			return value.ToString(format)+mod+unit;
		}

		public static string formatSmallValue(float value, string unit, string format = "F1")
		{
			string mod = "";
			if(value > 1e-3) { value *= 1e3f; mod = "m"; }
			else if(value > 1e-6) { value *= 1e6f; mod = "μ"; }
			else if(value > 1e-9) { value *= 1e9f; mod = "n"; }
			return value.ToString(format)+mod+unit;
		}

		public static string formatMass(float mass)
		{
			if(mass >= 0.1f)
				return mass.ToString("n2") + "t";
			if(mass >= 0.001f)
				return (mass * 1e3f).ToString("n1") + "kg";
			return (mass * 1e6f).ToString("n0") + "g";
		}

		public static string formatVolume(double volume)
		{
			if(volume < 1f)
				return (volume * 1e3f).ToString ("n0") + "L";
			return volume.ToString("n1") + "m3";
		}

		public static string formatUnits(float units)
		{
			units = Mathf.Abs(units);
			if(units >= 1f)
				return units.ToString ("n2") + "u";
			if(units >= 1e-3f)
				return (units * 1e3f).ToString ("n1") + "mu";
			if(units >= 1e-6f)
				return (units * 1e6f).ToString ("n1") + "mku";
			if(units >= 1e-9f)
				return (units * 1e9f).ToString ("n1") + "nu";
			if(units >= 1e-13f) //to fully use the last digit 
				return (units * 1e12f).ToString ("n1") + "pu";
			return "0.0u"; //effectivly zero
		}

//		public static string FormatTimeDelta(double value)
//		{
//			var h = 0;
//			if(value > 3600) h
//		}

		public static string formatDimensions(Vector3 size)
		{ return string.Format("{0:F2}m x {1:F2}m x {2:F2}m", size.x, size.y, size.z); }

		public static string formatVector(Vector3 v)
		{ return string.Format("({0}, {1}, {2}); |v| = {3}", v.x, v.y, v.z, v.magnitude); }

		public static string formatVector(Vector3d v)
		{ return string.Format("({0}, {1}, {2}); |v| = {3}", v.x, v.y, v.z, v.magnitude); }

		public static string formatOrbit(Orbit o)
		{
			return Utils.Format(
				"Body:   {}\n" +
				"\trotation: {} s\n" +
				"\tradius:   {}\n" +
				"\trot angle {} deg\n" +
				"PeA:    {} m\n" +
				"ApA:    {} m\n" +
				"PeR:    {} m\n" +
				"ApR:    {} m\n" +
				"Ecc:    {}\n" +
				"Inc:    {} deg\n" +
				"LAN:    {} deg\n" +
				"MA:     {} rad\n" +
				"TA:     {} deg\n" +
				"AoP:    {} deg\n" +
				"Period: {} s\n" +
				"epoch:   {}\n" +
				"T@epoch: {} s\n" +
				"T:       {} s\n" +
				"T2Pe     {} per\n" +
				"T2Ap     {} per\n" +
				"Vel: {} m/s\n" +
				"Pos: {} m\n",
				o.referenceBody.bodyName, o.referenceBody.rotationPeriod, o.referenceBody.rotationAngle,
				formatBigValue((float)o.referenceBody.Radius, "m"),
				o.PeA, o.ApA,
				o.PeR, o.ApR, 
				o.eccentricity, o.inclination, o.LAN, o.meanAnomaly, o.trueAnomaly, o.argumentOfPeriapsis,
				o.period, o.epoch, o.ObTAtEpoch, o.ObT,
				o.timeToPe/o.period, o.timeToAp/o.period,
				formatVector(o.vel), formatVector(o.pos));
		}

		public static string formatBounds(Bounds b, string name="")
		{
			return string.Format("Bounds:  {0}\n" +
			                     "Center:  {1}\n" +
			                     "Extents: {2}\n" +
			                     "Min:     {3}\n" +
			                     "Max:     {4}\n" +
			                     "Volume:  {5}", 
			                     name, b.center, b.extents, b.min, b.max,
			                     b.size.x*b.size.y*b.size.z);
		}

		public static string formatException(Exception ex)
		{ return string.Format("{0}\n{1}\n{2}", ex.Message, ex.Source, ex.StackTrace); }

		public static string Format(string s, params object[] args)
		{
			if(args == null || args.Length == 0) return s;
			convert_args(args);
			for(int i = 0, argsLength = args.Length; i < argsLength; i++)
			{
				var ind = s.IndexOf("{}"); 
				if(ind >= 0) s = s.Substring(0, ind)+"{"+i+"}"+s.Substring(ind+2);
				else s += string.Format(" arg{0}: {{{0}}}", i);
			}
			return string.Format(s.Replace("{}", "[no arg]"), args);
		}

		static void convert_args(object[] args)
		{
			for(int i = 0, argsL = args.Length; i < argsL; i++) 
			{
				var arg = args[i];
				if(arg is string) continue;
				else if(arg == null) args[i] = "null";
				else if(arg is Vector3) args[i] = formatVector((Vector3)arg);
				else if(arg is Vector3d) args[i] = formatVector((Vector3d)arg);
				else if(arg is Orbit) args[i] = formatOrbit((Orbit)arg);
				else if(arg is Bounds) args[i] = formatBounds((Bounds)arg);
				else if(arg is Exception) args[i] = formatException((Exception)arg);
				else if(arg is IEnumerable) 
				{
					var arr = (arg as IEnumerable).Cast<object>().ToArray();
					convert_args(arr);
					args[i] = string.Concat("[\n", 
					                        arr.Aggregate("", (s, el) => 
					                                      string.IsNullOrEmpty(s)? 
					                                      el.ToString() : string.Concat(s, ",\n", el)),
					                        "\n]");
				}
				else args[i] = arg.ToString();
			}
		}

		static string prepare_message(string msg)
		{
			var mod_name = "AT_Utils";
			var stack = new StackTrace(2);
			foreach(var f in stack.GetFrames())
			{
				var method = f.GetMethod();
				if(log_re.IsMatch(method.Name)) continue;
				mod_name = method.DeclaringType.Assembly.GetName().Name;
				break;
			}
			#if DEBUG
			UnityEngine.Debug.Log(stack);
			#endif
			return string.Format("[{0}: {1:HH:mm:ss.fff}] {2}", mod_name, DateTime.Now, msg);
		}

		static readonly Regex log_re = new Regex("[Ll]og");
		public static void Log(string msg, params object[] args)
		{ 
			msg = prepare_message(msg);
			if(args.Length > 0)
			{
				convert_args(args);
				UnityEngine.Debug.Log(Format(msg, args)); 
			}
			else UnityEngine.Debug.Log(msg);
		}

		public static void Log2File(string filename, string msg, params object[] args)
		{
			using(var f = new StreamWriter(filename, true))
			{
				msg = prepare_message(msg);
				if(args.Length > 0)
				{
					convert_args(args);
					f.WriteLine(Format(msg, args));
				}
				else f.WriteLine(msg);
			}
		}

		//from http://stackoverflow.com/questions/716399/c-sharp-how-do-you-get-a-variables-name-as-it-was-physically-typed-in-its-dec
		//second answer
		public static string PropertyName<T>(T obj) { return typeof(T).GetProperties()[0].Name; }

		public static bool PartIsPurchased(string name)
		{
			if(PartLoader.Instance == null) return false;
			var info = PartLoader.getPartInfoByName(name);
			if(info == null || HighLogic.CurrentGame == null) return false;
			return HighLogic.CurrentGame.Mode != Game.Modes.CAREER || ResearchAndDevelopment.PartModelPurchased(info);
		}

		public static Vector3[] BoundCorners(Bounds b)
		{
			var edges = new Vector3[8];
			Vector3 min = b.min;
			Vector3 max = b.max;
			edges[0] = new Vector3(min.x, min.y, min.z); //left-bottom-back
			edges[1] = new Vector3(min.x, min.y, max.z); //left-bottom-front
			edges[2] = new Vector3(min.x, max.y, min.z); //left-top-back
			edges[3] = new Vector3(min.x, max.y, max.z); //left-top-front
			edges[4] = new Vector3(max.x, min.y, min.z); //right-bottom-back
			edges[5] = new Vector3(max.x, min.y, max.z); //right-bottom-front
			edges[6] = new Vector3(max.x, max.y, min.z); //right-top-back
			edges[7] = new Vector3(max.x, max.y, max.z); //right-top-front
			return edges;
		}

		public static Vector3[] BoundCorners(Vector3 center, Vector3 size)
		{
			var b = new Bounds(center, size);
			return BoundCorners(b);
		}

		//KSP-provided System.dll declares Path.Combine(strin[]) as internal O_o
		public static string PathChain(params string[] paths)
		{
			string path = "";
			for(int p = 0, len = paths.Length; p < len; p++)
				path = Path.Combine(path, paths[p]);
			return path;
		}

		//sound (from the KAS mod; KAS_Shared class)
		public static bool createFXSound(Part part, FXGroup group, string sndPath, bool loop, float maxDistance = 30f)
		{
			group.audio = part.gameObject.AddComponent<AudioSource>();
			group.audio.volume = GameSettings.SHIP_VOLUME;
			group.audio.rolloffMode = AudioRolloffMode.Logarithmic;
			group.audio.dopplerLevel = 0f;
			group.audio.maxDistance = maxDistance;
			group.audio.loop = loop;
			group.audio.playOnAwake = false;
			if(GameDatabase.Instance.ExistsAudioClip(sndPath))
			{
				group.audio.clip = GameDatabase.Instance.GetAudioClip(sndPath);
				return true;
			}
			Utils.Message(10, "Sound file : {0} has not been found, please check your Hangar installation", sndPath);
			return false;
		}

		public static void SaveGame(string name, bool with_message = true)
		{ 
			Game game = HighLogic.CurrentGame.Updated();
			game.startScene = GameScenes.FLIGHT;
			GamePersistence.SaveGame(game, name, HighLogic.SaveFolder, SaveMode.OVERWRITE);
			if(with_message) Message("Game saved as: {0}", name);
		}
	}

	public static class WaitWithPhysics
	{
		public static void DelayPhysicsForSeconds(float dt)
		{ OrbitPhysicsManager.HoldVesselUnpack(Mathf.CeilToInt(dt/TimeWarp.fixedDeltaTime)+1); }

		public static WaitForSeconds ForSeconds(float dt)
		{
			DelayPhysicsForSeconds(dt);
			return new WaitForSeconds(dt);
		}

		public static WaitForFixedUpdate ForFixedUpdate()
		{
			OrbitPhysicsManager.HoldVesselUnpack(2);
			return new WaitForFixedUpdate();
		}

		public static YieldInstruction ForNextUpdate()
		{
			DelayPhysicsForSeconds(TimeWarp.deltaTime);
			return null;
		}
	}

	public class ListDict<K,V> : Dictionary<K, List<V>>
	{
		public void Add(K key, V value)
		{
			List<V> lst;
			if(TryGetValue(key, out lst))
				lst.Add(value);
			else this[key] = new List<V>{value};
		}
	}
}
