using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AT_Utils
{
    public static class VesselExtensions
    {
        public static void Log(this Vessel v, string msg) => Utils.Log($"{v.GetID()}: {msg}");
        public static void Debug(this Vessel v, string msg) => Utils.Debug($"{v.GetID()}: {msg}");
        public static void Info(this Vessel v, string msg) => Utils.Info($"{v.GetID()}: {msg}");
        public static void Warning(this Vessel v, string msg) => Utils.Warning($"{v.GetID()}: {msg}");
        public static void Error(this Vessel v, string msg) => Utils.Error($"{v.GetID()}: {msg}");

        public static void Log(this Vessel v, string msg, params object[] args) =>
            Utils.Log($"{v.GetID()}: {msg}", args);

        public static bool InOrbit(this Vessel v) =>
            !v.LandedOrSplashed
            && (v.situation == Vessel.Situations.ORBITING
                || v.situation == Vessel.Situations.SUB_ORBITAL
                || v.situation == Vessel.Situations.ESCAPING);

        public static bool OnPlanet(this Vessel v) =>
            v.LandedOrSplashed
            || v.situation != Vessel.Situations.ORBITING
            && v.situation != Vessel.Situations.ESCAPING
            || v.orbit.radius < v.orbit.MinPeR();

        public static bool HasLaunchClamp(this IShipconstruct ship)
        {
            foreach(Part p in ship.Parts)
            {
                if(p.HasModule<LaunchClamp>())
                    return true;
            }
            return false;
        }

        public static void Unload(this ShipConstruct construct)
        {
            if(construct == null)
                return;
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

        public static Vector3[] uniqueVertices(this Mesh m)
        {
            var v_set = new HashSet<Vector3>(m.vertices);
            var new_verts = new Vector3[v_set.Count];
            v_set.CopyTo(new_verts);
            return new_verts;
        }

        static Bounds Bounds(this Part p, Transform refT, ref Bounds b, ref bool inited)
        {
            var part_rot = p.partTransform.rotation;
            p.partTransform.rotation = Quaternion.identity;
            foreach(var rend in p.FindModelComponents<Renderer>())
            {
                if(rend.gameObject == null
                   || !(rend is MeshRenderer || rend is SkinnedMeshRenderer))
                    continue;
                var verts = Utils.BoundCorners(rend.bounds);
                for(int j = 0, len = verts.Length; j < len; j++)
                {
                    var v = p.partTransform.position
                            + part_rot * (verts[j] - p.partTransform.position);
                    if(refT != null)
                        v = refT.InverseTransformPoint(v);
                    if(inited)
                        b.Encapsulate(v);
                    else
                    {
                        b.center = v;
                        inited = true;
                    }
                }
            }
            p.partTransform.rotation = part_rot;
            return b;
        }

        public static Bounds Bounds(this Part p, Transform refT)
        {
            var b = new Bounds();
            var inited = false;
            return p.Bounds(refT, ref b, ref inited);
        }

        public static Bounds Bounds(this IShipconstruct vessel, Transform refT = null)
        {
            //update physical bounds
            var b = new Bounds();
            var inited = false;
            var parts = vessel.Parts;
            for(int i = 0, partsCount = parts.Count; i < partsCount; i++)
            {
                var p = parts[i];
                if(p != null)
                    p.Bounds(refT, ref b, ref inited);
            }
            return b;
        }

        public static Bounds EnginesExhaust(this Vessel vessel, Transform refT)
        {
            var CoM = vessel.CurrentCoM;
            var b = new Bounds();
            var inited = false;
            for(int i = 0, vesselPartsCount = vessel.Parts.Count; i < vesselPartsCount; i++)
            {
                var p = vessel.Parts[i];
                var engines = p.Modules.GetModules<ModuleEngines>();
                for(int j = 0, enginesCount = engines.Count; j < enginesCount; j++)
                {
                    var e = engines[j];
                    if(!e.exhaustDamage)
                        continue;
                    for(int k = 0, tCount = e.thrustTransforms.Count; k < tCount; k++)
                    {
                        var t = e.thrustTransforms[k];
                        var term =
                            refT.InverseTransformDirection(
                                t.position + t.forward * e.exhaustDamageMaxRange - CoM);
                        if(inited)
                            b.Encapsulate(term);
                        else
                        {
                            b = new Bounds(term, Vector3.zero);
                            inited = true;
                        }
                    }
                }
            }
            return b;
        }

        public static Bounds BoundsWithExhaust(this Vessel vessel, Transform refT)
        {
            var b = vessel.Bounds(refT);
            b.Encapsulate(vessel.EnginesExhaust(refT));
            return b;
        }

        public static float Radius(this Vessel vessel, bool fromCoM = false)
        {
            if(!vessel.loaded)
            {
                if(vessel.vesselType == VesselType.SpaceObject)
                {
                    var ast = vessel.protoVessel.protoPartSnapshots
                        .Select(p => p.FindModule("ModuleAsteroid"))
                        .FirstOrDefault();
                    if(ast != null)
                    {
                        float rho;
                        if(float.TryParse(ast.moduleValues.GetValue("density"), out rho))
                            return (float)Math.Pow(vessel.GetTotalMass() / rho, 1 / 3.0);
                    }
                }
                return (float)Math.Pow(vessel.GetTotalMass() / 2, 1 / 3.0);
            }
            var refT = vessel.packed ? vessel.transform : vessel.ReferenceTransform;
            var bounds = vessel.BoundsWithExhaust(refT);
            if(fromCoM)
            {
                var shift = refT.TransformPoint(bounds.center) - vessel.CoM;
                return bounds.extents.magnitude + shift.magnitude;
            }
            return bounds.extents.magnitude;
        }

        #region VesselRanges
        private static readonly List<FieldInfo> situation_ranges = typeof(VesselRanges)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(fi => fi.FieldType == typeof(VesselRanges.Situation))
            .ToList();

        public static VesselRanges SetUnpackDistance(this Vessel vessel, float distance, bool ifGreater = false)
        {
            var doNotCompare = !ifGreater;
            var pack = distance * 1.5f;
            var unpack = distance;
            var load = distance * 2f;
            var unload = distance * 2.5f;
            var orig_ranges = new VesselRanges(vessel.vesselRanges);
            foreach(var fi in situation_ranges)
            {
                if(!(fi.GetValue(vessel.vesselRanges) is VesselRanges.Situation sit))
                    continue;
                if(doNotCompare || sit.pack < pack)
                    sit.pack = pack;
                if(doNotCompare || sit.unpack < unpack)
                    sit.unpack = unpack;
                if(doNotCompare || sit.unload < unload)
                    sit.unload = unload;
                if(doNotCompare || sit.load < load)
                    sit.load = load;
            }
            return orig_ranges;
        }
        #endregion
    }
}
