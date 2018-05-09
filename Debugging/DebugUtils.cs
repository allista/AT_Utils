//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri
//
// This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.

#if DEBUG
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace AT_Utils
{
    public static class DebugUtils
    {
        public static void CSV(string filename, params object[] args)
        { 
            var row = new StringBuilder();
            var len = args.Length;
            var last_i = len-1;
            for(int i = 0; i < len; i++) 
            {
                var arg = args[i];
                if(arg is Vector2)
                {
                    var v = (Vector2)arg;
                    row.Append(v.x);
                    row.Append(",");
                    row.Append(v.y);
                }
                else if(arg is Vector2d)
                {
                    var v = (Vector2d)arg;
                    row.Append(v.x);
                    row.Append(",");
                    row.Append(v.y);
                }
                else if(arg is Vector3)
                {
                    var v = (Vector3)arg;
                    row.Append(v.x);
                    row.Append(",");
                    row.Append(v.y);
                    row.Append(",");
                    row.Append(v.z);
                }
                else if(arg is Vector3d)
                {
                    var v = (Vector3d)arg;
                    row.Append(v.x);
                    row.Append(",");
                    row.Append(v.y);
                    row.Append(",");
                    row.Append(v.z);
                }
                else if(arg is Vector4)
                {
                    var v = (Vector4)arg;
                    row.Append(v.x);
                    row.Append(",");
                    row.Append(v.y);
                    row.Append(",");
                    row.Append(v.z);
                    row.Append(",");
                    row.Append(v.w);
                }
                else if(arg is Vector4d)
                {
                    var v = (Vector4d)arg;
                    row.Append(v.x);
                    row.Append(",");
                    row.Append(v.y);
                    row.Append(",");
                    row.Append(v.z);
                    row.Append(",");
                    row.Append(v.w);
                }
                else if(arg is string)
                {
                    row.Append("\"");
                    row.Append(arg);
                    row.Append("\"");
                }
                else
                    row.Append(arg);
                if(i < last_i) row.Append(", ");
            }
            using(var f = new StreamWriter(filename, true))
                f.WriteLine(row);
        }

        public static void logVectors(string tag, bool normalize = true, params Vector3[] vecs)
        {
            var s = tag+":\n";
            foreach(var v in vecs)
            {
                var vn = normalize? v.normalized : v;
                s += string.Format("({0}, {1}, {2}),\n", vn.x, vn.y, vn.z);
            }
            Utils.Log(s);
        }

        public static string formatSteering(Vector3 steering)
        { return Utils.Format("[pitch {}, roll {}, yaw {}]", steering.x, steering.y, steering.z); }

        public static string formatSteering(FlightCtrlState s)
        { return Utils.Format("[pitch {}, roll {}, yaw {}]", s.pitch, s.roll, s.yaw); }

        public static string FormatActions(BaseActionList actions)
        {
            return actions.Aggregate("", (s, a) => s + string.Format("{0} ({1}, active: {2}); ", 
                                                                     a.guiName, a.actionGroup, a.active));
        }

        public static string getStacktrace(int skip = 0) { return new StackTrace(skip+1, true).ToString(); }

        public static void Log(string msg, params object[] args)
        { Utils.Log("{}\n{}", Utils.Format(msg, args), getStacktrace(1)); }

        public static void logStamp(string msg = "") { Utils.Log("=== " + msg); }

        public static void logCrewList(List<ProtoCrewMember> crew)
        {
            string crew_str = "";
            foreach(ProtoCrewMember c in crew)
                crew_str += string.Format("\n{0}, seat {1}, seatIdx {2}, roster {3}, ref {4}", 
                                          c.name, c.seat, c.seatIdx, c.rosterStatus, c.KerbalRef);
            Utils.Log("Crew List:{}", crew_str);
        }

        public static Vector3d planetaryPosition(Vector3 v, CelestialBody planet) 
        { 
            double lng = planet.GetLongitude(v);
            double lat = planet.GetLatitude(v);
            double alt = planet.GetAltitude(v);
            return planet.GetWorldSurfacePosition(lat, lng, alt);
        }

        public static void logPlanetaryPosition(Vector3 v, CelestialBody planet) 
        { Utils.Log("Planetary position: {}", planetaryPosition(v, planet));    }

        public static void logLongLatAlt(Vector3 v, CelestialBody planet) 
        { 
            double lng = planet.GetLongitude(v);
            double lat = planet.GetLatitude(v);
            double alt = planet.GetAltitude(v);
            Utils.Log("Long: {}, Lat: {}, Alt: {}", lng, lat, alt);
        }

        public static void logProtovesselCrew(ProtoVessel pv)
        {
            for(int i = 0; i < pv.protoPartSnapshots.Count; i++)
            {
                ProtoPartSnapshot p = pv.protoPartSnapshots[i];
                Utils.Log("Part{}: {}", i, p.partName);
                if(p.partInfo.partPrefab != null)
                    Utils.Log("partInfo.partPrefab.CrewCapacity {}",p.partInfo.partPrefab.CrewCapacity);
                Utils.Log("partInfo.internalConfig: {}", p.partInfo.internalConfig);
                Utils.Log("partStateValues.Count: {}", p.partStateValues.Count);
                foreach(string k in p.partStateValues.Keys)
                    Utils.Log ("{} : {}", k, p.partStateValues[k]);
                Utils.Log("modules.Count: {}", p.modules.Count);
                foreach(ProtoPartModuleSnapshot pm in p.modules)
                    Utils.Log("{} : {}", pm.moduleName, pm.moduleValues);
                foreach(string k in p.partStateValues.Keys)
                    Utils.Log("{} : {}", k, p.partStateValues[k]);
                Utils.Log("customPartData: {}", p.customPartData);
            }
        }

        public static void logTransfrorm(Transform T)
        {
            Utils.Log
            (
                "Transform: {}\n" +
                "Position: {}\n" +
                "Rotation: {}\n"+
                "Local Position: {}\n" +
                "Local Rotation: {}",
                T.name, 
                T.position, T.eulerAngles,
                T.localPosition, T.localEulerAngles
            );
        }

        public static string formatTransformTree(Transform T, string indent = "")
        {
            var log = new List<string>();
            log.Add(string.Format("{0}{1}, scl: {2}, pos: {3}, rot: {4}\n", 
                                  indent, T, T.localScale, T.localPosition, T.localRotation.eulerAngles));
            indent += "\t";
            for(int i = 0; i < T.childCount; i++)
                log.Add(formatTransformTree(T.GetChild(i), indent));
            return string.Join("", log.ToArray());
        }

        public static void logShipConstruct(ShipConstruct ship)
        {
            Utils.Log("ShipConstruct: {}\n{}",
                      ship.shipName,
                      ship.parts.Aggregate("", (s, p) => s + p.Title() + "\n"));
        }

        //does not work with monodevelop generated .mdb files =(
//        public static void LogException(Action action)
//        {
//            try { action(); }
//            catch(Exception ex)
//            {
//                // Get stack trace for the exception with source file information
//                var st = new StackTrace(ex, true);
//                // Get the top stack frame
//                var frame = st.GetFrame(st.FrameCount-1);
//                // Log exception coordinates and stacktrace
//                Utils.Log("\nException in {} at line {}, column {}\n{}", 
//                          frame.GetFileName(), frame.GetFileLineNumber(), frame.GetFileColumnNumber(), 
//                          st.ToString());
//            }
//        }
    }

    class NamedStopwatch
    {
        readonly Stopwatch sw = new Stopwatch();
        readonly string name;

        public NamedStopwatch(string name)
        { this.name = name; }

        public double ElapsedSecs 
        { get { return sw.ElapsedTicks/(double)System.Diagnostics.Stopwatch.Frequency; } }

        public void Start()
        {
            Utils.Log("{}: start counting time", name);
            sw.Start();
        }

        public void Stamp()
        {
            Utils.Log("{}: elapsed time: {}us", name, 
                      sw.ElapsedTicks/(Stopwatch.Frequency/(1000000L)));
        }

        public void Stop() { sw.Stop(); Stamp(); }

        public void Reset() { sw.Stop(); sw.Reset(); }
    }

    class AT_Profiler
    {
        class Counter
        {
            public long Total { get; protected set; }
            public uint Count { get; protected set; }

            public void Add(long val) { Total += val; Count++; }
            public virtual void Add(Counter c) {}
            public virtual double Avg { get { return (double)Total/Count; } }
            public override string ToString() { return string.Format("avg: {0}us", Avg); }
        }

        class SumCounter : Counter
        {
            double avg;
            public override double Avg { get { return avg; } }

            public override void Add(Counter c)
            {
                Count = (uint)Mathf.Max(Count, c.Count);
                avg += c.Avg;
            }
        }

        readonly System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        readonly Dictionary<string, Counter> counters = new Dictionary<string, Counter>();

        long last = 0;

        public void Start() { sw.Stop(); sw.Reset(); sw.Start(); last = 0; }

        public void Log(string name)
        {
            var current = sw.ElapsedTicks/(Stopwatch.Frequency/(1000000L));
            Counter v;
            if(counters.TryGetValue(name, out v)) 
                v.Add(current-last);
            else 
            {
                v = new Counter();
                v.Add(current-last);
                counters[name] = v;
            }
            last = current;
        }

        static void make_report(List<KeyValuePair<string,Counter>> clist)
        {
            var report = "\nName, NumCalls, Avg.Time (us)\n";
            foreach(var c in clist)
                report += string.Format("{0}, {1}, {2}\n", c.Key, c.Value.Count, c.Value.Avg);
            Utils.Log("Profiler Report:{}", report);
        }

        public void PlainReport()
        {
            var clist = counters.ToList();
            clist.Sort((a, b) => b.Value.Avg.CompareTo(a.Value.Avg));
            make_report(clist);
        }

        public void TreeReport()
        {
            var cum_counters = new Dictionary<string, Counter>();
            foreach(var c in counters)
            {
                cum_counters[c.Key] = c.Value;
                var names = c.Key.Split(new []{'.'});
                var cname = "";
                for(int i = 0; i < names.Length-1; i++)
                {
                    cname += "."+names[i];
                    Counter v;
                    if(cum_counters.TryGetValue(cname, out v)) v.Add(c.Value);
                    else 
                    {
                        v = new SumCounter();
                        v.Add(c.Value);
                        cum_counters[cname] = v;
                    }
                }
            }
            var clist = cum_counters.ToList();
            clist.Sort((a, b) => a.Key.CompareTo(b.Key));
            make_report(clist);
        }
    }

    class DebugCounter
    {
        int count = 0;
        string name = "";
        public DebugCounter(string name = "Debug", params object[] args) { this.name = string.Format(name, args); }
        public void Log(string msg="", params object[] args) 
        { 
            if(msg == "") Utils.Log("{}: {}", name, count++); 
            else Utils.Log("{}: {} {}", name, count++, string.Format(msg, args)); 
        }
        public void Reset() { count = 0; }
    }

    public class DebugModuleRCS : ModuleRCS
    {
        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            this.Log("ThrusterTransforms:\n{}",
                     thrusterTransforms.Aggregate("", (s, t) => s+t.name+": "+t.position+"\n"));
        }

        public new void FixedUpdate()
        {
            base.FixedUpdate();
            this.Log("Part: enabled {}, shielded {}, controllable {}", 
                     enabled, part.ShieldedFromAirstream, part.isControllable);
            if(thrustForces.Length > 0)
            {
                this.Log("ThrustForces:\n{}",
                         thrustForces.Aggregate("", (s, f) => s+f+", "));
                this.Log("FX.Power:\n{}",
                         thrusterFX.Aggregate("", (s, f) => s+f.Power+", "+f.Active+"; "));
            }
        }
    }

    public class TemperatureReporter : PartModule
    {
        [KSPField(isPersistant=false, guiActiveEditor=true, guiActive=true, guiName="T", guiUnits = "C")]
        public float temperatureDisplay;

        public override void OnUpdate()
        { temperatureDisplay = (float)part.temperature; }
    }
}
#endif
