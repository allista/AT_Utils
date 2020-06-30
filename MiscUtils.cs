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
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AT_Utils
{
    public static partial class Utils
    {
        public static readonly ResourceInfo ElectricCharge = new ResourceInfo("ElectricCharge");

        private static readonly Dictionary<string, int> _layers = new Dictionary<string, int>();

        public static int GetLayer(string name)
        {
            if(_layers.TryGetValue(name, out var layer))
                return layer;
            layer = 1 << LayerMask.NameToLayer(name);
            _layers[name] = layer;
            return layer;
        }

        public static int GetLayers(params string[] names)
        {
            var layers = 0;
            for(int i = 0, len = names.Length; i < len; i++)
                layers |= GetLayer(names[i]);
            return layers;
        }

        /// <summary>
        /// The camel case components matching regexp.
        /// From: http://stackoverflow.com/questions/155303/net-how-can-you-split-a-caps-delimited-string-into-an-array
        /// </summary>
        private const string CamelCaseRegexp = "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))";

        private static readonly Regex CCR = new Regex(CamelCaseRegexp);

        public static string ParseCamelCase(string s) => CCR.Replace(s, "$1 ");

        public static readonly char[] Delimiters = { ' ', '\t', ',', ';' };
        public static readonly char[] Whitespace = { ' ', '\t' };
        public static readonly char[] Semicolon = { ';' };
        public static readonly char[] Comma = { ',' };

        public static string[] ParseLine(string line, char[] delimiters, bool trim = true)
        {
            if(string.IsNullOrEmpty(line))
                return new string[] { };
            var array = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            if(!trim)
                return array;
            for(int i = 0, len = array.Length; i < len; i++)
                array[i] = array[i].Trim();
            return array;
        }

        public static string formatVeryBigValue(float value, string unit, string format = "F1")
        {
            string mod;
            if(value > 1e24)
            {
                value /= 1e24f;
                mod = "Y";
            }
            else if(value > 1e21)
            {
                value /= 1e21f;
                mod = "Z";
            }
            else if(value > 1e18)
            {
                value /= 1e18f;
                mod = "E";
            }
            else if(value > 1e15)
            {
                value /= 1e15f;
                mod = "P";
            }
            else if(value > 1e12)
            {
                value /= 1e12f;
                mod = "T";
            }
            else
                return formatBigValue(value, unit, format);
            return value.ToString(format) + mod + unit;
        }

        public static string formatBigValue(float value, string unit, string format = "F1")
        {
            var mod = "";
            if(value > 1e9)
            {
                value /= 1e9f;
                mod = "G";
            }
            else if(value > 1e6)
            {
                value /= 1e6f;
                mod = "M";
            }
            else if(value > 1e3)
            {
                value /= 1e3f;
                mod = "k";
            }
            return value.ToString(format) + mod + unit;
        }

        public static string formatSmallValue(float value, string unit, string format = "F1")
        {
            var mod = "";
            if(value < 1)
            {
                if(value > 1e-3)
                {
                    value *= 1e3f;
                    mod = "m";
                }
                else if(value > 1e-6)
                {
                    value *= 1e6f;
                    mod = "μ";
                }
                else if(value > 1e-9)
                {
                    value *= 1e9f;
                    mod = "n";
                }
            }
            return value.ToString(format) + mod + unit;
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
                return (volume * 1e3f).ToString("n0") + "L";
            return volume.ToString("n1") + "m3";
        }

        public static string formatUnits(float units)
        {
            units = Mathf.Abs(units);
            if(units >= 1f)
                return units.ToString("n2") + "u";
            if(units >= 1e-3f)
                return (units * 1e3f).ToString("n1") + "mu";
            if(units >= 1e-6f)
                return (units * 1e6f).ToString("n1") + "μu";
            if(units >= 1e-9f)
                return (units * 1e9f).ToString("n1") + "nu";
            if(units >= 1e-13f) //to fully use the last digit 
                return (units * 1e12f).ToString("n1") + "pu";
            return "0.0u"; // effectively zero
        }

        private readonly struct _DateTime
        {
            public readonly int seconds, minutes, hours, days, years;

            public _DateTime(double time, int year_len, int day_len)
            {
                years = (int)(time / year_len);
                time -= years * year_len;
                int secs = (int)time;
                seconds = secs % 60;
                minutes = secs / 60 % 60;
                hours = secs / 3600 % (day_len / 3600);
                days = secs / day_len;
            }
        }

        public static string formatTimeDelta(double delta)
        {
            var dt = new _DateTime(delta, KSPUtil.dateTimeFormatter.Year, KSPUtil.dateTimeFormatter.Day);
            if(dt.years > 0)
                return $"{dt.years}y {dt.days,3}d {dt.hours,2}:{dt.minutes:00}:{dt.seconds:00}";
            if(dt.days > 0)
                return $"{dt.days,3}d {dt.hours,2}:{dt.minutes:00}:{dt.seconds:00}";
            return $"{dt.hours,2}:{dt.minutes:00}:{dt.seconds:00}";
        }

        public static string formatDimensions(Vector3 size) => $"{size.x:F2}m x {size.y:F2}m x {size.z:F2}m";

        public static string formatVector(Vector3 v) => $"({v.x}, {v.y}, {v.z}); |v| = {v.magnitude}";

        public static string formatVector(Vector3d v) => $"({v.x}, {v.y}, {v.z}); |v| = {v.magnitude}";

        public static string formatComponents(Vector3 v) =>
            $"[{v.x: 0.000;-0.000; 0.000;}, {v.y: 0.000;-0.000; 0.000;}, {v.z: 0.000;-0.000; 0.000;}]";

        public static string formatCB(CelestialBody cb)
        {
            if(cb == null)
                return "Body:   null";
            return Utils.Format(
                "Body:   {}\n" + "\trotation: {} s\n" + "\tradius:   {}\n" + "\trot angle {} deg",
                cb.bodyName,
                cb.rotationPeriod,
                formatBigValue((float)cb.Radius, "m"),
                cb.rotationAngle);
        }

        public static string formatOrbit(Orbit o)
        {
            return Utils.Format(
                "{}\n"
                + "PeA:    {} m\n"
                + "ApA:    {} m\n"
                + "PeR:    {} m\n"
                + "ApR:    {} m\n"
                + "SMA:    {} m\n"
                + "SmA:    {} m\n"
                + "Ecc:    {}\n"
                + "Inc:    {} deg\n"
                + "LAN:    {} deg\n"
                + "MA:     {} rad\n"
                + "TA:     {} rad\n"
                + "AoP:    {} deg\n"
                + "Period: {} s\n"
                + "epoch:   {}\n"
                + "T@epoch: {} s\n"
                + "T:       {} s\n"
                + "T2Pe     {} s\n"
                + "T2Ap     {} s\n"
                + "StartUT  {}  StartTrans: {}\n"
                + "EndUT    {}, EndTrans:   {}\n"
                + "Vel: {} m/s\n"
                + "Pos: {} m\n",
                formatCB(o.referenceBody),
                o.PeA,
                o.ApA,
                o.PeR,
                o.ApR,
                o.semiMajorAxis,
                o.semiMinorAxis,
                o.eccentricity,
                o.inclination,
                o.LAN,
                o.meanAnomaly,
                o.trueAnomaly,
                o.argumentOfPeriapsis,
                o.period,
                o.epoch,
                o.ObTAtEpoch,
                o.ObT,
                o.timeToPe,
                o.timeToAp,
                o.StartUT,
                o.patchStartTransition,
                o.EndUT,
                o.patchEndTransition,
                formatVector(o.vel),
                formatVector(o.pos));
        }

        public static string formatPatches(Orbit o, string tag)
        {
            var with_tag = !string.IsNullOrEmpty(tag);
            var ret = with_tag
                ? Format("===================== {} : {} =======================\n{}",
                    tag,
                    Planetarium.GetUniversalTime(),
                    o)
                : formatOrbit(o);
            ret += "\n";
            if(o.nextPatch != null
               && o.nextPatch.referenceBody != null
               && o.patchEndTransition != Orbit.PatchTransitionType.FINAL)
                ret += formatPatches(o.nextPatch, "");
            if(with_tag)
                ret += "===================================================================\n";
            return ret;
        }

        public static string formatBounds(Bounds b, string name = "") =>
            $"Bounds:  {name}\nCenter:  {b.center}\nExtents: {b.extents}\nMin:     {b.min}\nMax:     {b.max}\nVolume:  {b.size.x * b.size.y * b.size.z}";

        public static string formatException(Exception ex) => $"{ex.Message}\n{ex.Source}\n{ex.StackTrace}";

        public static string Format(string s, params object[] args)
        {
            if(args == null || args.Length == 0)
                return s;
            convert_args(args);
            for(int i = 0, argsLength = args.Length; i < argsLength; i++)
            {
                var ind = s.IndexOf("{}", StringComparison.InvariantCulture);
                if(ind >= 0)
                    s = s.Substring(0, ind) + "{" + i + "}" + s.Substring(ind + 2);
                else
                    s += string.Format(" arg{0}: {{{0}}}", i);
            }
            return string.Format(s.Replace("{}", "[no arg]"), args);
        }

        private static void convert_args(object[] args)
        {
            for(int i = 0, argsL = args.Length; i < argsL; i++)
            {
                var arg = args[i];
                switch(arg)
                {
                    case string _:
                        continue;
                    case null:
                        args[i] = "null";
                        break;
                    case Vector3 vector3:
                        args[i] = formatVector(vector3);
                        break;
                    case Vector3d vector3d:
                        args[i] = formatVector(vector3d);
                        break;
                    case CelestialBody body:
                        args[i] = formatCB(body);
                        break;
                    case Orbit orbit:
                        args[i] = formatOrbit(orbit);
                        break;
                    case Bounds bounds:
                        args[i] = formatBounds(bounds);
                        break;
                    case Exception exc:
                        args[i] = formatException(exc);
                        break;
                    case IConfigNode node:
                        args[i] = node.ToConfigString();
                        break;
                    case Transform t:
                        args[i] = $"{t.name}: pos {t.position}, rot {t.rotation.eulerAngles}";
                        break;
                    case Object obj:
                        args[i] = obj.GetID();
                        break;
                    case IEnumerable enumerable:
                    {
                        var arr = enumerable.Cast<object>().ToArray();
                        convert_args(arr);
                        args[i] = string.Join("\n",
                            $"Count: {arr.Length}",
                            "[",
                            string.Join(",\n", arr.Cast<string>().ToArray()),
                            "]");
                        break;
                    }
                    default:
                        args[i] = arg.ToString();
                        break;
                }
            }
        }

        static string prepare_message(string msg)
        {
            var mod_name = "AT_Utils";
            var stack = new StackTrace(2);
            var frames = stack.GetFrames();
            if(frames != null)
            {
                foreach(var f in frames)
                {
                    var method = f.GetMethod();
                    if(log_re.IsMatch(method.Name))
                        continue;
                    if(method.DeclaringType != null)
                        mod_name = method.DeclaringType.Assembly.GetName().Name;
                    break;
                }
            }
#if DEBUG
            UnityEngine.Debug.Log(stack);
#endif
            return $"[{mod_name}: {DateTime.Now:HH:mm:ss.fff} {Time.frameCount}] {msg}";
        }

        static readonly Regex log_re = new Regex("[Ll]og");


        public static void Log(string msg)
        {
            msg = prepare_message(msg);
            UnityEngine.Debug.Log(msg);
#if DEBUG
            BackupLogger.LogRaw(msg);
#endif
        }
        
        public static void Log(string msg, params object[] args)
        {
            if(args.Length > 0)
            {
                convert_args(args);
                msg = Format(msg, args);
            }
            Log(msg);
        }

        public static void Debug(string msg)
        {
#if DEBUG
            Log($"DEBUG: {msg}");
#endif
        }
        
        public static void Debug(string msg, params object[] args)
        {
#if DEBUG
            Log($"DEBUG: {msg}", args);
#endif
        }

        public static void Info(string msg) => Log($"INFO: {msg}");
        public static void Info(string msg, params object[] args) => Log($"INFO: {msg}", args);
        public static void Warning(string msg) => Log($"WARNING: {msg}");
        public static void Warning(string msg, params object[] args) => Log($"WARNING: {msg}", args);
        public static void Error(string msg) => Log($"ERROR: {msg}");
        public static void Error(string msg, params object[] args) => Log($"ERROR: {msg}", args);

        public static void Log2File(string filename, string msg, params object[] args)
        {
            using(var f = new StreamWriter(filename, true))
            {
                if(args.Length > 0)
                {
                    convert_args(args);
                    msg = Format(msg, args);
                }
                f.WriteLine(prepare_message(msg));
            }
        }

        public static bool PartIsPurchased(string name)
        {
            if(PartLoader.Instance == null)
                return false;
            var info = PartLoader.getPartInfoByName(name);
            return info != null && PartIsPurchased(info);
        }

        public static bool PartIsPurchased(AvailablePart info)
        {
            return (HighLogic.CurrentGame != null
                    && (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX
                        || ResearchAndDevelopment.PartModelPurchased(info)));
        }

        public static Vector3[] BoundCorners(Bounds b)
        {
            var corners = new Vector3[8];
            var min = b.min;
            var max = b.max;
            corners[0] = new Vector3(min.x, min.y, min.z); //left-bottom-back
            corners[1] = new Vector3(min.x, min.y, max.z); //left-bottom-front
            corners[2] = new Vector3(min.x, max.y, min.z); //left-top-back
            corners[3] = new Vector3(min.x, max.y, max.z); //left-top-front
            corners[4] = new Vector3(max.x, min.y, min.z); //right-bottom-back
            corners[5] = new Vector3(max.x, min.y, max.z); //right-bottom-front
            corners[6] = new Vector3(max.x, max.y, min.z); //right-top-back
            corners[7] = new Vector3(max.x, max.y, max.z); //right-top-front
            return corners;
        }

        public static int[] BoundTriangles()
        {
            return new[] {
                0, 1, 2, 2, 1, 3, //left
                3, 1, 7, 7, 1, 5, //front
                5, 4, 7, 7, 4, 6, //right
                6, 4, 2, 2, 4, 0, //back
                2, 6, 3, 3, 6, 7, //top
                0, 4, 1, 1, 4, 5, //bottom
            };
        }

        public static Vector3[] BoundCorners(Vector3 center, Vector3 size)
        {
            var b = new Bounds(center, size);
            return BoundCorners(b);
        }

        //KSP-provided System.dll declares Path.Combine(strin[]) as internal O_o
        public static string PathChain(params string[] paths)
        {
            var path = "";
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
            Message(10, "Sound file : {0} has not been found, please check your Hangar installation", sndPath);
            return false;
        }

        public static void SaveGame(string name, bool with_message = true)
        {
            var game = HighLogic.CurrentGame.Updated();
            game.startScene = GameScenes.FLIGHT;
            GamePersistence.SaveGame(game, name, HighLogic.SaveFolder, SaveMode.OVERWRITE);
            if(with_message)
                Message("Game saved as: {0}", name);
        }

        // ReSharper disable once IteratorNeverReturns
        public static IEnumerator<YieldInstruction> SlowUpdate(Action action, float period = 1)
        {
            while(true)
            {
                action();
                yield return new WaitForSeconds(period);
            }
        }

        public static bool NameMatches(string name, IList<string> list)
        {
            if(string.IsNullOrEmpty(name))
                return false;
            for(int j = 0, count = list.Count; j < count; j++)
            {
                var lname = list[j];
                if(string.IsNullOrEmpty(lname))
                    continue;
                if(name.IndexOf(lname, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        public static Collider AddCollider(this MeshFilter mesh, bool isTrigger = false)
        {
            var col = mesh.GetComponent<Collider>();
            if(col == null || !col.isTrigger)
            {
                var collider = mesh.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh.sharedMesh;
                collider.convex = true;
                collider.isTrigger = isTrigger;
                col = collider;
            }
            col.enabled = true;
            return col;
        }
    }

    public static class WaitWithPhysics
    {
        public static void DelayPhysicsForSeconds(float dt)
        {
            OrbitPhysicsManager.HoldVesselUnpack(Mathf.CeilToInt(dt / TimeWarp.fixedDeltaTime) + 1);
        }

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

    public class ListDict<K, V> : Dictionary<K, List<V>>
    {
        public void Add(K key, V value)
        {
            if(TryGetValue(key, out var lst))
                lst.Add(value);
            else
                this[key] = new List<V> { value };
        }
    }
}
